using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Craft : MonoBehaviour, IDropHandler
{
    [SerializeField] private UI_ItemSlotParent inventoryParent;
    private Inventory_Player inventory;
    private UI_CraftSlot[] craftSlots;
    private UI_CraftPreviw craftPreviwUI;
    private UI_CraftListButton[] craftListButtons;

    public void SetupCraftUI(Inventoty_Storage storage)
    {
        inventory = storage.playerInventory;
        inventory.OnInventoryChange += UpdateUI;
        UpdateUI();

        // 带 true：面板可能在初始化时还未激活
        craftPreviwUI = GetComponentInChildren<UI_CraftPreviw>(true);
        craftPreviwUI.SetupCraftPreviw(storage);
        SetupCraftListButtons();
    }

    private void SetupCraftListButtons()
    {
        // 带 true：合成面板可能尚未激活（SetupCraftUI 在 SetActive(true) 之前调用），
        // 不带 true 会跳过未激活子物体，craftSlots/craftListButtons 会变成空数组
        craftSlots = GetComponentsInChildren<UI_CraftSlot>(true);
        craftListButtons = GetComponentsInChildren<UI_CraftListButton>(true);

        foreach (var slot in craftSlots)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (var button in craftListButtons)
        {
            button.SetCraftSlots(craftSlots);
        }
    }

    private void UpdateUI() => inventoryParent.UpdateSlots(inventory.itemList);

    /// <summary>
    /// 把物品拖到制作列表：
    /// 1. 先在所有分类里找到该物品所属的分类并自动切换过去（列表当前只显示一个分类的产物）；
    /// 2. 再选中对应配方槽位并显示其材料需求；
    /// 3. 任何分类都没有该产物时，在预览区提示"无法制作"。
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        UI_ItemSlot source = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<UI_ItemSlot>() : null;
        if (source == null || source.itemInSlot == null || craftPreviwUI == null) return;

        ItemDateSO droppedItem = source.itemInSlot.itemDate;

        if (craftSlots == null || craftListButtons == null) SetupCraftListButtons();

        foreach (var category in craftListButtons)
        {
            if (category.ContainsProduct(droppedItem))
            {
                // 切换到该分类：填充 craftSlots 后，对应产物的槽位才处于激活状态
                category.UpdateCraftSlots();

                foreach (var slot in craftSlots)
                {
                    if (slot.ItemToCraft == droppedItem)
                    {
                        slot.UpdateCraftPreviw();
                        return;
                    }
                }
                return;
            }
        }

        craftPreviwUI.ShowCannotCraft(droppedItem);
    }
}
