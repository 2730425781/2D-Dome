using System;
using UnityEngine;

/// <summary>
/// 防御类属性分组：物理防御（护甲/闪避）+ 三种元素抗性。
/// 为什么用 [Serializable] 普通类而不是 MonoBehaviour / ScriptableObject：
/// 它只是数据的"收纳盒"，没有生命周期或行为，由 Entity_Stats 序列化持有，
/// 让 Inspector 里属性按组折叠展示、便于整体赋值。
/// 三个抗性字段与 ElementType 一一对应，供 GetElementalResistance() 按元素取值。
/// </summary>
[Serializable]
public class Stat_DefenseGroup
{
    [Header("物理防御")]
    [InspectorName("护甲")]
    [Tooltip("减少受到的物理伤害")]
    // 物理减伤数值，GetArmorMitigation() 会换算成百分比减伤并封顶 85%
    public Stat armor;

    [InspectorName("闪避")]
    [Tooltip("闪避物理攻击的几率")]
    // 闪避几率（0-100），GetEvasion() 会再叠加敏捷加成并封顶 100
    public Stat evasion;

    [Header("元素抗性")]
    [InspectorName("火焰抗性")]
    [Tooltip("减少受到的火焰伤害")]
    // 元素抗性按 ElementType 取值，与智力加成叠加后封顶 75%
    public Stat fireRes;

    [InspectorName("冰冻抗性")]
    [Tooltip("减少受到的冰冻伤害")]
    // 同上，对应 ElementType.Ice
    public Stat iceRes;

    [InspectorName("雷电抗性")]
    [Tooltip("减少受到的雷电伤害")]
    // 同上，对应 ElementType.Lightning
    public Stat lightningRes;
}