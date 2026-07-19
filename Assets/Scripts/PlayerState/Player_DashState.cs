using UnityEngine;

/// <summary>
/// 冲刺状态：人物沿水平方向快速移动一段距离。
/// 冲刺期间关闭重力，撞墙时提前取消。
/// </summary>
public class Player_DashState : PlayerState
{
    private float originalGravityScale;  // 冲刺前的重力值（退出时恢复）
    private int dashDir;                 // 冲刺方向

    public Player_DashState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 冲刺方向：有输入时朝输入方向，否则朝当前朝向
        dashDir = player.moveInput.x != 0 ? ((int)player.moveInput.x) : player.facingDir;

        stateTimer = player.dashDuration;

        // 关闭重力，冲刺期间不受下落影响
        originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;
    }

    public override void Update()
    {
        base.Update();

        // 持续施加冲刺速度
        player.SetVelocity(player.dashSpeed * dashDir, 0);

        // 冲刺时间到 → 根据是否触地决定回到待机或下落
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.groundDetected ? player.idleState : player.fallState);
        }

        // 撞墙 → 提前取消冲刺
        CancelDash();
    }

    public override void Exit()
    {
        base.Exit();

        // 退出时停住并恢复重力
        player.SetVelocity(0, 0);
        rb.gravityScale = originalGravityScale;
    }

    /// <summary>
    /// 冲刺中撞墙 → 切到待机（地面）或壁滑（空中）。
    /// </summary>
    private void CancelDash()
    {
        if (player.wallDetected)
        {
            if (player.groundDetected)
            {
                stateMachine.ChangeState(player.idleState);
            }
            else
            {
                stateMachine.ChangeState(player.wallSlideState);
            }
        }
    }
}