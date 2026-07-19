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

        // 处理壁滑物理（速度、翻转）
        HandleWallSlide();

        // 按下跳跃键 → 从墙上跳离（wallJumpState）
        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.wallJumpState);
        }

        // 不再贴墙并且正在下落 → 回到下落状态
        if (player.wallDetected == false && rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }

        // 触地 → 回到待机状态，必要时翻转朝向
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
        if (player.moveInput.y < 0)
            player.SetVelocity(player.moveInput.x, rb.linearVelocity.y);
        else
            player.SetVelocity(player.moveInput.x, rb.linearVelocity.y * player.wallSlideSlowMultiplier);
    }
}