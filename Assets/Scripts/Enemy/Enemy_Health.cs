using UnityEngine;

/// <summary>
/// 敌人生命值组件。
/// 
/// 覆写 TakeDamage 的原因：
/// 基础生命逻辑（扣血、击退、死亡）在 Entity_Health 中已经实现了，
/// 但敌人受伤后需要额外触发战斗行为（转向攻击者），
/// 所以在伤害来源是 Player 时调用 TryToBattle。
/// </summary>
public class Enemy_Health : Entity_Health
{
    private Enemy enemy => GetComponent<Enemy>();

    public override void TakeDamage(float damage, Transform damageDealer)
    {
        base.TakeDamage(damage, damageDealer);
        if (isDead)
        {
            return;
        }
        // 被玩家攻击时进入战斗状态
        if (damageDealer.CompareTag("Player"))
        {
            enemy.TryToBattle(damageDealer);
        }
    }
}