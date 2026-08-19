using UnityEngine;

/// <summary>
/// Time Echo 的生命值组件。
/// 
/// 为什么覆写 Die：
/// 克隆体死亡时不能像玩家/敌人那样停在原地，
/// 要通知 SkillObject_TimeEcho 执行"变成精灵或销毁"的逻辑。
/// </summary>
public class SkillObject_Health : Entity_Health
{
    protected override void Die()
    {
        SkillObject_TimeEcho timeEcho = GetComponent<SkillObject_TimeEcho>();
        timeEcho.HandleDeath();
    }
}
