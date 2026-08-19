using UnityEngine;

/// <summary>
/// 骷髅敌人。继承自 Enemy 并实现 ICounterable。
/// 
/// 实现 ICounterable 而不是在 Enemy 基类中处理的原因：
/// 不是所有敌人都应该可以被反击（例如 Boss 可能免疫），
/// 让具体的敌人类型自己决定是否实现 ICounterable 更灵活。
/// 
/// HandleCounter 中的 CanCountered 检查：只有在反击窗口开启时才触发眩晕，
/// 避免玩家随便在任何时候按反击都能打断敌人攻击。
/// </summary>
public class Enemy_Skeleton : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeCountered; }

    protected override void Awake()
    {
        base.Awake();
        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        battleState = new Enemy_BattleState(this, stateMachine, "battle");
        deathState = new Enemy_DeathState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    /// <summary>
    /// 实现 ICounterable.HandleCounter。只有当 CanCountered = true 时才有效。
    /// 切到眩晕状态后敌人会被击飞并无法行动一段时间。
    /// </summary>
    public void HandleCounter()
    {
        if (!CanBeCountered)
        {
            return;
        }
        stateMachine.ChangeState(stunnedState);
    }
}