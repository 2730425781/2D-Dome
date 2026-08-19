using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 地面基础攻击状态，支持最多三段连击。
/// 每段攻击有独立的速度（attackVelocity），通过连击索引决定播放哪段动画。
/// </summary>
public class Player_BasicAttackState : PlayerState
{
    private const int FirstComboIndex = 1;  // 连击起始索引
    private int comboIndex = 1;             // 当前连击段数
    private int comboLimit = 3;             // 最大连击段数
    private int attackDir;                  // 攻击方向（人物朝向或输入方向）
    private bool comboAttackQueued;         // 是否已按下此攻击键（排队中）
    private float lastTimeAttacked;         // 上次攻击时间（用于超时重置连击）
    private float attackVelocityTimer;      // 攻击速度计时器

    public Player_BasicAttackState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
        // 根据 Inspector 中 attackVelocity 数组长度动态调整连击上限
        if (comboLimit != player.attackVelocity.Length)
        {
            Debug.Log("基础攻击连段上限已设置为" + player.attackVelocity.Length);
            comboLimit = player.attackVelocity.Length;
        }
    }

    public override void Enter()
    {
        base.Enter();
        comboAttackQueued = false;

        // 根据上次攻击时间判断是否重置连击索引
        ResetComboIndex();
        // 调整攻击速度
        SyncAttackSpeed();

        // 攻击方向：有输入时朝输入方向，否则朝当前朝向
        attackDir = player.moveInput.x != 0 ? ((int)player.moveInput.x) : player.facingDir;

        // 告诉 Animator 当前是第几段攻击
        animator.SetInteger("basicAttackIndex", comboIndex);

        // 施加该段攻击的初始速度
        ApplyAttackVelocity();
    }

    public override void Update()
    {
        base.Update();

        // 攻击速度衰减
        HandleAttackVelocity();

        // 按下攻击键 → 排队下一段连击
        if (input.Player.Attack.WasPressedThisDynamicUpdate())
        {
            QueueNextAttack();
        }

        // 动画事件触发 → 处理状态退出
        if (triggerCalled)
        {
            HandleStateExit();
        }
    }

    /// <summary>
    /// 排队下一次攻击（不超过连击上限）。
    /// </summary>
    private void QueueNextAttack()
    {
        if (comboIndex < comboLimit)
        {
            comboAttackQueued = true;
        }
    }

    public override void Exit()
    {
        base.Exit();

        // 连击索引递增，记录本次攻击的时间
        comboIndex++;
        lastTimeAttacked = Time.time;
    }

    /// <summary>
    /// 动画播放完毕后的退出逻辑：
    /// - 有排队连击 → 延迟进入下一段攻击
    /// - 否则回到待机状态
    /// </summary>
    private void HandleStateExit()
    {
        if (comboAttackQueued)
        {
            // 关闭当前动画，通过 Player 协程延迟进入下一段
            animator.SetBool(animBoolName, false);
            player.EnterAttackWithDelay();
        }
        else
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    /// <summary>
    /// 攻击速度计时衰减，超时后停止水平移动。
    /// </summary>
    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;
        if (attackVelocityTimer < 0)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// 应用当前段位的攻击速度。
    /// </summary>
    private void ApplyAttackVelocity()
    {
        Vector2 attackVelocity = player.attackVelocity[comboIndex - 1];
        attackVelocityTimer = player.attackVelocityDuration;
        player.SetVelocity(attackVelocity.x * attackDir, attackVelocity.y);
    }

    /// <summary>
    /// 超出连击重置时间或超出连击上限时重置回第一段。
    /// </summary>
    private void ResetComboIndex()
    {
        if (Time.time > lastTimeAttacked + player.comboResetTime)
        {
            comboIndex = FirstComboIndex;
        }
        if (comboIndex > comboLimit)
        {
            comboIndex = FirstComboIndex;
        }
    }
}
