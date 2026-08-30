using UnityEngine;
using UnityEngine.EventSystems;

public class UI_QuickItemSlot : UI_ItemSlot
{
    [SerializeField] private Sprite defaultSrite;
    [SerializeField] private int slotNumber;

    public void SetupQuickSlotItem(Inventory_Item item)
    {
        inventory.SetQuickItemInSlot(slotNumber, item);
    }

    public void UpdateQuickItemSlot(Inventory_Item currentItem)
    {
        // 必须同步 itemInSlot：基类的悬停提示框/拖拽都依赖它；
        // 之前只更新了图标/数量，itemInSlot 一直是 null，所以快捷栏既不显示提示框也不能拖拽
        itemInSlot = currentItem;

        if (currentItem == null || currentItem.itemDate == null)
        {
            itemIcon.sprite = defaultSrite;
            itemStackSize.text = "";
            return;
        }

        itemIcon.sprite = currentItem.itemDate.itemIcon;
        itemStackSize.text = currentItem.stackSize.ToString();
    }

    /// <summary>
    /// 拖放：
    /// - 快捷栏之间拖动 = 交换两个栏位的内容
    /// - 背包可消耗物品拖入 = 替换该栏位；若该物品已占用其它栏位则拒绝
    /// </summary>
    protected override void HandleDrop(UI_ItemSlot source)
    {
        if (source == null || source.itemInSlot == null) return;

        // 快捷栏之间拖动 = 交换两栏内容
        if (source is UI_QuickItemSlot sourceQuick)
        {
            Inventory_Item sourceItem = source.itemInSlot;
            Inventory_Item targetItem = itemInSlot;
            SetupQuickSlotItem(sourceItem);              // 本栏位 ← 源栏位
            sourceQuick.SetupQuickSlotItem(targetItem);  // 源栏位 ← 本栏位（空则清空）
            return;
        }

        // 背包物品拖入快捷栏 = 替换该栏位（仅消耗品）
        if (source.itemInSlot.itemDate.itemType != ItemType.Consumable) return;
        // 该物品已占用其它快捷栏位时拒绝：避免同一实例占两格导致堆叠在使用后不同步
        if (inventory != null && inventory.IsItemInQuickSlot(source.itemInSlot)) return;

        SetupQuickSlotItem(source.itemInSlot);
    }

    protected override void ExecuteClickAction(PointerEventData eventData)
    {
        ui.inGameUI.OpenQuickItemOptions(this, rect);
    }
}
