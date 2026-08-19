using UnityEngine;

/// <summary>
/// Time Echo 的动画事件接收器。
/// 
/// 为什么单独挂一个脚本来接收动画事件：
/// Animator 上的动画事件只能发给挂载 Animator 的游戏对象或其子物体，
/// 把事件接收器放在子物体上，主逻辑（SkillObject_TimeEcho）保持独立职责。
/// </summary>
public class SkillObject_AnimationTrigglers : MonoBehaviour
{
    private SkillObject_TimeEcho timeEcho;

    private void Awake()
    {
        timeEcho = GetComponentInParent<SkillObject_TimeEcho>();
    }

    /// <summary>
    /// 动画攻击判定帧调用，执行一次攻击伤害。
    /// </summary>
    private void AttackTrggler()
    {
        timeEcho.PerformAttack();
    }

    /// <summary>
    /// 动画最后一帧调用：攻击次数达到上限后让克隆体消失。
    /// currentAttackIndex 由 Animator 的 int 参数传入。
    /// </summary>
    private void TryTerminate(int currentAttackIndex)
    {
        if (currentAttackIndex == timeEcho.maxAttacks)
        {
            timeEcho.HandleDeath();
        }
    }
}
