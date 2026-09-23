using UnityEngine;

/// <summary>
/// 音频管理器：按名称从音频数据库取一段随机 clip 并播放。
///
/// 为什么是持久化单例：切场景时音乐/音效配置不该重新建立，DontDestroyOnLoad 让它在关卡间常驻。
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDataBaseSO audioDataBase;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

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
