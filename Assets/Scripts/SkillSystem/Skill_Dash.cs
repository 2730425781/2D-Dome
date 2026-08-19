using UnityEngine;

/// <summary>
/// 冲刺技能：负责在冲刺开始/结束时触发已解锁的附加效果。
/// 
/// 为什么这个脚本只管效果、不管移动：
/// 冲刺移动本身由 Player_DashState 状态机控制，
/// 技能脚本只负责"冲刺前后触发什么"（分身、碎片），职责单一。
/// </summary>
public class Skill_Dash : Skill_Base
{
    bool skillTriggler = true;

    /// <summary>
    /// 由外部每帧/事件调用，交替执行开始效果和结束效果。
    /// skillTriggler 作为开关：true 表示下一次是冲刺开始，false 表示结束。
    /// </summary>
    public override void TryUseSkill()
    {
        base.TryUseSkill();
        if (skillTriggler)
        {
            OnStartEffect();
            skillTriggler = false;
        }
        else
        {
            OnEndEffect();
            skillTriggler = true;
        }
    }

    public void OnStartEffect()
    {
        // 冲刺开始时根据升级分支决定是否生成分身/碎片
        if (Unlocked(SkillUpgradeType.Dash_CloneOnStart) || Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
        {
            CreateClone();
        }

        if (Unlocked(SkillUpgradeType.Dash_ShardOnShart) || Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
        {
            CreateShard();
        }
    }

    public void OnEndEffect()
    {
        // 只有"开始和结束都生成"的分支才会在冲刺结束时再生成一次
        if (Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
        {
            CreateClone();
        }

        if (Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
        {
            CreateShard();
        }
    }

    private void CreateShard()
    {
        if (skillManager == null || skillManager.shard == null)
        {
            Debug.LogWarning("未找到 Skill_Shard,无法创建碎片");
            return;
        }
        skillManager.shard.CreateRawShard();
    }

    private void CreateClone()
    {
        skillManager.timeEcho.CreateTimeEcho();
    }
}
