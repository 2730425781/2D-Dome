using UnityEngine;

/// <summary>
/// 空中攻击（跳劈）状态。
/// 进入时给人物一个向下的斜向攻击速度，触地后播放落地动画并退出。
/// </summary>
public class Player_JumpAttackState : PlayerState
{
    private bool touchedGround;
    private bool shouldExit;
    public Player_JumpAttackState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        touchedGround = false;
        shouldExit = false;

        player.SetVelocity(player.jumpAttackVelocity.x * player.facingDir, player.jumpAttackVelocity.y);
    }

    public override void Update()
    {
        // shouldExit 延迟退出而非直接 ChangeState：
        // 避免 AnimationTrigger 和 Exit().SetBool 在相邻帧冲突
        // Animator 松开 trigger 后立即被关闭 bool 会报 beforeDecrement >= 0
        if (shouldExit)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        base.Update();

        // 首次触地时：触发落地动画触发器，水平速度归零
        if (player.groundDetected && touchedGround == false)
        {
            touchedGround = true;
            animator.SetTrigger("jumpAttackTrigger");
            player.SetVelocity(0, rb.linearVelocity.y);
        }

        // 动画事件触发（triggerCalled）且已触地 -> 标记退出
        if (triggerCalled && touchedGround)
        {
            shouldExit = true;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
