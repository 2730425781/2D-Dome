using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftPreviwSlot : MonoBehaviour
{
    [SerializeField] private Image materialIcon;
    [SerializeField] private TextMeshProUGUI materialName;

    public void SetupPreviwSlot(ItemDataSO itemDate, int avaliableAmount, int requireAmount)
    {
        materialIcon.sprite = itemDate.itemIcon;
        materialName.text = itemDate.itemName + " - " + avaliableAmount + " / " + requireAmount;
    }
}
