using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftPreviw : MonoBehaviour
{
    private Inventory_Item itemToCraft;
    private Inventoty_Storage storage;
    private UI_CraftPreviwSlot[] craftPreviwSlots;
    // 制作按钮的默认文案（场景里配置的"制作"之类）：ShowCannotCraft 会临时改成提示语，
    // 切换回可制作物品时必须还原，否则按钮文本一直停留在"无法制作"提示
    private string defaultButtonText;

    [Header("物品制作")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemInfo;
    [SerializeField] private TextMeshProUGUI buttonText;

    public void SetupCraftPreviw(Inventoty_Storage storage)
    {
        this.storage = storage;

        // 在任何人改文案之前记住默认值
        defaultButtonText = buttonText.text;

        // 带 true：面板可能在初始化时还未激活，不带 true 会得到空数组
        craftPreviwSlots = GetComponentsInChildren<UI_CraftPreviwSlot>(true);

        foreach (var slot in craftPreviwSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }

    public void ConfirmCraft()
    {
        if (itemToCraft == null)
        {
            buttonText.text = "选择一个物品";
            return;
        }

        if (storage.HasEnoughMaterials(itemToCraft) && storage.playerInventory.CanAddItem(itemToCraft))
        {
            storage.ConsumeMaterials(itemToCraft);
            storage.playerInventory.AddItem(itemToCraft);
        }

        UpdateCraftPreviwSlots();
    }

    public void UpdateCraftPreviw(ItemDataSO itemDate)
    {
        itemToCraft = new Inventory_Item(itemDate);
        itemIcon.sprite = itemDate.itemIcon;
        itemName.text = itemDate.itemName;
        itemInfo.text = itemToCraft.GetItemInfo();
        // 恢复制作按钮文案：ShowCannotCraft 改过的话这里必须还原，
        // 否则从"无法制作"切回可制作物品时按钮仍显示提示语
        if (!string.IsNullOrEmpty(defaultButtonText)) buttonText.text = defaultButtonText;
        UpdateCraftPreviwSlots();
    }

    /// <summary>拖入的物品没有对应配方时，在预览区给出提示并清空材料需求。</summary>
    public void ShowCannotCraft(ItemDataSO itemDate)
    {
        itemIcon.sprite = itemDate != null ? itemDate.itemIcon : null;
        itemName.text = itemDate != null ? itemDate.itemName : "";
        itemInfo.text = "该物品无法制作";
        buttonText.text = "选择一个可制作的物品";

        foreach (var slot in craftPreviwSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }

    private void UpdateCraftPreviwSlots()
    {
        foreach (var slot in craftPreviwSlots)
        {
            slot.gameObject.SetActive(false);
        }

        for (int i = 0; i < itemToCraft.itemDate.craftRecipe.Length; i++)
        {
            Inventory_Item requiredItem = itemToCraft.itemDate.craftRecipe[i];
            int avaliableAmount = storage.GetAvaliableAmount(requiredItem.itemDate);
            int requiredAmount = requiredItem.stackSize;

            craftPreviwSlots[i].gameObject.SetActive(true);
            craftPreviwSlots[i].SetupPreviwSlot(requiredItem.itemDate, avaliableAmount, requiredAmount);
        }
    }
}
