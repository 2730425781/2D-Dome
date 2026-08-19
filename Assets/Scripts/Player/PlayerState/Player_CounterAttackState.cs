using UnityEngine;

/// <summary>
/// 反击状态：持有两个 Animator 阶段（counterAttack1 → counterAttack2）。
/// 
/// 设计原因：
/// 1. counterAttack1 是反击的前摇/等待帧，此时玩家处于无敌反击窗口
/// 2. 如果窗口内侦测到可反击目标 → 设置 counterAttack2 = true → Animator 切到第二段动画
/// 3. 如果窗口超时且没有目标 → 直接退出回到待机（不播放第二段）
/// 4. 第二段动画播放完毕后，由动画事件 CurrentStateTrigger 触发退出
/// 
/// 冷却时间记录在 Player_Combat 组件上，进入此状态时调用 MarkCounterUsed()
/// 记录时间戳，冷却结束前 GroundedState 中的 CanCounter 会阻止再次进入。
/// </summary>
public class Player_CounterAttackState : PlayerState
{
    private Player_Combat playerCombat;
    private bool counteredSomebody;  // 是否成功反击到了目标

    public Player_CounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        playerCombat = player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();

        // 不管反击成功还是失败，进入就记冷却
        playerCombat?.MarkCounterUsed();

        stateTimer = playerCombat != null ? playerCombat.GetCounterRecoveryDuration() : 0;
        counteredSomebody = playerCombat != null && playerCombat.CounterAttack2();
        animator.SetBool("counterAttack2", counteredSomebody);
    }

    public override void Update()
    {
        base.Update();
        player.SetVelocity(0, rb.linearVelocity.y);
        // 动画播完（CurrentStateTrigger 事件触发）→ 退出回到待机
        if (triggerCalled)
        {
            animator.SetBool("counterAttack2", false);
            stateMachine.ChangeState(player.idleState);
        }

        // 反击窗口超时且没有成功反击 → 退出（避免一直摆着反击姿势）
        if (stateTimer <= 0 && !counteredSomebody)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
