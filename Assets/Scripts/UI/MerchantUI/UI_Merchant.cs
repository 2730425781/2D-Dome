using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Merchant : MonoBehaviour, IDropHandler
{
    [SerializeField] private UI_ItemSlotParent merchantSlots;
    [SerializeField] private UI_ItemSlotParent playerSlots;
    [SerializeField] private UI_ItemSlotParent materialSlots;

    private Inventory_Player playerInventory;
    private Inventory_Merchant merchantInventory;
    private Inventoty_Storage materialInventory;

    public void SetupMerchantUI(Inventory_Merchant merchant, Inventory_Player player, Inventoty_Storage material)
    {
        playerInventory = player;
        merchantInventory = merchant;
        materialInventory = material;

        // 先退订再订阅：Interact 每次进入商店都会调用本方法，直接 += 会让委托越积越多，
        // 之后每次物品变化触发 N 次刷新
        if (merchantInventory != null)
        {
            merchantInventory.OnInventoryChange -= UpdateSlotUI;
            merchantInventory.OnInventoryChange += UpdateSlotUI;
        }
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChange -= UpdateSlotUI;
            playerInventory.OnInventoryChange += UpdateSlotUI;
        }
        if (materialInventory != null)
        {
            // 买材料只触发材料库（存储背包）的变更事件，不订阅则商店材料面板不刷新
            materialInventory.OnInventoryChange -= UpdateSlotUI;
            materialInventory.OnInventoryChange += UpdateSlotUI;
        }

        UI_MerchantSlot[] merchantSlots = GetComponentsInChildren<UI_MerchantSlot>();

        foreach (var slot in merchantSlots)
        {
            slot.SetupMerchantUI(merchant);
        }

        // 材料箱区域自己的拖放规则（只收商店材料），挂载在 UI_MaterialStash 上
        var materialBox = GetComponentInChildren<UI_MerchantMaterialBox>(true);
        if (materialBox != null) materialBox.Setup(merchant);

        UpdateSlotUI();
    }

    private void UpdateSlotUI()
    {
        if (playerInventory == null || merchantInventory == null || materialInventory == null) return;

        playerSlots.UpdateSlots(playerInventory.itemList);
        merchantSlots.UpdateSlots(merchantInventory.itemList);
        materialSlots.UpdateSlots(materialInventory.materialStash);
    }

    /// <summary>
    /// 整块面板接受拖放：拖到槽位之外的面板区域也算数——
    /// 玩家物品拖到商店面板 = 整组出售；
    /// 材料箱物品拖到面板 = 整组出售材料；
    /// 商店物品拖到面板 = 整组购买。
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        UI_ItemSlot source = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<UI_ItemSlot>() : null;
        if (source == null || source.itemInSlot == null || merchantInventory == null || playerInventory == null) return;

        if (source.SlotItemList == playerInventory.itemList)
        {
            merchantInventory.TrySellItem(source.itemInSlot, true);
        }
        else if (materialInventory != null && source.SlotItemList == materialInventory.materialStash)
        {
            merchantInventory.TrySellStashItem(source.itemInSlot, true);
        }
        else if (source.SlotItemList == merchantInventory.itemList)
        {
            merchantInventory.TryBuyItem(source.itemInSlot, true);
        }
    }
}
