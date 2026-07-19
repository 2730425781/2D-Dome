using UnityEngine;

/// <summary>
/// 挂在角色子物体上的动画事件接收器。
/// 动画片段中插入的事件（如攻击判定帧、反击窗口开关）通过此类转发给逻辑层。
/// 
/// 为什么单独挂在一个子物体上而不是在角色主物体上接收事件：
/// 1. 动画事件只能发送给挂载了 Animator 的游戏对象或它的子物体
/// 2. 子物体上挂这个脚本，主物体上的 Entity/Entity_ComBat 保持独立职责
/// </summary>
public class Entity_AnimationTriggers : MonoBehaviour
{
    private Entity entity;
    private Entity_ComBat entityComBat;

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();
        entityComBat = GetComponentInParent<Entity_ComBat>();
    }

    /// <summary>
    /// 由 Animation Event 调用，通知当前状态的 triggerCalled = true。
    /// </summary>
    private void CurrentStateTrigger()
    {
        entity.CurrentStateAnimationTrigger();
    }

    /// <summary>
    /// 由攻击动画中的事件调用，执行伤害检测。
    /// 攻击判定只在动画的特定帧触发，而不是每帧检测。
    /// </summary>
    private void AttackTrigger()
    {
        entityComBat.PerformAttack();
    }
}