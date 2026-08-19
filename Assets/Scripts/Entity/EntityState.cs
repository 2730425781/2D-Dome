using UnityEngine;

/// <summary>
/// 实体状态的抽象基类。
/// 
/// EntityState 设计中几个关键选择的原因：
/// 1. 不持有 Entity 引用而由子类持有（如 PlayerState、EnemyState），
///    因为 Player 和 Enemy 有不同的组件和方法签名，统一定义到基类反而耦合。
/// 2. animBoolName 用 string 而非 enum：灵活，子类/子状态可以复用同一个动画参数。
/// 3. UpdateAnimationParameters 抽出为虚方法：让 PlayerState 和 EnemyState 各自同步不同的参数集。
/// 4. triggerCalled 由 Animation Event 设置，不在代码中自动清零：保证状态自己决定何时消费标记。
/// </summary>
public abstract class EntityState
{
    protected StateMachine stateMachine;
    protected string animBoolName;          // 对应 Animator Controller 中的 bool 参数名
    protected Animator animator;
    protected Rigidbody2D rb;
    protected Entity_Stats stats;
    protected float stateTimer;             // 通用倒计时定时器，子类按需使用
    protected bool triggerCalled;           // 由 Animation Event 设置为 true，状态逻辑读取后决定下一步

    public EntityState(StateMachine stateMachine, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    /// <summary>
    /// 进入状态：打开 Animator 中对应的 bool 参数，重置 trigger 标记。
    /// </summary>
    public virtual void Enter()
    {
        animator.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    /// <summary>
    /// 每帧更新：倒计时、更新动画参数。
    /// 动画参数同步放在这里而不是在 Player.Update() 中：
    /// 不同实体（Player/Enemy）需要同步的参数不同，各自在派生类中实现。
    /// </summary>
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();
    }

    /// <summary>
    /// 退出状态：关闭 Animator 中对应的 bool 参数。
    /// </summary>
    public virtual void Exit()
    {
        animator.SetBool(animBoolName, false);
    }

    /// <summary>
    /// 由动画事件调用，标记 triggerCalled = true。
    /// 为什么不用事件/回调而用标记轮询：
    /// 状态机每帧 Update，标记检查是自然融入当前 Update 流程的，比异步回调更可控。
    /// </summary>
    public void AnimationTrigger()
    {
        triggerCalled = true;
    }

    /// <summary>
    /// 用于同步动画参数（如速度、战斗倍率等）。
    /// 设计为独立虚方法而不是直接写在 Update 里面：
    /// PlayerState 和 EnemyState 各自覆写以获得不同的参数同步逻辑。
    /// </summary>
    public virtual void UpdateAnimationParameters()
    {
    }

    public void SyncAttackSpeed()
    {
        float attackspeed = stats.offense.attackSpeed.GetValue();
        animator.SetFloat("attackSpeedmultiplier", attackspeed);
    }
}
