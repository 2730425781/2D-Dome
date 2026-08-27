using System;
using UnityEngine;

/// <summary>
/// 攻击类属性分组：物理（攻速/伤害/暴击/穿透）+ 元素附加伤害。
/// 物理与元素分开存放，是因为伤害计算走两条独立路径：
/// GetPhysicalDamage() 只读物理字段，GetElementalDamage() 从三个元素伤害里取最高值。
/// 暴击率/暴击威力/护甲穿透都是百分比（0-100），在公式中除以 100 后使用。
/// </summary>
[Serializable]
public class Stat_OffenseGroup
{
    [InspectorName("攻击速度")]
    [Tooltip("攻击的频率")]
    // 攻击频率，决定攻击动画/连段的节奏
    public Stat attackSpeed;

    [Header("物理伤害")]
    [InspectorName("基础伤害")]
    [Tooltip("普通攻击的伤害值")]
    // 物理基础伤害，最终值还叠加力量（见 GetBaseDamage）
    public Stat damage;

    [InspectorName("暴击伤害")]
    [Tooltip("暴击时伤害的倍率加成")]
    // 暴击倍率（百分比），GetPhysicalDamage 中除以 100 后作为乘数
    public Stat critPower;

    [InspectorName("暴击率")]
    [Tooltip("触发暴击的几率")]
    // 暴击几率（0-100），与 Random.Range(0, 100) 比较判定本次攻击是否暴击
    public Stat critChance;

    [InspectorName("护甲穿透")]
    [Tooltip("无视百分比护甲的效果")]
    // 护甲穿透（百分比），GetArmorMitigation 中按比例削减对方有效护甲
    public Stat armorReduction;

    [Header("元素伤害")]
    [InspectorName("火焰伤害")]
    [Tooltip("火焰属性附加伤害")]
    // 元素附加伤害，GetElementalDamage 只取三者中的最高值作为攻击元素
    public Stat fireDamage;

    [InspectorName("冰冻伤害")]
    [Tooltip("冰冻属性附加伤害")]
    // 同上，若高于火焰/雷电则本次攻击按冰元素结算
    public Stat iceDamage;

    [InspectorName("雷电伤害")]
    [Tooltip("雷电属性附加伤害")]
    // 同上，若高于火焰/冰冻则本次攻击按雷元素结算
    public Stat lightningDamage;
}