using UnityEngine;

/// <summary>
/// 敌人生命值组件。
/// 
/// 覆写 TakeDamage 的原因：
/// 基础生命逻辑（扣血、击退、死亡）在 Entity_Health 中已经实现了，
/// 但敌人受伤后需要额外触发战斗行为（转向攻击者）。
/// 
/// 伤害来源检测逻辑：
/// 1. 伤害来源是玩家 → 直接锁定该玩家进入战斗
/// 2. 伤害来源不是玩家（Shard 爆炸、陷阱等）或已销毁 →
///    在圆形范围内查找玩家，找到则进入战斗状态
/// </summary>
public class Enemy_Health : Entity_Health
{
    private Enemy enemy => GetComponent<Enemy>();

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (!canTakeDamage)
        {
            return false;
        }

        bool wasHit = base.TakeDamage(damage, elementalDamage, element, damageDealer);
        //Debug.Log("敌人受到了" + damage + "点物理伤害  " + elementalDamage + "点" + element + "元素伤害");
        if (!wasHit) return false;

        // 直接由玩家造成的伤害 → 立即锁定该玩家进入战斗
        if (damageDealer != null && damageDealer.CompareTag("Player"))
        {
            enemy.TryToBattle(damageDealer);
            return true;
        }

        // 间接伤害（Shard 爆炸、陷阱等）没有可靠的玩家来源：
        // 在圆形范围内找玩家，避免敌人因找不到伤害来源而卡在战斗状态
        Transform player = enemy.FindPlayerInRadius();
        if (player != null)
        {
            enemy.TryToBattle(player);
        }
        return true;
    }
}
