using UnityEngine;

public class UI_Storage : MonoBehaviour
{
    private Inventoty_Storage storage;
    private Inventory_Player inventory;

    [SerializeField] private UI_ItemSlotParent inventoryParent;
    [SerializeField] private UI_ItemSlotParent storageParent;
    [SerializeField] private UI_ItemSlotParent materialStashParent;

    public void SetupStorageUI(Inventoty_Storage storage)
    {
        this.storage = storage;
        inventory = storage.playerInventory;
        storage.OnInventoryChange += UpdateUI;
        UpdateUI();

        // 带 true：存储面板可能在初始化时还未激活，不带 true 会得到空数组
        UI_StorageSlot[] storageSlots = GetComponentsInChildren<UI_StorageSlot>(true);

        foreach (var slot in storageSlots)
        {
            slot.SetStorage(storage);
        }
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (storage == null)
        {
            return;
        }

        inventoryParent.UpdateSlots(inventory.itemList);
        storageParent.UpdateSlots(storage.itemList);
        materialStashParent.UpdateSlots(storage.materialStash);
    }
}
