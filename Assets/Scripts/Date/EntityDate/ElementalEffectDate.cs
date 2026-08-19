using System;
using UnityEngine;

/// <summary>
/// 元素状态效果的结算参数。
/// 
/// 为什么在构造时就算好所有数值：
/// 技能实体命中时只需要把 effectDate 交给 Entity_StatusHandler 应用，
/// 不需要在命中循环里再查询玩家属性，避免重复计算。
/// </summary>
[Serializable]
public class ElementalEffectDate
{
    public float chillDuration;
    public float chillSlowMultiplier;
    public float burnDuration;
    public float totalBurnDamage;
    public float shockDuration;
    public float shockDamage;
    public float shockCharge;

    public ElementalEffectDate(Entity_Stats stats, DamageScaleDate damageScale)
    {
        // 冰冻/燃烧/感电的持续时间和强度都由技能升级的缩放配置决定
        chillDuration = damageScale.chillDurationScale;
        chillSlowMultiplier = damageScale.chillSlowScale;

        burnDuration = damageScale.burnDurationScale;
        totalBurnDamage = stats.offense.fireDamage.GetValue() * damageScale.burnDamageScale;

        shockDuration = damageScale.shockDurationScale;
        shockDamage = stats.offense.lightningDamage.GetValue() * damageScale.shockDamageScale;
        shockCharge = damageScale.shockChargeScale;
    }
}
