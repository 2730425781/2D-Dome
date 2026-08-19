using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class Inventory_Item
{
    private string itemId;

    public ItemDateSO itemDate;
    public int stackSize = 1;
    public ItemModifier[] modifiers { get; private set; }
    public ItemEffectDateSO itemEffect;

    public Inventory_Item(ItemDateSO itemDate)
    {
        this.itemDate = itemDate;
        modifiers = EquipmentDate()?.modifiers;
        itemEffect = itemDate.itemEffect;

        itemId = itemDate.itemName + " - " + Guid.NewGuid();
    }

    public void AddModfifiers(Entity_Stats playerStats)
    {
        foreach (var mod in modifiers)
        {
            Stat statToModify = playerStats.GetStatValueByType(mod.statType);
            statToModify.AddModifier(mod.value, itemId);
        }
    }

    public void RemoveModifiers(Entity_Stats playerStats)
    {
        foreach (var mod in modifiers)
        {
            Stat statToModify = playerStats.GetStatValueByType(mod.statType);
            statToModify.RemoveModifier(itemId);
        }
    }

    public void AddItemEffect(Player player) => itemEffect?.Subscribe(player);
    public void RemoveItemEffect() => itemEffect?.Unsubscribe();

    private EquipmentDateSO EquipmentDate()
    {
        if (itemDate is EquipmentDateSO equipment)
        {
            return equipment;
        }
        return null;
    }

    public bool CanAddStack() => stackSize < itemDate.maxStackSize;
    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;
}
