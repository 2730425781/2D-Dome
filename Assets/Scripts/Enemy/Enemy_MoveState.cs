using UnityEngine;

/// <summary>
/// 敌人巡逻状态：朝 facingDir 方向行走，走到平台边缘或撞墙时转向。
/// 因为敌人没有跳跃能力，遇到墙壁或悬崖时必须掉头而不是跳过去。
/// </summary>
public class Enemy_MoveState : Enemy_GroundState
{
    public Enemy_MoveState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 进入移动时如果已经到了边缘或撞墙，先转向再走
        // 否则会直接走出去掉落或卡在墙上
        if (!enemy.groundDetected || enemy.wallDetected)
        {
            enemy.Flip();
        }
    }

    public override void Update()
    {
        base.Update();
        // 持续朝 facingDir 方向移动
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.linearVelocity.y);

        // 前方没地面（悬崖）或撞墙 → 回到待机，下次 MoveState 会转向
        if (!enemy.groundDetected || enemy.wallDetected)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}