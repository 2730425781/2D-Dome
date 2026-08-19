using System;
using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/装备数据", fileName = "装备数据设置-")]
public class EquipmentDateSO : ItemDateSO
{
    [Header("物品数据设置")]
    public ItemModifier[] modifiers;
}

[Serializable]
public class ItemModifier
{
    public StatType statType;
    public float value;
}
