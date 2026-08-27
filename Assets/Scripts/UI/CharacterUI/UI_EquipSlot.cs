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
}
