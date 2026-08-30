using System;
using UnityEngine;

/// <summary>
/// 下落状态（继承 AirState）。
/// 控制下落时的状态切换：落地 → idle，贴墙 → 壁滑。
/// </summary>
public class Player_FallState : Player_AirState
{
    public Player_FallState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        animator.SetBool("inAir", true);
        // 可在此处添加进入下落时的逻辑（如重置双跳计数等）
    }

    public override void Update()
    {
        base.Update();

        // 落地且 y 速度接近 0 → 切回待机
        if (player.groundDetected && MathF.Abs(rb.linearVelocity.y) < 0.001f)
        {
            stateMachine.ChangeState(player.idleState);
        }

        // 贴墙 → 切到壁滑
        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        animator.SetBool("inAir", false);
    }

}