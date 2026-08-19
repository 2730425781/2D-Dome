using UnityEngine;

/// <summary>
/// 玩家技能管理器：集中持有玩家拥有的所有技能实例。
/// 
/// 为什么要集中管理：
/// 1. 技能树解锁时通过 GetSkillByType 找到具体技能，把升级数据写进去；
/// 2. 全局效果（如 Wisp 减少所有技能冷却）需要遍历全部技能；
/// 3. 技能脚本挂成 Player 的子物体，管理器在 Awake 里自动收集，
///    不需要在 Inspector 中逐个拖引用。
/// </summary>
public class Player_SkillManager : MonoBehaviour
{
    public Skill_Base[] allSkills;
    public Skill_Dash dash { get; private set; }
    public Skill_Shard shard { get; private set; }
    public Skill_TimeEcho timeEcho { get; private set; }
    public Skill_SwordThrow swordThrow { get; private set; }
    public Skill_DomainExpansion domainExpansion { get; private set; }

    private void Awake()
    {
        // 用 GetComponentInChildren 自动收集，而不是手动赋值：
        // 新增技能时只要挂到 Player 子物体上就会自动被找到
        dash = GetComponentInChildren<Skill_Dash>();
        shard = GetComponentInChildren<Skill_Shard>();
        timeEcho = GetComponentInChildren<Skill_TimeEcho>();
        swordThrow = GetComponentInChildren<Skill_SwordThrow>();
        domainExpansion = GetComponentInChildren<Skill_DomainExpansion>();

        allSkills = GetComponentsInChildren<Skill_Base>();

        // 某个技能没有挂载时给出提示，避免运行时找不到技能却没有任何反馈
        if (dash == null)
        {
            Debug.Log("dash为空");
        }
        if (shard == null)
        {
            Debug.Log("shard为空");
        }
        if (timeEcho == null)
        {
            Debug.Log("timeEcho为空");
        }
    }

    /// <summary>
    /// 减少所有技能的冷却时间（Wisp 效果用）。
    /// </summary>
    public void ReduceAllSkillCooldown(float amount)
    {
        foreach (var skill in allSkills)
        {
            skill.ReduceCoolDownBy(amount);
        }
    }

    /// <summary>
    /// 按技能类型返回对应实例，供技能树解锁时写入升级数据。
    /// 为什么用 switch 而不是字典：
    /// 技能类型是编译期固定的枚举，switch 简单直观，
    /// 新增技能时编译器也能提醒这里漏了分支。
    /// </summary>
    public Skill_Base GetSkillByType(SkillType type)
    {
        switch (type)
        {
            case SkillType.Dash: return dash;
            case SkillType.TimeShard: return shard;
            case SkillType.TimeEcho: return timeEcho;
            case SkillType.SwordThrow: return swordThrow;
            case SkillType.DomainExpansion: return domainExpansion;

            default: return null;
        }
    }
}
