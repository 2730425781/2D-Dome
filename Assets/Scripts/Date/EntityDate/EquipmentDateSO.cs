using System;
using UnityEngine;

/// <summary>
/// 装备数据：在物品数据的基础上增加属性修正（modifiers）。
/// 为什么装备用“属性数组”而不是每个属性一个字段：
/// 任意组合的属性加成只需往数组里加一项，新增属性类型不用改这个类，
/// 也方便将来做随机词条装备时直接复用同一结构。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/装备数据", fileName = "装备数据设置-")]
public class EquipmentDateSO : ItemDateSO
{
    [Header("物品数据设置")]
    public ItemModifier[] modifiers;
}

/// <summary>
/// 单个属性修正：指定改哪个属性、加多少。
/// 为什么是 [Serializable] 普通类而不是 ScriptableObject：
/// 它是 EquipmentDateSO 资源上内嵌的数组元素，需要在 Inspector 里逐项编辑。
/// </summary>
[Serializable]
public class ItemModifier
{
    public StatType statType;
    public float value;
}
