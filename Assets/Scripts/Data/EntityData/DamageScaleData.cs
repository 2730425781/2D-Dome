using System;
using UnityEngine;

/// <summary>
/// 技能升级对伤害和元素效果的缩放配置。
/// 
/// 为什么单独一个类：
/// 每个技能升级（UpgradeDate）可以携带自己的伤害缩放，
/// 这样同一个技能在不同升级分支下打出的伤害/状态效果可以不同，
/// 数值全部在 Inspector 中配置，不需要写死在代码里。
/// </summary>
[Serializable]
public class DamageScaleData
{
    [Header("伤害效果设置")]
    public float physical = 1f;      // 物理伤害倍率
    public float elemental = 1f;     // 元素伤害倍率

    [Header("冰冻效果设置")]
    public float chillDurationScale = 3f;  // 冰冻持续时间
    public float chillSlowScale = 0.3f;    // 冰冻减速幅度

    [Header("燃烧效果设置")]
    public float burnDurationScale = 3f;   // 燃烧持续时间
    public float burnDamageScale = 0.8f;   // 燃烧总伤害 = 火焰伤害 * 此倍率

    [Header("雷电效果设置")]
    public float shockDurationScale = 3f;  // 感电持续时间
    public float shockDamageScale = 1f;    // 感电伤害倍率
    public float shockChargeScale = 0.35f; // 每次命中累积的感电充能
}
