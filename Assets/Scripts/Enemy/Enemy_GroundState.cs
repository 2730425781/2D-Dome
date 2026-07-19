using UnityEngine;

/// <summary>
/// 敌人地面状态的基类（Idle / Move 的父类）。
/// 核心逻辑：每帧检测玩家，一旦发现就切到战斗状态。
/// 为什么不在基类中做坠落检测：
/// 敌人不需要像玩家一样跳跃和下平台，地面检测仅用于判定是否走到平台边缘切回 Idle。
/// </summary>
public class Enemy_GroundState : EnemyState
{
    public Enemy_GroundState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        // 检测到玩家 → 进入战斗状态（巡逻/待机姿态被中断）
        if (enemy.PlayerDetection())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}