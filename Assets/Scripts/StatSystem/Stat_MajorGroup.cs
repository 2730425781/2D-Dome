using System;
using UnityEngine;

/// <summary>
/// 主要属性（力量/敏捷/智力/体力）：角色的"成长基础"。
/// 这类属性不直接参与战斗数值，而是作为其他公式的加成系数，
/// 集中在 Entity_Stats 的派生公式里（体力→生命值、敏捷→暴击/闪避、
/// 力量→物理伤害/暴击威力、智力→元素伤害与抗性）。
/// 用固定字段而非字典，是因为这四项在设计上就固定不变，Inspector 可直接配置。
/// </summary>
[Serializable]
public class Stat_MajorGroup
{
    [Header("主要属性")]
    [InspectorName("敏捷")]
    [Tooltip("影响攻击速度和闪避")]
    // 暴击率 +0.3/点、闪避 +0.5/点（见 GetCritChance / GetEvasion）
    public Stat agility;

    [InspectorName("力量")]
    [Tooltip("影响物理伤害和暴击威力")]
    // 基础伤害 +1/点、暴击威力 +0.5/点（见 GetBaseDamage / GetCritPower）
    public Stat strength;

    [InspectorName("智力")]
    [Tooltip("影响元素伤害")]
    // 元素伤害直接加成、元素抗性 +0.5/点（见 GetElementalDamage / GetElementalResistance）
    public Stat intelligence;

    [InspectorName("体力")]
    [Tooltip("每点体力提供 5 点额外生命值")]
    // 每点 +5 最大生命值，与 GetMaxHealth() 中的 bonusHp 保持一致
    public Stat vitality;
}