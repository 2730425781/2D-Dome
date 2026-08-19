using UnityEngine;

/// <summary>
/// 敌人动画事件接收器。继承自 Entity_AnimationTriggers 以复用攻击判定。
/// 扩展了反击窗口开关的逻辑：攻击动画的特定帧开启/关闭反击窗口，
/// 这样玩家只有在敌人攻击的特定时刻才能触发反击，增加操作深度。
/// </summary>
public class Enemy_AnimationTriggers : Entity_AnimationTriggers
{
    private Enemy enemy;
    private Enemy_VFX enemy_VFX;

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponentInParent<Enemy>();
        enemy_VFX = GetComponentInParent<Enemy_VFX>();
    }

    /// <summary>
    /// 由攻击动画中的事件调用，开启反击窗口。
    /// 同时显示视觉提示（攻击预警），让玩家知道现在可以反击。
    /// </summary>
    private void EnableCounterWindow()
    {
        enemy.EnableCounterWindow(true);
        enemy_VFX.EnableAttackAlert(true);
    }

    /// <summary>
    /// 关闭反击窗口并隐藏视觉提示。
    /// </summary>
    private void DisableCounterWindow()
    {
        enemy.EnableCounterWindow(false);
        enemy_VFX.EnableAttackAlert(false);
    }
}
