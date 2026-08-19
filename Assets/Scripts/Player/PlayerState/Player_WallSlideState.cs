using Unity.Burst.Intrinsics;
using UnityEngine;

public class Player_WallSlideState : PlayerState
{
    public Player_WallSlideState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Update()
    {
        base.Update();

        HandleWallSlide();

        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.wallJumpState);
        }

        if (player.wallDetected == false && rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }

        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
            if (player.moveInput.x != player.facingDir)
            {
                player.Flip();
            }
        }
    }

    private void HandleWallSlide()
    {
        // [Why] moveInput.x 而非固定方向:
        //   悬墙时朝向墙体的同时允许抽身或反向, 不是锁死在墙上。
        // [Why] y < 0 用原始速度（加速下滑）, 否则加 slowMultiplier 控制下滑速度
        if (player.moveInput.y < 0)
            player.SetVelocity(player.moveInput.x, rb.linearVelocity.y);
        else
            player.SetVelocity(player.moveInput.x, rb.linearVelocity.y * player.wallSlideSlowMultiplier);
    }
}
