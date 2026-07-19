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
/// 之所以不拆成两个独立状态，是因为两段动画共享同一个逻辑上下文：
/// - counteredSombody 标记需要跨阶段保持
/// - 退出条件共享 triggerCalled
/// </summary>
public class Player_AttackCounterState : PlayerState
{
    private Player_Combat playerCombat;
    private bool counteredSombody;  // 是否成功反击到了目标

    public Player_AttackCounterState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        playerCombat = player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();
        counteredSombody = false;
        // 进入状态时保证 counterAttack2 为 false，以免 Animator 直接跳到第二段
        animator.SetBool("counterAttack2", false);
        stateTimer = playerCombat.GetCounterDuration();
    }

    public override void Update()
    {
        base.Update();

        // 只有第一段（counterAttack1）期间检测反击，第二段（counterAttack2）不需要再检测
        if (playerCombat.CounterAttack2())
        {
            counteredSombody = true;
            animator.SetBool("counterAttack2", true);
        }

        // 动画播完（CurrentStateTrigger 事件触发）→ 退出回到待机
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }

        // 反击窗口超时且没有成功反击 → 退出（避免一直摆着反击姿势）
        if (stateTimer <= 0 && !counteredSombody)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}