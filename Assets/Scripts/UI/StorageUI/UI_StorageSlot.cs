using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StorageSlot : UI_ItemSlot
{
    private Inventoty_Storage storage;

    public enum StorageSlotType { StorageSlot, PlayerInventorySlot, MaterialStashSlot }
    public StorageSlotType slotType;

    public void SetStorage(Inventoty_Storage storage) => this.storage = storage;

    /// <summary>按槽位类型返回各自绑定的数据列表（仓库/玩家背包/材料库）。</summary>
    public override List<Inventory_Item> SlotItemList
    {
        get
        {
            if (storage == null) return base.SlotItemList;

            switch (slotType)
            {
                case StorageSlotType.StorageSlot: return storage.itemList;
                case StorageSlotType.PlayerInventorySlot:
                    return storage.playerInventory != null ? storage.playerInventory.itemList : null;
                case StorageSlotType.MaterialStashSlot: return storage.materialStash;
                default: return base.SlotItemList;
            }
        }
    }

    protected override void ExecuteClickAction(PointerEventData eventData)
    {
        if (itemInSlot == null) return;

        // 左键点击沿用原有"转移单个（Ctrl=整组）"逻辑；
        // 右键交给基类的使用/穿戴（材料会被基类拦截），材料库槽位点击不转移
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            bool transferStack = Input.GetKey(KeyCode.LeftControl);

            if (slotType == StorageSlotType.StorageSlot)
            {
                storage.StoargeToPlayer(itemInSlot, transferStack);
            }
            else if (slotType == StorageSlotType.PlayerInventorySlot)
            {
                storage.PlayerToStorage(itemInSlot, transferStack);
            }
        }

        ui.itemToolTip.ShowToolTip(false, null);
    }

    /// <summary>
    /// 拖放：
    /// - 同一列表内 → 基类排序/交换
    /// - 跨列表（玩家/仓库/材料库两两之间）→ 整组转移
    /// </summary>
    protected override void HandleDrop(UI_ItemSlot source)
    {
        if (source == this || source.itemInSlot == null) return;

        List<Inventory_Item> myList = SlotItemList;
        if (myList != null && source.SlotItemList == myList)
        {
            base.HandleDrop(source);
            return;
        }

        if (storage == null) return;

        var item = source.itemInSlot;
        var playerList = storage.playerInventory != null ? storage.playerInventory.itemList : null;
        var storageList = storage.itemList;

        if (slotType == StorageSlotType.PlayerInventorySlot)
        {
            if (source.SlotItemList == storageList) storage.StoargeToPlayer(item, true);                             // 仓库 → 玩家
            else if (source.SlotItemList == storage.materialStash) storage.MoveStashTo(storage.playerInventory, item); // 材料库 → 玩家
        }
        else if (slotType == StorageSlotType.StorageSlot)
        {
            if (source.SlotItemList == playerList) storage.PlayerToStorage(item, true);                              // 玩家 → 仓库
            else if (source.SlotItemList == storage.materialStash) storage.MoveStashTo(storage, item);               // 材料库 → 仓库
        }
        else if (slotType == StorageSlotType.MaterialStashSlot)
        {
            if (source.SlotItemList == playerList) storage.MoveToStash(playerList, item);                            // 玩家 → 材料库
            else if (source.SlotItemList == storageList) storage.MoveToStash(storageList, item);                     // 仓库 → 材料库
        }

        // 玩家背包被改动时同步刷新角色面板与合成面板（存储面板由 storage 事件刷新）
        if (source.SlotItemList == playerList || SlotItemList == playerList)
        {
            if (storage.playerInventory != null) storage.playerInventory.TriggerUpdateUI();
        }
    }
}
