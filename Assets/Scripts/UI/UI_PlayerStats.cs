using UnityEngine;

public class UI_PlayerStats : MonoBehaviour
{
    private UI_StatSlot[] uiStatSolts;
    private Inventory_Player inventory;

    private void Awake()
    {
        uiStatSolts = GetComponentsInChildren<UI_StatSlot>();

        inventory = FindAnyObjectByType<Inventory_Player>();
        inventory.OnInventoryChange += UpdateStatsUI;
    }

    private void Start()
    {
        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        foreach (var statSlot in uiStatSolts)
        {
            statSlot.UpdateStatValue();
        }
    }
}
