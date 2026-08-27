using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
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
    private Sprite defaultSlotSprite;
    private Color defaultSlotColor = Color.white;

    // 按下后是否发生了拖动：发生过拖动则"点击行为"不再执行，
    // 否则会出现"按下就买/卖一次 + 拖走又买/卖整组"的双重触发
    protected bool dragStarted;
    // 拖动时跟随鼠标的幽灵图标（拖拽期间临时创建，结束即销毁）
    private GameObject dragGhost;

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

    // ---------- 点击与拖拽的区分 ----------

    public void OnPointerDown(PointerEventData eventData)
    {
        // 只记录"按下"，点击行为延后到 OnPointerUp：
        // 若按下后发生了拖拽，说明用户意图是拖动而不是点击
        dragStarted = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!dragStarted) ExecuteClickAction(eventData);
    }

    /// <summary>点击行为（未发生拖拽的按下-抬起）。子类覆写为各自的点击语义（买卖/转移/卸下）。</summary>
    protected virtual void ExecuteClickAction(PointerEventData eventData)
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

    // ---------- 拖拽 ----------

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 只响应左键拖动：右键在子类里是"使用/出售"等独立行为
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (itemInSlot == null) return;

        dragStarted = true;
        // 拖拽开始先藏掉 ToolTip，避免拖拽过程中悬停残留
        if (ui != null) ui.itemToolTip.ShowToolTip(false, null);

        CreateDragGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null) dragGhost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyDragGhost();
    }

    /// <summary>拖放目标收到事件：pointerDrag 即拖拽源物体，取其槽位组件作为来源。</summary>
    public void OnDrop(PointerEventData eventData)
    {
        UI_ItemSlot source = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<UI_ItemSlot>() : null;
        if (source == null || source.itemInSlot == null) return;

        HandleDrop(source);
    }

    /// <summary>本槽位绑定的数据列表。子类覆写以指向各自容器（玩家/商店/仓库/材料库）。</summary>
    public virtual List<Inventory_Item> SlotItemList => inventory != null ? inventory.itemList : null;

    /// <summary>
    /// 处理拖放。默认行为：同一列表内排序/交换（拖到有物品的格=交换，拖到空格=移动）。
    /// 跨列表的转移（买卖/存取）由子类覆写本方法实现。
    /// </summary>
    protected virtual void HandleDrop(UI_ItemSlot source)
    {
        if (source == this || source.itemInSlot == null) return;

        List<Inventory_Item> list = SlotItemList;
        if (list == null || source.SlotItemList != list) return;

        ReorderItems(source, list);
        // 源与目标可能分属两个面板，两侧都要刷新
        source.RefreshUI();
        RefreshUI();
    }

    private void ReorderItems(UI_ItemSlot source, List<Inventory_Item> list)
    {
        int fromIndex = list.IndexOf(source.itemInSlot);
        if (fromIndex < 0) return;

        if (itemInSlot != null)
        {
            // 目标格有物品：交换两格
            int toIndex = list.IndexOf(itemInSlot);
            if (toIndex < 0) return;
            (list[fromIndex], list[toIndex]) = (list[toIndex], list[fromIndex]);
        }
        else
        {
            // 目标格为空：把物品从原位置移除，插入到目标槽位序号
            var parent = GetComponentInParent<UI_ItemSlotParent>();
            int targetIndex = parent != null ? parent.IndexOf(this) : -1;
            if (targetIndex < 0) return;

            var item = source.itemInSlot;
            list.Remove(item);
            list.Insert(Mathf.Clamp(targetIndex, 0, list.Count), item);
        }
    }

    /// <summary>刷新本槽位所属容器的界面。子类覆写指向各自的刷新入口。</summary>
    protected virtual void RefreshUI() => inventory?.TriggerUpdateUI();

    // ---------- 拖拽幽灵图标 ----------

    private void CreateDragGhost(PointerEventData eventData)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
        var image = dragGhost.GetComponent<Image>();
        image.sprite = itemInSlot.itemDate.itemIcon;
        // 幽灵不参与射线检测，否则会挡住下方槽位的 OnDrop
        image.raycastTarget = false;

        var ghostRect = dragGhost.GetComponent<RectTransform>();
        // 槽位图标通常是拉伸锚点（stretch），此时 sizeDelta 恒为 0——
        // 必须用 rect.size 取实际宽高，否则幽灵图标尺寸为 0 完全不可见
        Vector2 iconSize = itemIcon != null ? itemIcon.rectTransform.rect.size : new Vector2(48, 48);
        if (iconSize.x <= 0 || iconSize.y <= 0) iconSize = new Vector2(48, 48);
        ghostRect.sizeDelta = iconSize;
        ghostRect.SetParent(canvas.transform, false);
        // 置为 Canvas 最后一个子物体，保证渲染在最上层
        ghostRect.SetAsLastSibling();
        ghostRect.position = eventData.position;
    }

    private void DestroyDragGhost()
    {
        if (dragGhost != null)
        {
            Destroy(dragGhost);
            dragGhost = null;
        }
    }

    // ---------- 展示 ----------

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

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (itemInSlot == null) return;

        ui.itemToolTip.ShowToolTip(true, rect, itemInSlot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.itemToolTip.ShowToolTip(false, null);
    }

    /// <summary>
    /// 供编辑器脚本把"普通槽位"升级为带容器上下文的子类槽位时复制序列化引用
    /// （如把材料库的普通 UI_ItemSlot 替换为 UI_StorageSlot）。
    /// </summary>
    public void CopySlotVisuals(UI_ItemSlot other)
    {
        itemIcon = other.itemIcon;
        itemStackSize = other.itemStackSize;
    }
}
