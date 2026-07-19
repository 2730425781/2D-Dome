using UnityEngine;

/// <summary>
/// 地面移动状态。
/// 根据输入方向设置水平速度，无输入或贴墙时切回待机。
/// </summary>
public class Player_MoveState : Player_GroundedState
{
    public Player_MoveState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (player.moveInput.x == 0)
        {
            // 松开方向键 → 回到待机
            stateMachine.ChangeState(player.idleState);
        }
        else if (player.wallDetected && player.moveInput.x * player.facingDir > 0)
        {
            // 输入方向朝向墙体且正在贴墙 → 切待机（避免顶墙抖动）
            stateMachine.ChangeState(player.idleState);
        }
        else
        {
            // 正常移动：按输入方向施加地面移动速度
            player.SetVelocity(player.moveInput.x * player.moveSpeed, rb.linearVelocity.y);
        }
    }
}