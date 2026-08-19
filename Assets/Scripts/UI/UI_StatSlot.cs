using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
        gameObject.name = "UI-Stat - " + GetStatNameByType(statType);
        statName.text = GetStatNameByType(statType);
    }

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        playerStats = FindAnyObjectByType<Player_Stats>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.statToolTip.ShowToolTip(true, rect, statType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.statToolTip.ShowToolTip(false, null);
    }

    public void UpdateStatValue()
    {
        Stat stat = playerStats.GetStatValueByType(statType);

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
                value = playerStats.GetArmorReduction() * 100;
                break;
            case StatType.AttackSpeed:
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
                value = playerStats.GetElementalDamage(out ElementType element, 1);
                break;

            case StatType.IceResistance:
                value = playerStats.GetElementalResistance(ElementType.Ice) * 100;
                break;
            case StatType.FireResistance:
                value = playerStats.GetElementalResistance(ElementType.Fire) * 100;
                break;
            case StatType.LightningResistance:
                value = playerStats.GetElementalResistance(ElementType.Lightning) * 100;
                break;
        }
        statValue.text = IsPercentageStat(statType) ? value + "%" : value.ToString();
    }

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
