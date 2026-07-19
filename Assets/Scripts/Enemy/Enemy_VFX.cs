using UnityEngine;

/// <summary>
/// 敌人视觉特效组件。在 Entity_VFX 的基础上增加反击窗口视觉提示。
/// attackAlert 是一个 GameObject（例如感叹号图标），
/// 在反击窗口开启时显示，提醒玩家现在可以按反击。
/// </summary>
public class Enemy_VFX : Entity_VFX
{
    [Header("反击窗口")]
    [SerializeField] private GameObject attackAlert;

    public void EnableAttackAlert(bool enable) => attackAlert.SetActive(enable);
}