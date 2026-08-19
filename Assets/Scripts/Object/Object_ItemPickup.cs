using UnityEngine;

public class Object_ItemPickup : MonoBehaviour
{
    private SpriteRenderer sr;
    [SerializeField] private ItemDateSO itemDate;
    private Inventory_Item item;
    private Inventory_Base inventory;

    private void Awake()
    {
        item = new Inventory_Item(itemDate);
    }

    private void OnValidate()
    {
        if (itemDate == null) return;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = itemDate.itemIcon;
        gameObject.name = itemDate.itemName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        inventory = collision.GetComponent<Inventory_Base>();
        if (inventory == null) return;

        bool canAddItem = inventory.CanAddItem() || inventory.FindAddStack(item) != null;

        if (canAddItem)
        {
            inventory.AddItem(item);
            Destroy(gameObject);
        }
    }
}
