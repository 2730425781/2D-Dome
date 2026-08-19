using UnityEngine;

/// <summary>
/// 技能树升级分支枚举。
/// 为什么用一个全局枚举而不是每个技能单独一个：
/// 技能树 UI 只需要一个统一的枚举来显示/配置所有升级节点，
/// 并且 Skill_DataSO 里一个字段就能序列化整棵树的升级配置。
/// </summary>
public enum SkillUpgradeType
{
    None,

    // Dash 树
    Dash,
    Dash_CloneOnStart, // 冲刺开始时生成分身
    Dash_CloneOnStartAndArrival, // 冲刺开始和结束时都生成分身
    Dash_ShardOnShart, // 冲刺开始时生成碎片
    Dash_ShardOnStartAndArrival, // 冲刺开始和结束时都生成碎片

    // Shard 树
    Shard, // 碎片碰到敌人或倒计时结束爆炸
    Shard_MoveToEnemy, // 碎片会飞向最近的敌人
    Shard_Multicast, // 碎片有最多 N 次充能，可以连续释放
    Shard_Teleport, // 与最后一个碎片交换位置
    Shard_TeleportHpRewind, // 交换位置时，生命百分比回到释放碎片时的值

    // Sword Throw 树
    SwordThrow, // 扔出剑造成远程伤害
    SwordThrow_Spin, // 剑停在某处旋转，像电锯一样持续伤害
    SwordThrow_Pierce, // 穿刺剑，穿透 N 个目标
    SwordThrow_Bounce, // 反弹剑，在敌人之间弹跳

    // Time Echo 树
    TimeEcho,  // 生成玩家的克隆体，克隆体可以承受伤害
    TimeEcho_SingleAttack, // 克隆体可以攻击一次
    TimeEcho_MultiAttack, // 克隆体可以攻击 N 次
    TimeEcho_ChanceToDuplicate, // 克隆体攻击时有几率再生成一个克隆体

    TimeEcho_HealWisp, // 克隆体死亡时变成精灵飞回玩家，按承受伤害的百分比治疗
    TimeEcho_CleanseWisp, // 精灵还会移除玩家的负面状态
    TimeEcho_CooldownWisp, // 精灵还会减少所有技能 N 秒冷却

    // Domain 树
    Domain_SlowingDown, // 展开领域，敌人减速 90-100%，玩家可以自由移动战斗
    Domain_EchoSpam, // 无法移动，但会持续生成 Time Echo 攻击
    Domain_ShardSpam // 无法移动，但会持续生成 Shard 攻击
}
