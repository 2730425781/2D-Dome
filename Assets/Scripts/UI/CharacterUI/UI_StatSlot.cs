using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 属性面板中的一行：显示单个属性数值，悬停时弹出该属性的说明 ToolTip。
/// 为什么一个属性一个组件：每种属性的显示规则（是否百分比、读哪个接口）
/// 都不同，拆成组件后只需在 Inspector 里选 StatType 就能复用同一套逻辑，
/// 新增属性不需要改这个类。
/// </summary>
public class UI_StatSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Player_Stats playerStats;
    private RectTransform rect;
    private UI ui;

    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statName;
    [SerializeField] private TextMeshProUGUI statValue;

    private void OnValidate()
    {
        // 编辑期同步物体名与显示名：
        // 改 StatType 后层级里一眼认出是哪一行，且界面文案不用手动填
        gameObject.name = "UI-Stat - " + GetStatNameByType(statType);
        statName.text = GetStatNameByType(statType);
    }

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        // 场景中只有单一玩家，按类型直接查找属性来源
        playerStats = FindAnyObjectByType<Player_Stats>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 悬停显示属性说明；传入自身 rect 让 ToolTip 定位到这一行旁边
        ui.statToolTip.ShowToolTip(true, rect, statType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.statToolTip.ShowToolTip(false, null);
    }

    public void UpdateStatValue()
    {
        // 背包数据变化（穿戴/卸下/增益）时由 UI_PlayerStats 统一调用刷新
        Stat stat = playerStats.GetStatValueByType(statType);

        // ElementalDamage 是三种元素合并的计算值，没有对应的 Stat 实例，
        // GetStatValueByType 对它返回 null 属正常，所以单独放行；
        // 其余属性为 null 说明数据没配好，提示而不是静默显示 0
        if (stat == null && statType != StatType.ElementalDamage)
        {
            Debug.LogWarning($"当前\"{statType}\"属性值还未分配给玩家");
            return;
        }

        float value = 0;

        switch (statType)
        {
            case StatType.Strength:
                value = playerStats.majorStat.strength.GetValue();
                break;
            case StatType.Agility:
                value = playerStats.majorStat.agility.GetValue();
                break;
            case StatType.Intelligence:
                value = playerStats.majorStat.intelligence.GetValue();
                break;
            case StatType.Vitality:
                value = playerStats.majorStat.vitality.GetValue();
                break;

            case StatType.Damage:
                value = playerStats.GetBaseDamage();
                break;
            case StatType.CritChance:
                value = playerStats.GetCritChance();
                break;
            case StatType.CritPower:
                value = playerStats.GetCritPower();
                break;
            case StatType.ArmorReduction:
                // 减伤接口返回 0~1 比例，乘 100 转成百分比显示
                value = playerStats.GetArmorReduction() * 100;
                break;
            case StatType.AttackSpeed:
                // 攻速按 0~1 存储，乘 100 显示为百分比（如 0.5 → 50%）
                value = playerStats.offense.attackSpeed.GetValue() * 100;
                break;

            case StatType.MaxHealth:
                value = playerStats.GetMaxHealth();
                break;
            case StatType.HealthRegen:
                value = playerStats.resources.healthRegen.GetValue();
                break;
            case StatType.Evasion:
                value = playerStats.GetEvasion();
                break;
            case StatType.Armor:
                value = playerStats.GetBaseArmor();
                break;

            case StatType.IceDamage:
                value = playerStats.offense.iceDamage.GetValue();
                break;
            case StatType.FireDamage:
                value = playerStats.offense.fireDamage.GetValue();
                break;
            case StatType.LightningDamage:
                value = playerStats.offense.lightningDamage.GetValue();
                break;
            case StatType.ElementalDamage:
                // 元素伤害没有 Stat 实例，走合并计算接口：
                // 取三种元素中最高的一项并附带对应元素类型；scaleFactor=1 表示按原值显示
                value = playerStats.GetElementalDamage(out ElementType element, 1);
                break;

            case StatType.IceResistance:
                // 抗性接口返回 0~0.75 的减伤比例，乘 100 转成百分比显示
                value = playerStats.GetElementalResistance(ElementType.Ice) * 100;
                break;
            case StatType.FireResistance:
                value = playerStats.GetElementalResistance(ElementType.Fire) * 100;
                break;
            case StatType.LightningResistance:
                value = playerStats.GetElementalResistance(ElementType.Lightning) * 100;
                break;
        }
        // 百分比属性加 % 后缀，其余直接显示数值；百分比判定集中在下方的 IsPercentageStat
        statValue.text = IsPercentageStat(statType) ? value + "%" : value.ToString();
    }

    // 属性中文名与物品 ToolTip、属性说明使用同一套命名，
    // 避免同一属性在不同界面叫法不一致让玩家困惑
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

    // 集中定义哪些属性按百分比展示：
    // 数据层以 0~1 存储（如暴击率 0.5），显示层需要转成 50%，
    // 判定必须与上面 switch 中 ×100 的项保持一致
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
