using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Entity_Stats : MonoBehaviour
{
    [Header("属性设置")]
    [InspectorName("默认设置")]
    public StatDateSO defaultStatSetup;

    [Header("基础属性")]
    [InspectorName("资源属性")]
    public Stat_ResourceGroup resources;

    [InspectorName("攻击属性")]
    public Stat_OffenseGroup offense;

    [InspectorName("防御属性")]
    public Stat_DefenseGroup defense;

    [InspectorName("主要属性")]
    public Stat_MajorGroup majorStat;

    protected virtual void Awake()
    {

    }

    public AttackDate GetAttackDate(DamageScaleDate damageScale)
    {
        return new AttackDate(this, damageScale);
    }

    public float GetElementalDamage(out ElementType element, float scaleFactor = 1) //需要重写
    {
        float fireDamage = offense.fireDamage.GetValue();
        float iceDamage = offense.iceDamage.GetValue();
        float lightningDamage = offense.lightningDamage.GetValue();
        float bonusElementalDamage = majorStat.intelligence.GetValue();

        float highestDamage = fireDamage;
        element = ElementType.Fire;
        if (iceDamage > highestDamage)
        {
            highestDamage = iceDamage;
            element = ElementType.Ice;
        }
        if (lightningDamage > highestDamage)
        {
            highestDamage = lightningDamage;
            element = ElementType.Lightning;
        }
        if (highestDamage <= 0)
        {
            element = ElementType.None;
            return 0;
        }
        float finaldamage = highestDamage + bonusElementalDamage;
        return finaldamage * scaleFactor;
    }

    public float GetElementalResistance(ElementType element)
    {
        float baseResistance = 0;
        float bonusResistance = majorStat.intelligence.GetValue() * 0.5f;

        switch (element)
        {
            case ElementType.Fire:
                baseResistance = defense.fireRes.GetValue();
                break;
            case ElementType.Ice:
                baseResistance = defense.iceRes.GetValue();
                break;
            case ElementType.Lightning:
                baseResistance = defense.lightningRes.GetValue();
                break;
        }

        float resistance = baseResistance + bonusResistance;
        float resistanceCap = 75f;
        float finalResistance = Mathf.Clamp(resistance, 0, resistanceCap) / 100;
        return finalResistance;
    }

    public float GetPhysicalDamage(out bool isCrit, float scaleFactor = 1)
    {
        float baseDamage = GetBaseDamage();
        float baseCritChance = GetCritChance();
        float baseCritPower = GetCritPower() / 100;

        isCrit = UnityEngine.Random.Range(0, 100) < baseCritChance;
        float finalDamage = isCrit ? baseDamage * baseCritPower : baseDamage;

        return finalDamage * scaleFactor;
    }

    public float GetBaseDamage() => offense.damage.GetValue() + majorStat.strength.GetValue();
    public float GetCritChance() => offense.critChance.GetValue() + majorStat.agility.GetValue() * 0.3f;
    public float GetCritPower() => offense.critPower.GetValue() + majorStat.strength.GetValue() * 0.5f;

    public float GetArmorReduction()
    {
        float finalReduction = offense.armorReduction.GetValue() / 100;
        return finalReduction;
    }

    public float GetArmorMitigation(float armorReduction)
    {
        float baseArmor = GetBaseArmor();
        float reductionMultiplier = Mathf.Clamp(1 - armorReduction, 0, 1);
        float finalArmor = baseArmor * reductionMultiplier;

        float mitigation = finalArmor / (finalArmor + 100);
        float mitigationCap = 0.85f;

        float finalMitigation = Mathf.Clamp(mitigation, 0, mitigationCap);
        return finalMitigation;
    }

    public float GetBaseArmor() => defense.armor.GetValue() + majorStat.vitality.GetValue();

    /// <summary>
    /// 计算最终最大生命值：基础值 + 体力 * 5。
    /// </summary>
    public float GetMaxHealth()
    {
        float baseHp = resources.maxHealth.GetValue();
        float bonusHp = majorStat.vitality.GetValue() * 5;
        float finalHp = baseHp + bonusHp;
        return finalHp;
    }

    public float GetEvasion()
    {
        float baseEvasion = defense.evasion.GetValue();
        float bonusEvasion = majorStat.agility.GetValue() * 0.5f;

        float totalEvasion = baseEvasion + bonusEvasion;
        float evasionCap = 100f;
        float finalEvasion = Mathf.Clamp(totalEvasion, 0, evasionCap);
        return finalEvasion;
    }

    public Stat GetStatValueByType(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return resources.maxHealth;
            case StatType.HealthRegen: return resources.healthRegen;

            case StatType.Strength: return majorStat.strength;
            case StatType.Agility: return majorStat.agility;
            case StatType.Intelligence: return majorStat.intelligence;
            case StatType.Vitality: return majorStat.vitality;

            case StatType.AttackSpeed: return offense.attackSpeed;
            case StatType.Damage: return offense.damage;
            case StatType.CritChance: return offense.critChance;
            case StatType.CritPower: return offense.critPower;
            case StatType.ArmorReduction: return offense.armorReduction;

            case StatType.FireDamage: return offense.fireDamage;
            case StatType.IceDamage: return offense.iceDamage;
            case StatType.LightningDamage: return offense.lightningDamage;

            case StatType.Armor: return defense.armor;
            case StatType.Evasion: return defense.evasion;

            case StatType.IceResistance: return defense.iceRes;
            case StatType.FireResistance: return defense.fireRes;
            case StatType.LightningResistance: return defense.lightningRes;

            default:
                if (type != StatType.ElementalDamage)
                {
                    Debug.LogWarning($"StatType {type} not implemented yet.");
                }
                return null;
        }
    }

    [ContextMenu("更新默认属性")]
    public void ApplyDefaultStatSetup()
    {
        if (defaultStatSetup == null)
        {
            Debug.Log("No default stat setup assigned");
            return;
        }

        resources.maxHealth.SetBaseValue(defaultStatSetup.maxHealth);
        resources.healthRegen.SetBaseValue(defaultStatSetup.healthRegen);

        majorStat.strength.SetBaseValue(defaultStatSetup.strength);
        majorStat.agility.SetBaseValue(defaultStatSetup.agility);
        majorStat.intelligence.SetBaseValue(defaultStatSetup.intelligence);
        majorStat.vitality.SetBaseValue(defaultStatSetup.vitality);

        offense.attackSpeed.SetBaseValue(defaultStatSetup.attackSpeed);
        offense.damage.SetBaseValue(defaultStatSetup.damage);
        offense.critChance.SetBaseValue(defaultStatSetup.critChance);
        offense.critPower.SetBaseValue(defaultStatSetup.critPower);
        offense.armorReduction.SetBaseValue(defaultStatSetup.armorReduction);

        offense.iceDamage.SetBaseValue(defaultStatSetup.iceDamage);
        offense.fireDamage.SetBaseValue(defaultStatSetup.fireDamage);
        offense.lightningDamage.SetBaseValue(defaultStatSetup.lightningDamage);

        defense.armor.SetBaseValue(defaultStatSetup.armor);
        defense.evasion.SetBaseValue(defaultStatSetup.evasion);

        defense.iceRes.SetBaseValue(defaultStatSetup.iceResistance);
        defense.fireRes.SetBaseValue(defaultStatSetup.fireResistance);
        defense.lightningRes.SetBaseValue(defaultStatSetup.lightningResistance);
    }
}