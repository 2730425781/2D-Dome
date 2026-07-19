using UnityEngine;

/// <summary>
/// 空中攻击（跳劈）状态。
/// 进入时给人物一个向下的斜向攻击速度，触地后播放落地动画并退出。
/// </summary>
public class Player_JumpAttackState : PlayerState
{
    private bool touchedGround;  // 是否已触地（触地后播放下落动画）
    private bool shouldExit;     // 是否应该退出状态

    public Player_JumpAttackState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        touchedGround = false;
        shouldExit = false;

        // 施加跳劈初始速度（由 Inspector 中的 jumpAttackVelocity 控制）
        player.SetVelocity(player.jumpAttackVelocity.x * player.facingDir, player.jumpAttackVelocity.y);
    }

    public override void Update()
    {
        // 等待动画事件后再退出，确保跳劈动画播完
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

        // 动画事件触发（triggerCalled）且已触地 → 标记退出
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