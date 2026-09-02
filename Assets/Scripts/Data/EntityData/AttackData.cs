using UnityEngine;

/// <summary>
/// 一次攻击的完整结算数据。
/// 
/// 为什么用"数据结构"而不是直接返回多个值：
/// 技能实体（剑、碎片、Time Echo）需要同时拿到物理伤害、元素伤害、
/// 是否暴击、元素类型和状态效果参数，把它们打包成 AttackDate，
/// 调用方一次就能拿到所有需要的数据。
/// </summary>
public class AttackDate
{
    public float physcalDamage;
    public float elementDamage;
    public bool isCrit;
    public ElementType element;

    public ElementalEffectData effectDate;

    public AttackDate(Entity_Stats stats, DamageScaleData damageScale)
    {
        // 用玩家当前属性 + 技能升级的伤害缩放系数计算本次伤害
        physcalDamage = stats.GetPhysicalDamage(out isCrit, damageScale.physical);
        elementDamage = stats.GetElementalDamage(out element, damageScale.elemental);

        effectDate = new ElementalEffectData(stats, damageScale);
    }
}
