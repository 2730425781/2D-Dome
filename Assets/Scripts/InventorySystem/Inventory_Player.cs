using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家背包：继承 Inventory_Base 复用背包的堆叠、容量与事件逻辑，再叠加“装备穿戴”这一玩家专属能力。
/// 用继承而非组合，是因为它和基类是严格的“是一种背包”的关系，UI 和仓库都只依赖基类接口即可操作它。
/// </summary>
public class Inventory_Player : Inventory_Base
{
    public int gold = 10000;

    private Player player;
    public List<Inventory_EquipmentSlot> equipList;
    public Inventoty_Storage storage { get; private set; }

    // 在 Awake 而非 Start 里取 Player：换装可能在任何帧被触发，必须保证进入游戏时引用就已就绪；
    // 先调 base.Awake() 保住基类的初始化契约
    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
        storage = FindAnyObjectByType<Inventoty_Storage>();
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