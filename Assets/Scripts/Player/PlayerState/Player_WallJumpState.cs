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

        // -facingDir：蹬墙跳需要向背离墙体的方向弹开（与当前朝向相反）
        player.SetVelocity(player.wallJumpForce.x * -player.facingDir, player.wallJumpForce.y);
    }

    public override void Update()
    {
        base.Update();

        if (rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }

        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
/// 蹬墙跳状态：从墙上弹开，获得一个背离墙体的初速度。
/// 
/// Enter 时 SetVelocity 使用 -facingDir：
/// facingDir 当前朝向墙体，背离的方向是 -facingDir。
/// 上升至顶点后自然进入下落状态，不需要额外计时器。
/// 
/// 为什么不用 stateTimer 控制：
/// 蹬墙跳的弧形轨迹由物理引擎自然计算，
/// 用计时器提前切出会打断自然的抛物线运动。
/// </summary>
