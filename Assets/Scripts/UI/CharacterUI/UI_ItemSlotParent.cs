using System.Collections.Generic;
using UnityEngine;

public class UI_ItemSlotParent : MonoBehaviour
{
    private UI_ItemSlot[] slots;

    /// <summary>子槽位数组（惰性缓存，包含未激活面板里的槽位）。</summary>
    public UI_ItemSlot[] Slots
    {
        get
        {
            if (slots == null) slots = GetComponentsInChildren<UI_ItemSlot>(true);
            return slots;
        }
    }

    /// <summary>返回指定槽位在本父节点中的下标，供"拖到空格"时计算插入位置。</summary>
    public int IndexOf(UI_ItemSlot slot) => System.Array.IndexOf(Slots, slot);

    public void UpdateSlots(List<Inventory_Item> itemList)
    {
        if (slots == null) slots = GetComponentsInChildren<UI_ItemSlot>(true);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < itemList.Count)
            {
                slots[i].UpdateSlot(itemList[i]);
            }
            else
            {
                slots[i].UpdateSlot(null);
            }
        }
    }
}
