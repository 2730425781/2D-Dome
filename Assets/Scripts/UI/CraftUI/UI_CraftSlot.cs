using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftSlot : MonoBehaviour
{
    private ItemDataSO itemToCraft;
    [SerializeField] private UI_CraftPreviw craftPreviw;
    [SerializeField] private Image craftItemIcon;
    [SerializeField] private TextMeshProUGUI craftItemName;

    /// <summary>本槽位对应的可制作物品（供拖拽到制作列表时按产物匹配）。</summary>
    public ItemDataSO ItemToCraft => itemToCraft;

    public void SetupButton(ItemDataSO itemDate)
    {
        itemToCraft = itemDate;
        craftItemIcon.sprite = itemDate.itemIcon;
        craftItemName.text = itemDate.itemName;
    }

    public void UpdateCraftPreviw()
    {
        craftPreviw.UpdateCraftPreviw(itemToCraft);
    }
}
