using System;
using System.Collections;
using System.Text;
using UnityEngine;

/// <summary>
/// 背包中的一个具体物品实例。做成可序列化的普通类而非 MonoBehaviour / ScriptableObject，
/// 是因为它表示“某次拾取到的、带独立状态的物品”（当前堆叠数、唯一 id），会被放进背包的 List 随背包一起保存；
/// 而物品的静态定义（名字、图标、堆叠上限）放在 ItemDateSO 资产里，一份数据资产可被无数实例共享。
/// </summary>
[Serializable]
public class Inventory_Item
{
    // 每个实例的唯一 id：装备属性修改器用该 id 登记来源，卸载装备时按 id 精确移除，避免误删其他装备的加成
    private string itemId;

    public ItemDateSO itemDate;
    public int stackSize = 1;
    public ItemModifier[] modifiers { get; private set; }
    public ItemEffectDateSO itemEffect;

    public int buyPrice { get; private set; }
    public float sellPride { get; private set; }

    public Inventory_Item(ItemDateSO itemDate)
    {
        this.itemDate = itemDate;
        // 只有装备（EquipmentDateSO）才带属性修改器；普通消耗品这里得到 null，反正也不会被穿戴
        modifiers = EquipmentDate()?.modifiers;
        itemEffect = itemDate.itemEffect;
        buyPrice = itemDate.itemPrice;
        sellPride = itemDate.itemPrice * 0.4f;

        // 同名物品也要区分实例：GUID 保证即使连拾两个相同装备，各自修改器也能独立撤销
        itemId = itemDate.itemName + " - " + Guid.NewGuid();
    }

    public void AddModfifiers(Entity_Stats playerStats)
    {
        foreach (var mod in modifiers)
        {
            Stat statToModify = playerStats.GetStatValueByType(mod.statType);
            statToModify.AddModifier(mod.value, itemId);
        }
    }

    public void RemoveModifiers(Entity_Stats playerStats)
    {
        foreach (var mod in modifiers)
        {
            Stat statToModify = playerStats.GetStatValueByType(mod.statType);
            statToModify.RemoveModifier(itemId);
        }
    }

    // 穿戴时订阅、卸下时退订：让持续型效果（如每帧回血）只在穿戴期间生效，且成对出现防止泄漏
    public void AddItemEffect(Player player) => itemEffect?.Subscribe(player);
    public void RemoveItemEffect() => itemEffect?.Unsubscribe();

    // 统一从这里判断物品是否为装备：用 is 模式匹配做安全转型，非装备返回 null，调用方无需再写类型判断
    private EquipmentDateSO EquipmentDate()
    {
        if (itemDate is EquipmentDateSO equipment)
        {
            return equipment;
        }
        return null;
    }

    // 堆叠上限存于共享资产（itemDate.maxStackSize），当前数量存于本实例（stackSize）：
    // 上限是定义数据不该随实例复制，数量是状态必须随实例保存
    public bool CanAddStack() => stackSize < itemDate.maxStackSize;
    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;

    public string GetItemInfo()
    {
        // 按类型分支：材料没有词条只有固定文案，消耗品直接显示效果描述，
        // 只有装备才需要逐条列出属性词条与特殊效果
        if (itemDate.itemType == ItemType.Material)
        {
            return "合成用材料";
        }
        if (itemDate.itemType == ItemType.Consumable)
        {
            return itemDate.itemEffect.effectDescription;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("");

        foreach (var mod in modifiers)
        {
            string modType = GetStatNameByType(mod.statType);
            // 百分比词条（暴击率等）加 % 后缀，与属性面板的显示规则一致
            string modValue = IsPercentageStat(mod.statType) ? mod.value.ToString() + "%" : mod.value.ToString();
            sb.AppendLine("+ " + modValue + " " + modType);
        }

        if (itemEffect != null)
        {
            sb.AppendLine("");
            sb.AppendLine("特殊效果:");
            sb.AppendLine(itemEffect.effectDescription);
        }

        return sb.ToString();
    }

    // 物品类型显示为中文，和属性名保持同样的本地化风格
    public string GetItemTypeName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Armor: return "防具";
            case ItemType.Weapon: return "武器";
            case ItemType.Trinket: return "饰品";
            case ItemType.Material: return "材料";
            case ItemType.Consumable: return "消耗品";
            default: return "未知类型";
        }
    }

    // 属性名统一用中文，与 Stat 面板（Stat_DefenseGroup/OffenseGroup/MajorGroup）里的名称保持一致
    private string GetStatNameByType(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return "最大生命值";
            case StatType.HealthRegen: return "生命值回复";
            case StatType.Armor: return "护甲";
            case StatType.Evasion: return "闪避";

            case StatType.Strength: return "力量";
            case StatType.Agility: return "敏捷";
            case StatType.Intelligence: return "智力";
            case StatType.Vitality: return "体力";

            case StatType.AttackSpeed: return "攻击速度";
            case StatType.Damage: return "基础伤害";
            case StatType.CritChance: return "暴击率";
            case StatType.CritPower: return "暴击伤害";
            case StatType.ArmorReduction: return "护甲穿透";

            case StatType.FireDamage: return "火焰伤害";
            case StatType.IceDamage: return "冰冻伤害";
            case StatType.LightningDamage: return "闪电伤害";
            case StatType.ElementalDamage: return "元素伤害";

            case StatType.IceResistance: return "冰冻抗性";
            case StatType.FireResistance: return "火焰抗性";
            case StatType.LightningResistance: return "闪电抗性";
            default: return "未知属性";
        }
    }

    private bool IsPercentageStat(StatType type)
    {
        // 与 UI_StatSlot.IsPercentageStat 相同的百分比属性清单：
        // 各 UI 组件各自维护一份保持自包含，避免为这个小工具引入共享依赖
        switch (type)
        {
            case StatType.CritChance:
            case StatType.CritPower:
            case StatType.ArmorReduction:
            case StatType.IceResistance:
            case StatType.FireResistance:
            case StatType.LightningResistance:
            case StatType.AttackSpeed:
            case StatType.Evasion:
                return true;
            default:
                return false;
        }
    }
}
