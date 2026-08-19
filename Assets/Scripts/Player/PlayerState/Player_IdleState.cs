using UnityEngine;

/// <summary>
/// 待机状态。人物静止不动。
/// 检测输入后切换到移动状态，贴墙时保持不动。
/// </summary>
public class Player_IdleState : Player_GroundedState
{
    public Player_IdleState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 进入待机时水平速度归零
        player.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // 贴着墙且输入方向朝向墙体 → 不切换（防止贴墙时乱抖）
        if (player.moveInput.x == player.facingDir && player.wallDetected)
        {
            return;
        }

        // 有移动输入 → 进入移动状态
        if (player.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}