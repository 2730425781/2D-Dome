using UnityEngine;

/// <summary>
/// Time Echo（时间回响）技能：生成一个会攻击/承伤/治疗玩家的克隆体。
/// 
/// 为什么所有升级判断都放在这个技能类里：
/// SkillObject_TimeEcho 是临时实体，不知道技能树配置，
/// 通过这里暴露的 GetXxx() 方法读取"当前升级应该有什么行为"，
/// 让数据判断集中在一处，实体只负责执行。
/// </summary>
public class Skill_TimeEcho : Skill_Base
{
    [SerializeField] private GameObject timeEchoPrefab;
    [SerializeField] private float timeEchoDuration;
    [Header("攻击升级")]
    [SerializeField] private int maxAttacks = 3;
    [SerializeField] private float duplicateChance = 0.3f;

    [Header("治疗升级")]
    [SerializeField] private float damagePercentHealed;
    [SerializeField] private int cooldownReducedInSeconds;

    public float GetPercentOfDamageHealed()
    {
        // 只有"治疗精灵"分支才回血；其他分支返回 0，避免未解锁时误治疗
        if (ShouldBeWisp() == false)
            return 0;

        return damagePercentHealed;
    }

    public float GetCooldownReduceInSeconds()
    {
        if (upgradeType != SkillUpgradeType.TimeEcho_CooldownWisp)
            return 0;

        return cooldownReducedInSeconds;
    }

    public bool CanRemoveNegativeEffects()
    {
        return upgradeType == SkillUpgradeType.TimeEcho_CleanseWisp;
    }

    /// <summary>
    /// 克隆体死亡后是否变成精灵飞回玩家。
    /// 三个"精灵"分支共享同一套移动/治疗逻辑，只是附加效果不同。
    /// </summary>
    public bool ShouldBeWisp()
    {
        return upgradeType == SkillUpgradeType.TimeEcho_HealWisp
            || upgradeType == SkillUpgradeType.TimeEcho_CleanseWisp
            || upgradeType == SkillUpgradeType.TimeEcho_CooldownWisp;
    }

    public float GetDuplicateChance()
    {
        if (upgradeType != SkillUpgradeType.TimeEcho_ChanceToDuplicate)
        {
            return 0;
        }

        return duplicateChance;
    }

    public int GetMaxAttacks()
    {
        // 不同分支决定克隆体能攻击几次：单次、多次或不能攻击
        if (upgradeType == SkillUpgradeType.TimeEcho_SingleAttack)
        {
            return 1;
        }
        if (upgradeType == SkillUpgradeType.TimeEcho_MultiAttack || upgradeType == SkillUpgradeType.TimeEcho_ChanceToDuplicate)
        {
            return maxAttacks;
        }
        return 0;
    }

    public float GetEchoDuration()
    {
        return timeEchoDuration;
    }

    public override void TryUseSkill()
    {
        if (!CanUseSkill())
        {
            return;
        }

        CreateTimeEcho();
    }

    /// <summary>
    /// 生成 Time Echo 实体；targetPosition 可选，供"复制"分支在命中点生成新克隆体。
    /// </summary>
    public void CreateTimeEcho(Vector3? targetPosition = null)
    {
        Vector3 position = targetPosition ?? transform.position + new Vector3(1, 0);

        GameObject timeEcho = Instantiate(timeEchoPrefab, position, Quaternion.identity);
        timeEcho.GetComponent<SkillObject_TimeEcho>().SetUpEcho(this);
    }
}
