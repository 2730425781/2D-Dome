using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 快捷栏选项面板的"整块离开检测"。
/// 挂在选项面板父节点（UI_QuickItemSlotOptionParent）上，该对象需有一个
/// 透明且可被射线命中的 Image（raycastTarget=true）来兜住槽位之间的间隙。
///
/// 为什么要整块检测：选项槽位之间有间隙（GridLayoutGroup 的 spacing），
/// 若在单个槽位的 OnPointerExit 里关闭，鼠标跨间隙移动会误关面板。
/// 这里在鼠标离开整块面板区域时才关闭，跨槽位/间隙移动保持打开。
/// </summary>
public class UI_QuickItemOptionsZone : MonoBehaviour, IPointerExitHandler
{
    public void OnPointerExit(PointerEventData eventData)
    {
        var ui = GetComponentInParent<UI>();
        if (ui != null && ui.inGameUI != null)
        {
            ui.inGameUI.HideQuickItemOptions();
        }
    }
}
