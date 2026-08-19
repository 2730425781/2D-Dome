using UnityEngine;

/// <summary>
/// 冲刺状态：人物沿水平方向快速移动一段距离。
/// 冲刺期间关闭重力，撞墙时提前取消。
/// </summary>
public class Player_DashState : PlayerState
{
    private float originalGravityScale;
    private int dashDir;

    public Player_DashState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //skillManager.dash.OnStartEffect();
        skillManager.dash.TryUseSkill();
        player.vfx.DoImageEchoEffect(player.dashDuration);

        dashDir = player.moveInput.x != 0 ? ((int)player.moveInput.x) : player.facingDir;

        stateTimer = player.dashDuration;

        // gravityScale = 0 而非每帧覆盖 velocity.y：
        // 重力每帧重新计算 velocity，不关重力就得每帧覆盖 y
        originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;

        player.health.SetCanTakedamage(false);
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocity(player.dashSpeed * dashDir, 0);

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.groundDetected ? player.idleState : player.fallState);
        }

        CancelDash();
    }

    public override void Exit()
    {
        base.Exit();

        //skillManager.dash.OnEndEffect();
        skillManager.dash.TryUseSkill();

        player.SetVelocity(0, 0);
        rb.gravityScale = originalGravityScale;

        player.health.SetCanTakedamage(true);
    }

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
