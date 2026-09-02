using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 技能树节点的数据资产。
/// 
/// 为什么用 ScriptableObject：
/// 每个技能树节点只需要引用同一个 SO，技能名称、描述、图标、消耗、
/// 解锁后要应用的升级数据都集中在资产里配置，不需要复制到每个 UI 节点上。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/技能数据", fileName = "技能数据设置-")]
public class SkillDataSO : ScriptableObject
{
    [Header("技能详情")]
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("解锁与升级")]
    public int cost;                 // 解锁需要的技能点
    public bool unlockedByDefault;   // 是否默认解锁（技能树根部节点用）
    public SkillType skillType;      // 对应哪个技能
    public UpgradeDate upgradeDate;  // 解锁后写入技能的具体升级数据
}

/// <summary>
/// 技能升级配置。
/// 为什么单独做成可序列化类：技能树配置里需要一个"描述这次升级"的数据结构，
/// 包含升级分支、冷却和伤害缩放，方便在 Inspector 中直接编辑。
/// </summary>
[System.Serializable]
public class UpgradeDate
{
    public SkillUpgradeType upgradeType;
    public float cooldown;
    public DamageScaleData damageScaleDate;
}
