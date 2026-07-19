using UnityEngine;

/// <summary>
/// 死亡状态：禁用输入和物理模拟，仅播放死亡动画。
/// 之所以禁用 Input 和 Rigidbody，是为了防止玩家死亡后还能移动或受物理影响。
/// Rigidbody.simulated = false 会同时关闭碰撞和重力。
/// </summary>
public class Player_DeathState : PlayerState
{
    public Player_DeathState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        // 玩家死亡后不能再操作，也不应被敌人物理推开
        input.Disable();
        rb.simulated = false;
    }
}