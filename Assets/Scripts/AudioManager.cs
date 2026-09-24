using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频管理器：按名称从音频数据库取一段随机 clip 并播放。
///
/// 为什么是持久化单例：切场景时音乐/音效配置不该重新建立，DontDestroyOnLoad 让它在关卡间常驻。
///
/// 它同时是**音量设置的唯一持有者**：
/// BGM/SFX 音量在这里读写全局设置文件 AudioSettings.json，并统一换算成 dB 写进 AudioMixer。
/// 注意它刻意**不实现 ISaveable**——ISaveable 的数据是"每个存档槽一份"的 GameData，
/// 而音量是玩家偏好设置，换存档不该跟着变，所以它走独立的全局文件。
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // 全局音频设置文件名。与存档槽文件（YS_1.json、YS_2.json…）放在同一个 persistentDataPath 下，
    // 但不带槽位号，因此所有存档共用同一份音量设置
    private const string SettingsFileName = "AudioSettings.json";

    // 计算 dB 时的滑块下限：Mathf.Log10(0) 是 -Infinity，必须先夹住
    private const float MinSliderValue = 0.0001f;

    [SerializeField] private AudioDataBaseSO audioDataBase;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [Space]
    [SerializeField] private bool shouldPlayBgm;

    [Header("音量设置（写入全局 AudioSettings.json）")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string bgmParameter = "bgmMixer";
    [SerializeField] private string sfxParameter = "sfxMixer";
    [Tooltip("滑块值换算成 dB 的系数：dB = Log10(滑块值) * 系数")]
    [SerializeField] private float mixerMultiplier = 25f;
    [Tooltip("勾选后设置文件会做异或加密；默认不加密，便于直接打开文件排查问题")]
    [SerializeField] private bool encryptSettings;

    /// <summary>当前 BGM 滑块值（0~1）。设置面板读它来同步滑块位置。</summary>
    public float BgmVolume { get; private set; } = 1f;

    /// <summary>当前 SFX 滑块值（0~1）。</summary>
    public float SfxVolume { get; private set; } = 1f;

    /// <summary>音量变化时触发，供已打开的设置面板同步滑块。</summary>
    // 用完全限定名 System.Action，避免为了 Action 引入 using System，
    // 那会让下面的 Random.Range 与 System.Random 产生二义性
    public event System.Action OnVolumeChanged;

    private FileDataHandler<AudioSettingsData> settingsHandler;
    private Coroutine saveSettingsCo;
    private bool warnedMissingMixer;

    private string currentBgmGroupName;
    private Coroutine currentBgmCo;
    private AudioClip lastMusicPlayed;
    private Player player;

    private void Awake()
    {
        // 原条件写成 instance == null && instance != this，逻辑是反的：
        // 首个实例进入时 instance 还是 null，"null != this" 成立 → 条件为真 →
        // 它把自己 Destroy 掉并 return，instance 永远不会被赋值。
        // 结果 AudioManager.instance 恒为 null，所有 AudioManager.instance.PlaySFX(...)
        // 都会在调用点直接抛 NullReferenceException（音效全都不响）。
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 读取全局音量设置并立刻写入混音器。
        // 放在 Awake 而不是等某个设置面板 Start：这样无论从哪个场景启动、场景里有没有设置面板，
        // 音量都会是玩家上次设定的值；设置面板只需要回头读 BgmVolume/SfxVolume 显示即可。
        settingsHandler = new FileDataHandler<AudioSettingsData>(
            Application.persistentDataPath, SettingsFileName, encryptSettings);

        LoadAudioSettings();

        // 但混音器在场景初始化过程中还会套用它自己的默认快照，把上面 Awake 期间写入的值覆盖掉
        // （实测：Awake 里写入 -9.9 dB，进入游戏后读回来仍是 0.0 dB）。
        // 所以等一帧之后再补写一次，确保读到的设置真正作用到声音上。
        StartCoroutine(ApplyVolumeAfterInitCo());
    }

    /// <summary>
    /// 等一帧后重新应用音量。
    /// 为什么需要单独来这一下：AudioMixer 的默认快照是在场景初始化阶段应用的，
    /// 时机晚于 Awake，会把 Awake 中 SetFloat 的值冲掉——
    /// 表现为"设置文件读到了、混音器却没变化"，音量看起来没有恢复。
    /// </summary>
    private IEnumerator ApplyVolumeAfterInitCo()
    {
        yield return null;

        ApplyMixerVolume(bgmParameter, BgmVolume);
        ApplyMixerVolume(sfxParameter, SfxVolume);
    }

    // ---------- 音量设置（保存 / 读取） ----------

    /// <summary>设置 BGM 音量。参数是滑块值(0~1)：写入混音器并落盘。</summary>
    public void SetBgmVolume(float sliderValue)
    {
        BgmVolume = Mathf.Clamp01(sliderValue);
        ApplyMixerVolume(bgmParameter, BgmVolume);
        SaveAudioSettings();
    }

    /// <summary>设置 SFX 音量。参数是滑块值(0~1)：写入混音器并落盘。</summary>
    public void SetSfxVolume(float sliderValue)
    {
        SfxVolume = Mathf.Clamp01(sliderValue);
        ApplyMixerVolume(sfxParameter, SfxVolume);
        SaveAudioSettings();
    }

    /// <summary>把滑块值换算成 dB 写入混音器的暴露参数。</summary>
    private void ApplyMixerVolume(string parameter, float sliderValue)
    {
        if (audioMixer == null)
        {
            // 只警告一次，避免拖滑块时刷屏
            if (!warnedMissingMixer)
            {
                warnedMissingMixer = true;
                Debug.LogWarning("AudioManager 未配置 AudioMixer，音量设置无法作用到实际声音（数值仍会被保存）");
            }
            return;
        }

        if (string.IsNullOrEmpty(parameter)) return;

        // 先夹下限再取对数：Mathf.Log10(0) = -Infinity，
        // 滑块拖到最左（0）时直接计算会写入非法 dB，混音器参数会失效（表现为音量失控）
        float db = Mathf.Log10(Mathf.Max(sliderValue, MinSliderValue)) * mixerMultiplier;
        audioMixer.SetFloat(parameter, db);
    }

    /// <summary>从全局设置文件读取音量；文件不存在（首次运行）时用默认满音量。</summary>
    private void LoadAudioSettings()
    {
        AudioSettingsData data = settingsHandler.LoadData();
        if (data == null)
        {
            data = new AudioSettingsData();
        }

        BgmVolume = Mathf.Clamp01(data.bgmVolume);
        SfxVolume = Mathf.Clamp01(data.sfxVolume);

        ApplyMixerVolume(bgmParameter, BgmVolume);
        ApplyMixerVolume(sfxParameter, SfxVolume);
    }

    /// <summary>请求保存音量设置（带防抖，见 SaveSettingsCo）。</summary>
    private void SaveAudioSettings()
    {
        // 先通知 UI，界面响应不该等落盘
        OnVolumeChanged?.Invoke();

        // 拖动滑块时 Slider.onValueChanged 会连续触发几十次，
        // 每次都写文件等于每帧一次主线程 I/O → 防抖到"停下来 0.5 秒后"再落盘
        if (saveSettingsCo != null)
        {
            StopCoroutine(saveSettingsCo);
        }

        saveSettingsCo = StartCoroutine(SaveSettingsCo());
    }

    private IEnumerator SaveSettingsCo()
    {
        // 用 Realtime 而不是 WaitForSeconds：设置面板在暂停菜单里，
        // 此时 Time.timeScale = 0，用缩放时间会永远等不到，设置就存不进去
        yield return new WaitForSecondsRealtime(0.5f);

        saveSettingsCo = null;
        WriteSettingsFile();
    }

    private void WriteSettingsFile()
    {
        settingsHandler.SaveData(new AudioSettingsData
        {
            bgmVolume = BgmVolume,
            sfxVolume = SfxVolume
        });
    }

    // 退出游戏时补一次写入：防止"拖完滑块立刻退出"落在防抖窗口内而丢设置
    private void OnApplicationQuit()
    {
        if (settingsHandler == null) return;

        WriteSettingsFile();
    }

    private void Update()
    {
        if (!bgmSource.isPlaying && shouldPlayBgm)
        {
            if (!string.IsNullOrEmpty(currentBgmGroupName))
            {
                NextBGM(currentBgmGroupName);
            }
        }

        if (bgmSource.isPlaying && !shouldPlayBgm)
        {
            StopBGM();
        }
    }

    public void StartBGM(string musicGroup)
    {
        shouldPlayBgm = true;

        if (musicGroup == currentBgmGroupName)
        {
            return;
        }

        NextBGM(musicGroup);
    }

    public void NextBGM(string musicGroup)
    {
        shouldPlayBgm = true;
        currentBgmGroupName = musicGroup;

        if (currentBgmCo != null)
        {
            StopCoroutine(currentBgmCo);
        }

        currentBgmCo = StartCoroutine(SwitchMusicCo(musicGroup));
    }

    public void StopBGM()
    {
        shouldPlayBgm = false;

        StartCoroutine(FadeVolumeCo(bgmSource, 0, 1f));

        if (currentBgmCo != null)
        {
            StopCoroutine(currentBgmCo);
        }
    }

    private IEnumerator SwitchMusicCo(string musicGroup)
    {
        AudioClipData data = audioDataBase.GetAudioData(musicGroup);
        AudioClip nextMusic = data.GetRandomClip();

        if (data == null || data.clips.Count == 0)
        {
            Debug.Log("该音频文件组未找到对应的音频文件" + musicGroup);
            yield break;
        }
        if (data.clips.Count > 1)
        {
            while (nextMusic == lastMusicPlayed)
            {
                nextMusic = data.GetRandomClip();
            }
        }

        if (bgmSource.isPlaying)
        {
            yield return FadeVolumeCo(bgmSource, 0, 1f);
        }

        lastMusicPlayed = nextMusic;
        bgmSource.clip = nextMusic;
        bgmSource.volume = 0;
        bgmSource.Play();

        StartCoroutine(FadeVolumeCo(bgmSource, data.volume, 1f));
    }

    private IEnumerator FadeVolumeCo(AudioSource audio, float targetVolume, float duration)
    {
        float time = 0;
        float startVolume = audio.volume;

        while (time < duration)
        {
            time += Time.deltaTime;
            audio.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        audio.volume = targetVolume;
    }

    /// <summary>
    /// 播放音效。audioName 必须与音频数据库里的 audioName 完全一致
    /// （Entity_SFX 传的是 PlayerSFXType 枚举成员名，如 "AttackMissSFX"）。
    /// </summary>
    public void PlaySFX(string audioName, AudioSource targetSource, float minDistanceToHear = 10)
    {
        if (player == null)
        {
            player = Player.instance;
        }

        if (audioDataBase == null)
        {
            Debug.LogWarning("AudioManager 未配置音频数据库，无法播放音效：" + audioName);
            return;
        }

        // 播放源由调用方传入（各实体自己的 AudioSource）。
        // 不判空的话，实体没挂 AudioSource 时会在 PlayOneShot 处空引用
        if (targetSource == null)
        {
            Debug.LogWarning("PlaySFX 收到空的 AudioSource，无法播放音效：" + audioName);
            return;
        }

        var data = audioDataBase.GetAudioData(audioName);
        if (data == null)
        {
            // 用 Warning 而不是 Log：音频名与数据库键对不上时要能一眼看见（这是最常见的失声原因）
            Debug.LogWarning("音频数据库中找不到音效：" + audioName);
            return;
        }

        var clip = data.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning("音效 " + audioName + " 没有配置任何 AudioClip");
            return;
        }

        // ---------- 距离衰减 ----------
        // player 可能为 null：主菜单这类场景里没有 Player。
        // 原写法直接取 player.transform.position，在没有玩家的场景调用 PlaySFX 就会空引用崩溃；
        // 拿不到玩家时按"距离为 0"处理，即以满音量播放。
        float t = 1f;
        if (player != null && minDistanceToHear > 0f)
        {
            float distance = Vector2.Distance(targetSource.transform.position, player.transform.position);
            t = Mathf.Clamp01(1f - (distance / minDistanceToHear));
        }

        float targetVolume = Mathf.Lerp(0f, data.volume, t);

        targetSource.pitch = Random.Range(0.8f, 1.2f);

        // 直接赋值，不要写成 Mathf.Lerp(targetSource.volume, targetVolume, Time.deltaTime * 3)：
        // PlaySFX 是"每次播放才调用一次"，插值每调用一次只推进 Time.deltaTime*3 ≈ 5%，
        // 连按几十次都到不了目标音量，距离衰减等于没生效；
        // 而且把 AudioSource.volume 当作插值状态，会让上一次播放的音量残留影响下一次。
        // 每个实体有自己的 AudioSource，这里直接写入本次应有的音量即可。
        // 需要"逐帧平滑衰减"的持续音源（循环音/环境音）请用 AudioRangeController。
        targetSource.volume = targetVolume;

        // 把 clip 写进 AudioSource.clip 再 PlayOneShot。
        // 为什么不能只调 PlayOneShot：它只是在音频线程上叠加播放一段 one-shot，
        // **不会修改 AudioSource.clip 字段**，于是运行时在 Inspector 里看这个音频源，
        // AudioClip 槽永远是空的，没法确认"当前放的是哪一段"。
        // 赋值这一步只影响 Inspector 显示（以及该源的"默认 clip"），
        // 真正的播放仍由 PlayOneShot 负责，因此快速连击依然能叠加、不会互相打断。
        targetSource.clip = clip;
        targetSource.PlayOneShot(clip);
    }

    public void PlayGlobalSFX(string soundName)
    {
        // 与 PlaySFX 同样的空引用防护：数据库或全局播放源没配好时直接返回，不要崩
        if (audioDataBase == null || sfxSource == null) return;

        var data = audioDataBase.GetAudioData(soundName);
        if (data == null) return;

        var clip = data.GetRandomClip();
        if (clip == null) return;

        sfxSource.pitch = Random.Range(0.8f, 1.2f);
        sfxSource.volume = data.volume;
        // 与 PlaySFX 同理：先写入 clip，运行时才能在 Inspector 里看到正在播放的音效名
        sfxSource.clip = clip;
        sfxSource.PlayOneShot(clip);
    }
}
