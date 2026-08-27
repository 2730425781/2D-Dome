using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 商店材料箱的拖放规则：该区域只接受"商店材料"拖入（购买）。
/// 挂在商店面板的 UI_MaterialStash 上：它实现了 IDropHandler，
/// 会优先于整块面板的 UI_Merchant.OnDrop 拦截材料箱区域的拖放——
/// 玩家物品、商店非材料、材料箱自身物品拖到材料箱一律忽略。
/// 数据层的"只收材料"兜底在 Inventoty_Storage.AddMaterialToStash / MoveToStash。
/// </summary>
public class UI_MerchantMaterialBox : MonoBehaviour, IDropHandler
{
    private Inventory_Merchant merchant;

    public void Setup(Inventory_Merchant merchant) => this.merchant = merchant;

    public void OnDrop(PointerEventData eventData)
    {
        UI_ItemSlot source = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<UI_ItemSlot>() : null;
        if (source == null || source.itemInSlot == null || merchant == null) return;

        // 只有商店材料能进入材料箱；其他来源/非材料一律忽略
        if (source.SlotItemList == merchant.itemList
            && source.itemInSlot.itemDate.itemType == ItemType.Material)
        {
            merchant.TryBuyItem(source.itemInSlot, true);
        }
    }
}
