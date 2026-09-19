using UnityEngine;
using UnityEngine.EventSystems;

public class UI_QuestRewardSlot : UI_ItemSlot
{
    protected override void ExecuteClickAction(PointerEventData eventData)
    {

    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (itemInSlot == null)
            return;

        ui.itemToolTip.ShowToolTip(true, rect, itemInSlot, false, false, false);
    }
}
