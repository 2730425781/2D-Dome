using UnityEngine;

/// <summary>
/// 技能身份枚举。
/// 为什么和 SkillUpgradeType 分开：
/// SkillType 表示"玩家拥有哪几个技能"，是固定的四个技能；
/// SkillUpgradeType 表示"每个技能树里选哪条升级分支"，分支数远多于技能数。
/// </summary>
public enum SkillType
{
    Dash,
    TimeEcho,
    TimeShard,
    SwordThrow,
    DomainExpansion,
}
