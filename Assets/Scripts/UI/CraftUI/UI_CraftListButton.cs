using UnityEngine;

public class UI_CraftListButton : MonoBehaviour
{
    [SerializeField] private ItemListDateSO craftDate;
    private UI_CraftSlot[] craftSlots;

    public void SetCraftSlots(UI_CraftSlot[] craftSlots) => this.craftSlots = craftSlots;

    /// <summary>该分类的配方列表（供拖拽时按分类查找产物）。</summary>
    public ItemListDateSO CraftDate => craftDate;

    /// <summary>该分类的产物列表中是否包含指定物品。</summary>
    public bool ContainsProduct(ItemDataSO itemDate)
    {
        if (craftDate == null || itemDate == null) return false;

        foreach (var product in craftDate.itemList)
        {
            if (product == itemDate) return true;
        }
        return false;
    }

    public void UpdateCraftSlots()
    {
        if (craftDate == null)
        {
            Debug.Log("该物品制作列表还未设置");
            return;
        }

        foreach (var slot in craftSlots)
        {
            slot.gameObject.SetActive(false);
        }

        // 槽位池数量有限，分类产物超过池容量时只显示前 N 个，避免越界崩溃
        int showCount = Mathf.Min(craftDate.itemList.Length, craftSlots.Length);
        for (int i = 0; i < showCount; i++)
        {
            ItemDataSO itemDate = craftDate.itemList[i];

            craftSlots[i].gameObject.SetActive(true);
            craftSlots[i].SetupButton(itemDate);
        }
    }
}
