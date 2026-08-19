using System;
using UnityEngine;

[Serializable]
public class Stat_OffenseGroup
{
    [InspectorName("攻击速度")]
    [Tooltip("攻击的频率")]
    public Stat attackSpeed;

    [Header("物理伤害")]
    [InspectorName("基础伤害")]
    [Tooltip("普通攻击的伤害值")]
    public Stat damage;

    [InspectorName("暴击伤害")]
    [Tooltip("暴击时伤害的倍率加成")]
    public Stat critPower;

    [InspectorName("暴击率")]
    [Tooltip("触发暴击的几率")]
    public Stat critChance;

    [InspectorName("护甲穿透")]
    [Tooltip("无视百分比护甲的效果")]
    public Stat armorReduction;

    [Header("元素伤害")]
    [InspectorName("火焰伤害")]
    [Tooltip("火焰属性附加伤害")]
    public Stat fireDamage;

    [InspectorName("冰冻伤害")]
    [Tooltip("冰冻属性附加伤害")]
    public Stat iceDamage;

    [InspectorName("雷电伤害")]
    [Tooltip("雷电属性附加伤害")]
    public Stat lightningDamage;
}