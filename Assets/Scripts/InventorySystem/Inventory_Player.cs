using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家背包：继承 Inventory_Base 复用背包的堆叠、容量与事件逻辑，再叠加“装备穿戴”这一玩家专属能力。
/// 用继承而非组合，是因为它和基类是严格的“是一种背包”的关系，UI 和仓库都只依赖基类接口即可操作它。
/// </summary>
public class Inventory_Player : Inventory_Base
{
    public event Action<int, Inventory_Item> OnQuickSlotUsed;
    // 按下快捷栏按键使用物品后触发（不区分"设置/使用"，仅在使用时发出），用于 UI 变暗反馈
    public event Action<int> OnQuickItemUsed;
    public int gold = 10000;
    public List<Inventory_EquipmentSlot> equipList;
    public Inventoty_Storage storage { get; private set; }

    [Header("快速物品栏")]
    [SerializeField]
    private Inventory_Item[] quickItems = new Inventory_Item[2];

    // 在 Awake 而非 Start 里取 Player：换装可能在任何帧被触发，必须保证进入游戏时引用就已就绪；
    // 先调 base.Awake() 保住基类的初始化契约
    protected override void Awake()
    {
        base.Awake();
        storage = FindAnyObjectByType<Inventoty_Storage>();
    }

    public void SetQuickItemInSlot(int slotNumber, Inventory_Item item)
    {
        quickItems[slotNumber - 1] = item;
        OnQuickSlotUsed?.Invoke(slotNumber - 1, item);
    }

    /// <summary>
    /// 判断某物品"种类"是否已占用任意快捷栏位。
    /// 按 itemDate（物品数据资产）而非实例判断：不允许同类型的两个堆叠
    /// （如两格治疗药水）同时占用快捷栏——否则使用其中一个堆叠时，另一格的数量不会同步。
    /// </summary>
    public bool IsItemInQuickSlot(Inventory_Item item)
    {
        if (item == null || item.itemDate == null) return false;

        foreach (var qi in quickItems)
        {
            if (qi != null && qi.itemDate == item.itemDate) return true;
        }
        return false;
    }

    public void TryUseQuickItemInSlot(int passedSlotNumber)
    {
        int slotNumber = passedSlotNumber - 1;

        // 防止传入 0 或超出范围时数组越界
        if (slotNumber < 0 || slotNumber >= quickItems.Length)
        {
            return;
        }

        var itemToUse = quickItems[slotNumber];
        if (itemToUse == null)
        {
            Debug.Log("物品快捷栏没有物品");
            return;
        }

        TryUseItem(itemToUse);

        // 用完后重新从背包解析：可能已用完(→null)，或背包里还有同类型另一堆叠(→指向它)。
        // 不能沿用旧引用——最后一个堆叠用掉后旧实例已从背包移除，残留引用会导致
        // 之后再次按下时 Use 不生效甚至异常
        quickItems[slotNumber] = FindSameItem(itemToUse);

        OnQuickSlotUsed?.Invoke(slotNumber, quickItems[slotNumber]);
        // 使用反馈：和点击一致——按快捷键使用时 UI 也会变暗一闪
        OnQuickItemUsed?.Invoke(slotNumber);
    }

    public void TryEquipItem(Inventory_Item item)
    {
        if (item == null || item.itemDate == null) return;
        // 只有装备数据（EquipmentDateSO）才能穿戴，材料直接忽略
        if (item.itemDate is not EquipmentDateSO) return;

        // 用数据资产反查背包里的真实实例，而不是直接用传入的 item：
        // 传入的可能是 UI 持有的引用，只有背包实例参与移除/堆叠才一致
        var inventoryItem = FindItem(item.itemDate);
        if (inventoryItem == null)
        {
            Debug.LogWarning("背包中未找到该物品: " + item.itemDate.itemName);
            return;
        }

        if (equipList == null)
        {
            Debug.LogWarning("装备槽列表 equipList 未在 Inspector 中赋值");
            return;
        }

        // 按 ItemType 过滤出可穿的槽位，确保“头部装备只能进头部槽”
        var matchingSlots = equipList.FindAll(slot => slot.slotType == item.itemDate.itemType);
        if (matchingSlots.Count == 0)
        {
            Debug.LogWarning("没有匹配的装备槽位: " + item.itemDate.itemType);
            return;
        }

        foreach (var slot in matchingSlots)
        {
            if (!slot.HasItem())
            {
                EquipItem(inventoryItem, slot);
                return;
            }
        }

        // 所有匹配槽都已占用：先卸下第一个槽里的旧装备，再穿上新装备。
        // 卸下时带 replacingItem=true，跳过“背包满则拒绝”的检查——旧装备会回到背包，
        // 正好补回新装备占掉的那一格，容量上必然成立
        var slotToReplace = matchingSlots[0];
        var itemToUneqip = slotToReplace.equipedItem;

        UnequipItem(itemToUneqip, slotToReplace != null);
        EquipItem(item, slotToReplace);
    }

    private void EquipItem(Inventory_Item item, Inventory_EquipmentSlot slot)
    {
        // 换装会改变最大生命值（装备带生命加成）：先记录当前生命百分比，
        // 挂上修改器后再按百分比恢复，保证换装不产生“凭空回血/扣血”的跳变
        float saveHealthPercent = player.health.GetHealthPercentage();

        slot.equipedItem = item;
        slot.equipedItem.AddModfifiers(player.stats);
        slot.equipedItem.AddItemEffect(player);

        player.health.SetHealthPercentage(saveHealthPercent);
        RemoveItem(item);
    }

    public void UnequipItem(Inventory_Item item, bool replacingItem = false)
    {
        // 背包满时拒绝卸下，否则卸下的装备无处可放；replacingItem 时跳过，
        // 因为替换流程里新装备占用的那一格正好被旧装备填回
        if (!CanAddItem(item) && !replacingItem)
        {
            Debug.Log("物品库存不足");
            return;
        }

        float saveHealthPercent = player.health.GetHealthPercentage();

        foreach (var slot in equipList)
        {
            if (slot.equipedItem == item)
            {
                slot.equipedItem = null;
                break;
            }
        }

        // 与 EquipItem 严格对称：撤销属性修改器、退订持续效果、恢复生命百分比、物品回背包，
        // 顺序反过来就可能在属性未移除时错误计算血量
        item.RemoveModifiers(player.stats);
        item.RemoveItemEffect();
        player.health.SetHealthPercentage(saveHealthPercent);
        AddItem(item);
    }
}