using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Inventory_Item itemInSlot { get; private set; }
    protected Inventory_Player inventory;
    protected UI ui;
    protected RectTransform rect;

    [Header("物品槽位")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemStackSize;

    // 槽位默认底框：空槽时恢复它，而不是把图标清空
    // 用 OnValidate 在编辑器中缓存，避免运行时 Awake 顺序问题
    // （如果 UI_Inventory.Awake 先执行并调用了 UpdateSlot，itemIcon.sprite
    //   已经被换成物品图标，运行时再缓存就会把物品图标当成默认底框）
    [SerializeField] private Sprite defaultSlotSprite;
    [SerializeField] private Color defaultSlotColor = Color.white;

    private void OnValidate()
    {
        if (itemIcon == null) return;

        defaultSlotSprite = itemIcon.sprite;
        defaultSlotColor = itemIcon.color;
    }

    protected void Awake()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        inventory = FindAnyObjectByType<Inventory_Player>();

        // 序列化缓存缺失时兜底：用当前 ItemIcon 的默认 sprite/颜色
        if (defaultSlotSprite == null)
        {
            defaultSlotSprite = itemIcon.sprite;
            defaultSlotColor = itemIcon.color;
        }

        if (itemInSlot == null)
        {
            // 开局强制所有槽位显示默认底框，确保空槽位不缺图
            itemIcon.sprite = defaultSlotSprite;
            itemIcon.color = defaultSlotColor;
            itemStackSize.text = "";
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (itemInSlot == null || itemInSlot.itemDate.itemType == ItemType.Material)
        {
            return;
        }

        if (itemInSlot.itemDate.itemType == ItemType.Consumable)
        {
            if (!itemInSlot.itemEffect.CanBeUsed())
            {
                return;
            }
            inventory.TryUseItem(itemInSlot);
        }
        else
        {
            inventory.TryEquipItem(itemInSlot);
        }

        ui.itemToolTip.ShowToolTip(false, null);
    }

    public void UpdateSlot(Inventory_Item item)
    {
        itemInSlot = item;

        // 槽位变空：恢复默认底框，而不是 sprite=null + Color.clear，
        // 否则所有空槽位都会消失，只剩下物品槽位可见
        if (itemInSlot == null)
        {
            itemStackSize.text = "";
            itemIcon.sprite = defaultSlotSprite;
            itemIcon.color = defaultSlotColor;
            return;
        }

        Color color = Color.white; color.a = 0.9f;
        itemIcon.color = color;
        itemIcon.sprite = itemInSlot.itemDate.itemIcon;
        itemStackSize.text = item.stackSize > 1 ? item.stackSize.ToString() : " ";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemInSlot == null) return;

        ui.itemToolTip.ShowToolTip(true, rect, itemInSlot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.itemToolTip.ShowToolTip(false, null);
    }
}