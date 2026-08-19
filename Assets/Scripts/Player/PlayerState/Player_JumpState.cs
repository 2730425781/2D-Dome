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
        player.SetVelocity(rb.linearVelocity.x, player.jumpForce);
    }

    public override void Update()
    {
        base.Update();

        // currentState != jumpAttackState 防止空中攻击时：
        // AirState 已读到 Attack 按键并切到 jumpAttackState，
        // 跳跃不应再因此切回下落
        if (rb.linearVelocity.y < 0 && stateMachine.currentState != player.jumpAttackState)
        {
            stateMachine.ChangeState(player.fallState);
        }

        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}
