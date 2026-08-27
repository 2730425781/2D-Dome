using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MerchantSlot : UI_ItemSlot
{
    private Inventory_Merchant merchant;
    public enum merchantSlotType { MerchantSlot, PlayerSlot, MerchantStashSlot }
    public merchantSlotType slotType;

    /// <summary>玩家一侧展示玩家背包，商店一侧展示商人库存，材料面板展示玩家材料库。</summary>
    public override List<Inventory_Item> SlotItemList
    {
        get
        {
            if (merchant == null) return base.SlotItemList;
            var player = merchant.PlayerInventory;

            switch (slotType)
            {
                case merchantSlotType.MerchantStashSlot:
                    // 商店的材料面板展示玩家材料库（与存储界面共用同一份数据）
                    return player != null && player.storage != null ? player.storage.materialStash : base.SlotItemList;
                case merchantSlotType.PlayerSlot:
                    return player != null ? player.itemList : base.SlotItemList;
                default:
                    return merchant.itemList;
            }
        }
    }

    protected override void ExecuteClickAction(PointerEventData eventData)
    {
        if (itemInSlot == null) return;

        bool rightButton = eventData.button == PointerEventData.InputButton.Right;
        bool leftButton = eventData.button == PointerEventData.InputButton.Left;

        if (slotType == merchantSlotType.PlayerSlot)
        {
            if (leftButton)
            {
                bool sellFullStack = Input.GetKey(KeyCode.LeftControl);
                merchant.TrySellItem(itemInSlot, sellFullStack);
            }
            else if (rightButton)
            {
                //查看装备  还未实现



                //base.ExecuteClickAction(eventData);

                // 使用/穿戴物品只触发"玩家背包"的变更事件，而商店面板监听的是
                // "商人背包"的事件——不手动刷新的话玩家一侧槽位会停在旧数量
                //if (merchant != null) merchant.TriggerUpdateUI();
                return;
            }
        }
        else if (slotType == merchantSlotType.MerchantSlot)
        {
            if (leftButton)
            {
                bool buyFullStack = Input.GetKey(KeyCode.LeftControl);
                merchant.TryBuyItem(itemInSlot, buyFullStack);
            }
            else if (rightButton)
            {
                // 查看装备  还未实现
                return;
            }
        }
        else if (slotType == merchantSlotType.MerchantStashSlot)
        {
            // 材料箱点击 = 出售材料（Ctrl=整组出售）
            if (leftButton)
            {
                bool sellFullStack = Input.GetKey(KeyCode.LeftControl);
                merchant.TrySellStashItem(itemInSlot, sellFullStack);
            }
        }

        ui.itemToolTip.ShowToolTip(false, null);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (itemInSlot == null) return;

        if (slotType == merchantSlotType.MerchantSlot)
        {
            ui.itemToolTip.ShowToolTip(true, rect, itemInSlot, true, true);
        }
        else
        {
            ui.itemToolTip.ShowToolTip(true, rect, itemInSlot, false, true);
        }
    }


    /// <summary>
    /// 拖放：
    /// - 同一列表内 → 基类排序/交换
    /// - 玩家物品拖到商店格 → 整组出售
    /// - 材料箱物品拖到商店格 → 整组出售材料
    /// - 商店物品拖到玩家格 → 整组购买
    /// - 商店材料拖到材料箱 → 购买；非材料不能加入材料箱
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

        if (merchant == null) return;

        var player = merchant.PlayerInventory;
        var playerList = player != null ? player.itemList : null;
        var merchantList = merchant.itemList;
        var stashList = player != null && player.storage != null ? player.storage.materialStash : null;

        if (slotType == merchantSlotType.MerchantSlot && source.SlotItemList == playerList)
        {
            // 玩家物品 → 商店格：整组出售
            merchant.TrySellItem(source.itemInSlot, true);
        }
        else if (slotType == merchantSlotType.MerchantSlot && stashList != null && source.SlotItemList == stashList)
        {
            // 材料箱物品 → 商店格：整组出售材料
            merchant.TrySellStashItem(source.itemInSlot, true);
        }
        else if (slotType == merchantSlotType.PlayerSlot && source.SlotItemList == merchantList)
        {
            // 商店物品 → 玩家格：整组购买
            merchant.TryBuyItem(source.itemInSlot, true);
        }
        else if (slotType == merchantSlotType.MerchantStashSlot
                 && source.SlotItemList == merchantList
                 && source.itemInSlot.itemDate.itemType == ItemType.Material)
        {
            // 商店材料 → 材料箱：整组购买（只接收材料，非材料不能加入）
            merchant.TryBuyItem(source.itemInSlot, true);
        }
    }

    public void SetupMerchantUI(Inventory_Merchant merchant) => this.merchant = merchant;
}
