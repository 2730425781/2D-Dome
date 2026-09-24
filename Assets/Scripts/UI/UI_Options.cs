using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置面板 UI（主菜单与暂停菜单各有一份）。
///
/// 这里只负责"显示与转发"：
/// 打开时从 AudioManager 读当前音量回显滑块位置，拖动时把值交给 AudioManager。
/// 真正的保存/读取、以及写入 AudioMixer 都在 AudioManager 里完成，
/// 避免主菜单和暂停菜单两份面板各自持有一份音量状态而互相覆盖。
/// </summary>
public class UI_Options : MonoBehaviour
{
    [Header("背景音设置")]
    [SerializeField] private Slider bgmSlider;

    [Header("音效设置")]
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        // 面板每次打开都按当前音量回显滑块位置（面板可能被关掉过，期间音量在别处改过）
        RefreshSliders();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.OnVolumeChanged += RefreshSliders;
        }
    }

    private void OnDisable()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.OnVolumeChanged -= RefreshSliders;
        }
    }

    /// <summary>把 AudioManager 中当前的音量写回滑块显示。</summary>
    private void RefreshSliders()
    {
        AudioManager manager = AudioManager.instance;
        if (manager == null) return;

        // 用 SetValueWithoutNotify：否则"回显滑块"会反过来触发 onValueChanged，
        // 又去写一次设置，两边来回覆盖
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(manager.BgmVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(manager.SfxVolume);
    }

    // 下面两个方法由 Slider 的 OnValueChanged 在 Inspector 里绑定
    public void BGMSliderValue(float value)
    {
        if (AudioManager.instance == null) return;

        AudioManager.instance.SetBgmVolume(value);
    }

    public void SFXSliderValue(float value)
    {
        if (AudioManager.instance == null) return;

        AudioManager.instance.SetSfxVolume(value);
    }

    public void GoMainMenuBTN()
    {
        GameManager.instance.ChangeScene("MainMenu", RespawnType.None);
    }
}
