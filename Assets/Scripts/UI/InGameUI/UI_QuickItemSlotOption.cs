using UnityEngine;
using UnityEngine.EventSystems;

public class UI_QuickItemSlotOption : UI_ItemSlot
{
    private UI_QuickItemSlot currentQuickItemSlot;
    private UI_InGame inGameUI;

    public void SetupOption(UI_QuickItemSlot quickItemSlot, Inventory_Item item)
    {
        currentQuickItemSlot = quickItemSlot;
        UpdateSlot(item);
    }

    protected override void ExecuteClickAction(PointerEventData eventData)
    {
        currentQuickItemSlot.SetupQuickSlotItem(itemInSlot);
        ui.inGameUI.HideQuickItemOptions();
    }

    // 不在单个槽位的 OnPointerExit 里关闭整个选项面板：
    // 槽位之间有间隙，跨间隙移动会触发 OnPointerExit 而误关。
    // 关闭动作交给父节点 UI_QuickItemOptionsZone——鼠标离开整块面板区域时才触发。
}
