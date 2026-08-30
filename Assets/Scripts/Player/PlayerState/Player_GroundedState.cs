using UnityEngine;

/// <summary>
/// 地面状态的基类（Idle / Move 的父类）。
/// 负责检测离开地面的条件（下落、跳跃、攻击）。
/// 反击冷却记录在 Player_Combat 上，不在此处维护——每次状态切换 stateTimer 会被重置，
/// 计时就会不准确。用 Player_Combat.CanCounter 判断冷却，它跨状态迁移持续有效。
/// </summary>
public class Player_GroundedState : PlayerState
{
    private Player_Combat playerCombat;
    public Player_GroundedState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
        playerCombat = player.GetComponent<Player_Combat>();
    }

    public override void Update()
    {
        base.Update();

        if (rb.linearVelocity.y < 0 && player.groundDetected == false)
        {
            stateMachine.ChangeState(player.fallState);
        }

        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.jumpState);
        }

        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.basicAttackState);
        }

        if (playerCombat != null && playerCombat.CanCounter && input.Player.CounterAttack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.counterAttackState);
        }
        
        if (input.Player.RangeAttack.WasPressedThisFrame() && skillManager.swordThrow.CanUseSkill())
        {
            stateMachine.ChangeState(player.swordThrowState);
        }
    }
}
