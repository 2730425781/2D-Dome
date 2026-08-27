using UnityEngine;

/// <summary>
/// 可修改属性的统一枚举：Buff、消耗品、UI 都通过它寻址具体 Stat。
/// 分组顺序（资源/主要/攻击/防御）对应 Entity_Stats.GetStatValueByType 的 switch 分支，
/// 因此新增属性时需要同步两处。
/// ElementalDamage 是"虚拟"类型：没有对应的存储字段，只是 UI 表示元素总伤害的占位，
/// GetStatValueByType 遇到它会告警并返回 null。
/// </summary>
public enum StatType
{
    MaxHealth,
    HealthRegen,

    Strength,
    Agility,
    Intelligence,
    Vitality,

    AttackSpeed,
    Damage,
    CritChance,
    CritPower,
    ArmorReduction,

    FireDamage,
    IceDamage,
    LightningDamage,
    // 虚拟类型：无对应 Stat 字段，GetStatValueByType 会告警并返回 null
    ElementalDamage,

    Armor,
    Evasion,

    IceResistance,
    FireResistance,
    LightningResistance,

}
