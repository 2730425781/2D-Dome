using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    private Player player;
    public List<Inventory_EquipmentSlot> equipList;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    public void TryEquipItem(Inventory_Item item)
    {
        if (item == null || item.itemDate == null) return;
        // 只有装备数据（EquipmentDateSO）才能穿戴，材料直接忽略
        if (item.itemDate is not EquipmentDateSO) return;

        var inventoryItem = FindItem(item.itemDate);
        if (inventoryItem == null)
        {
            Debug.LogWarning("背包中未找到该物品: " + item.itemDate.itemName);
            return;
        }

        if (equipList == null)
        {
            Debug.LogWarning("装备槽列表 equipList 未在 Inspector 中赋值");
            return;
        }

        var matchingSlots = equipList.FindAll(slot => slot.slotType == item.itemDate.itemType);
        if (matchingSlots.Count == 0)
        {
            Debug.LogWarning("没有匹配的装备槽位: " + item.itemDate.itemType);
            return;
        }

        foreach (var slot in matchingSlots)
        {
            if (!slot.HasItem())
            {
                EquipItem(inventoryItem, slot);
                return;
            }
        }

        var slotToReplace = matchingSlots[0];
        var itemToUneqip = slotToReplace.equipedItem;

        UnequipItem(itemToUneqip, slotToReplace != null);
        EquipItem(item, slotToReplace);
    }

    private void EquipItem(Inventory_Item item, Inventory_EquipmentSlot slot)
    {
        float saveHealthPercent = player.health.GetHealthPercentage();

        slot.equipedItem = item;
        slot.equipedItem.AddModfifiers(player.stats);
        slot.equipedItem.AddItemEffect(player);

        player.health.SetHealthPercentage(saveHealthPercent);
        RemoveInventoryItem(item);
    }

    public void UnequipItem(Inventory_Item item, bool replacingItem = false)
    {
        if (!CanAddItem() && !replacingItem)
        {
            Debug.Log("物品库存不足");
            return;
        }

        float saveHealthPercent = player.health.GetHealthPercentage();

        foreach (var slot in equipList)
        {
            if (slot.equipedItem == item)
            {
                slot.equipedItem = null;
                break;
            }
        }

        item.RemoveModifiers(player.stats);
        item.RemoveItemEffect();
        player.health.SetHealthPercentage(saveHealthPercent);
        AddItem(item);
    }
}