using UnityEngine;

/// <summary>
/// 跳跃状态（继承 AirState）。
/// 进入时给角色施加向上的跳跃速度，跃至顶点后切回下落状态。
/// </summary>
public class Player_JumpState : Player_AirState
{
    public Player_JumpState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 施加跳跃力，保留当前水平速度
        player.SetVelocity(rb.linearVelocity.x, player.jumpForce);
    }

    public override void Update()
    {
        base.Update();

        // y 速度转为向下（跳跃顶点已过）且不在跳劈状态 → 进入下落
        if (rb.linearVelocity.y < 0 && stateMachine.currentState != player.jumpAttackState)
        {
            stateMachine.ChangeState(player.fallState);
        }

        // 贴墙 → 进入壁滑
        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}