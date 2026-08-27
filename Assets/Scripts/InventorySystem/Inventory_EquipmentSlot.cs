using System;
using UnityEngine;

/// <summary>
/// 装备槽：每个槽绑定一个 ItemType，用来约束“这类装备只能穿在对应部位”，
/// 防止任意部位互穿。做成可序列化普通类并放进 equipList，是为了在 Inspector 中按玩法灵活配置槽位，
/// 而不是把部位写死在代码里。HasItem() 同时检查物品及其数据，避免“有物品但数据被清空”的脏状态。
/// </summary>
[Serializable]
public class Inventory_EquipmentSlot
{
    public ItemType slotType;
    public Inventory_Item equipedItem;

    public bool HasItem() => equipedItem != null && equipedItem.itemDate != null;
}
