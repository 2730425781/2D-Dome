using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action OnInventoryChange;
    public int maxInventorySixe = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    protected virtual void Awake()
    {

    }

    public void TryUseItem(Inventory_Item itemToUse)
    {
        Inventory_Item consumable = itemList.Find(item => item == itemToUse);

        if (consumable == null) return;

        consumable.itemEffect.ExecuteEffect();

        if (consumable.stackSize > 1)
        {
            consumable.RemoveStack();
        }
        else
        {
            RemoveInventoryItem(consumable);
        }

        OnInventoryChange?.Invoke();
    }

    public bool CanAddItem() => itemList.Count < maxInventorySixe;
    public Inventory_Item FindAddStack(Inventory_Item itemToAdd)
    {
        List<Inventory_Item> stackItems = itemList.FindAll(item => item.itemDate == itemToAdd.itemDate);

        foreach (var stack in stackItems)
        {
            if (stack.CanAddStack())
            {
                return stack;
            }
        }

        return null;
    }

    public void AddItem(Inventory_Item item)
    {
        Inventory_Item inventoryItem = FindAddStack(item);

        if (inventoryItem != null)
        {
            inventoryItem.AddStack();
        }
        else
        {
            itemList.Add(item);
        }

        OnInventoryChange?.Invoke();
    }

    public void RemoveInventoryItem(Inventory_Item item)
    {
        itemList.Remove(item);
        OnInventoryChange?.Invoke();
    }

    public Inventory_Item FindItem(ItemDateSO itemDate)
    {
        return itemList.Find(item => item.itemDate == itemDate);
    }

    public void TriggerDateUI() => OnInventoryChange?.Invoke();
}
