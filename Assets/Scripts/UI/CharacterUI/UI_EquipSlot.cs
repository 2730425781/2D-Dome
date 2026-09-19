using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 装备槽位：继承 UI_ItemSlot 复用图标渲染与悬停 ToolTip 逻辑，
/// 只覆写点击行为（点击已穿戴装备 = 卸下）。
/// 为什么不直接改基类：物品槽点击是使用/穿戴，装备槽点击是卸下，
/// 行为相反，用覆写比在基类里堆分支更清晰。
/// slotType 做成公开字段：Inventory_Player.TryEquipItem 需要按类型
/// 找到可穿戴的槽位，因此它与玩家的 equipList 槽位类型一一对应。
/// </summary>
public class UI_EquipSlot : UI_ItemSlot
{
    public ItemType slotType;

    private void OnValidate()
    {
        // 编辑期按槽位类型重命名 GameObject，
        // 让场景层级里一眼看清每个槽位对应哪种装备
        gameObject.name = "装备槽位 - " + slotType.ToString();
    }

    protected override void ExecuteClickAction(PointerEventData eventData)
    {
        // 覆写基类点击逻辑：装备槽的点击含义是"卸下"而不是"使用/穿戴"；
        // 空槽点击直接返回，避免走卸下流程
        if (itemInSlot == null) return;
        inventory.UnequipItem(itemInSlot);
    }

    /// <summary>
    /// 装备槽是"单槽"容器：它挂的物品在 equipList 的某个槽位里，而不是 itemList，
    /// 因此不参与普通物品的可排序列表。返回 null 明确标识它不属于任何列表，
    /// 避免基类把装备当 itemList 里的物品去做重排/交换（IndexOf 找不到会静默失败）。
    /// </summary>
    public override List<Inventory_Item> SlotItemList => null;

    /// <summary>
    /// 拖放：
    /// - 背包(或其它来源)的装备拖入 → 穿戴到匹配类型的槽位。
    ///   TryEquipItem 按 itemDate.itemType 找空槽，满了则替换旧装备。
    /// - 从另一个装备槽拖来 → 该装备不在 itemList 里，TryEquipItem 的 FindItem 找不到它，
    ///   所以先卸下放回背包再穿戴，实现换槽/移动。
    /// - 装备槽之间不经过背包排序，因此也顺带绕开"背包满则拒绝换装"的限制。
    /// - 只接受装备数据，材料等其它类型直接忽略。
    /// </summary>
    protected override void HandleDrop(UI_ItemSlot source)
    {
        if (source == null || source.itemInSlot == null) return;
        if (source == this) return;

        Inventory_Item item = source.itemInSlot;
        if (item.itemDate is not EquipmentDateSO) return;

        // 来源也是装备槽：装备不在背包里，先卸下（放回 itemList），TryEquipItem 才找得到
        if (source is UI_EquipSlot)
        {
            inventory.UnequipItem(item);
        }

        inventory.TryEquipItem(item);
    }
}
