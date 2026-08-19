using System.Text;
using TMPro;
using UnityEngine;

public class UI_ItemToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemType;
    [SerializeField] private TextMeshProUGUI itemInfo;

    public void ShowToolTip(bool show, RectTransform targetRect, Inventory_Item item)
    {
        base.ShowToolTip(show, targetRect);

        itemName.text = item.itemDate.itemName;
        itemType.text = GetItemTypeName(item.itemDate.itemType);
        itemInfo.text = GetItemInfo(item);
    }

    public string GetItemInfo(Inventory_Item item)
    {
        if (item.itemDate.itemType == ItemType.Material)
        {
            return "合成用材料";
        }
        if (item.itemDate.itemType == ItemType.Consumable)
        {
            return item.itemDate.itemEffect.effectDescription;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("");

        foreach (var mod in item.modifiers)
        {
            string modType = GetStatNameByType(mod.statType);
            string modValue = IsPercentageStat(mod.statType) ? mod.value.ToString() + "%" : mod.value.ToString();
            sb.AppendLine("+ " + modValue + " " + modType);
        }

        if (item.itemEffect != null)
        {
            sb.AppendLine("");
            sb.AppendLine("特殊效果:");
            sb.AppendLine(item.itemEffect.effectDescription);
        }

        return sb.ToString();
    }

    // 物品类型显示为中文，和属性名保持同样的本地化风格
    private string GetItemTypeName(ItemType type)
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