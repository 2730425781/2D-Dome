using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EquipSlot : UI_ItemSlot
{
    public ItemType slotType;

    private void OnValidate()
    {
        gameObject.name = "装备槽位 - " + slotType.ToString();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (itemInSlot == null) return;
        inventory.UnequipItem(itemInSlot);
    }
}
