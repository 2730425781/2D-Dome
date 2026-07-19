using UnityEngine;

/// <summary>
/// 地面状态的基类（Idle / Move 的父类）。
/// 负责检测离开地面的条件（下落、跳跃、攻击）。
/// </summary>
public class Player_GroundedState : PlayerState
{
    public Player_GroundedState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // y 速度向下且不在地面 → 进入下落状态
        if (rb.linearVelocity.y < 0 && player.groundDetected == false)
        {
            stateMachine.ChangeState(player.fallState);
        }

        // 按下跳跃键
        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.jumpState);
        }

        // 按下攻击键
        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.basicAttackState);
        }

        if (input.Player.CounterAttack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.counterAttackState);
        }
    }
}