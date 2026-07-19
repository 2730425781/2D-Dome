using UnityEngine;

/// <summary>
/// 蹬墙跳状态：从墙上弹开，获得一个背离墙体的初速度。
/// 上升至顶点后自然进入下落状态。
/// </summary>
public class Player_WallJumpState : PlayerState
{
    public Player_WallJumpState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 给予背离墙体的跳跃速度（facingDir 取反 = 离墙方向）
        player.SetVelocity(player.wallJumpForce.x * -player.facingDir, player.wallJumpForce.y);
    }

    public override void Update()
    {
        base.Update();

        // 跳跃顶点已过 → 进入下落状态
        if (rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }

        // 触地 → 回到待机
        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}