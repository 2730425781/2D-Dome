using System;
using System.Collections;
using UnityEngine;

public class Player : Entity
{
    // 玩家死亡时通知所有订阅者（比如敌人停止战斗行为）
    public static event Action OnPlayerADeath;

    public PlayerInputSet input { get; private set; }

    // ---------- 状态实例 ----------
    // 每个状态对应 Animator Controller 中的一个动画状态，animBoolName 作为桥梁
    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }
    public Player_DashState dashState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }
    public Player_JumpAttackState jumpAttackState { get; private set; }
    public Player_DeathState deathState { get; private set; }
    public Player_AttackCounterState counterAttackState { get; private set; }

    [Header("攻击设置")]
    public Vector2[] attackVelocity;           // 基础攻击各段的前冲速度（x影响位移，y影响高度）
    public Vector2 jumpAttackVelocity;         // 跳劈时向下斜冲的速度
    public float attackVelocityDuration = 0.1f; // 攻击速度只维持短暂瞬间，避免角色一直向前滑
    public float comboResetTime = 0.3f;        // 连击间隔超过此值则重置回第一段
    private Coroutine queueAttackCo;

    [Header("移动设置")]
    public float moveSpeed;                    // 地面移动速度
    public float jumpForce = 5;                // 跳跃力
    public float inAirMoveMulyiplier = 0.65f;  // 空中移动倍率<1，让空中操控感更弱
    public float wallSlideSlowMultiplier = 0.65f; // 未使用
    public float wallSlideSpeed = 2f;          // 壁滑最大下落速度（防止物理碰撞求解把y速度压到0）
    public Vector2 wallJumpForce;              // 蹬墙跳速度
    [Space]
    public float dashDuration = 0.25f;         // 冲刺持续时间
    public float dashSpeed = 20;               // 冲刺速度

    public Vector2 moveInput { get; private set; }  // 当前帧的移动输入，被 OnEnable 中的 InputSystem 事件驱动

    protected override void Awake()
    {
        base.Awake();
        input = new PlayerInputSet();

        // 初始化所有状态，animBoolName 必须与 Animator Controller 中的参数名一致
        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpfall");
        fallState = new Player_FallState(this, stateMachine, "jumpfall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "jumpfall");
        dashState = new Player_DashState(this, stateMachine, "dash");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new Player_JumpAttackState(this, stateMachine, "jumpAttack");
        deathState = new Player_DeathState(this, stateMachine, "death");
        // counterAttackState 使用 "counterAttack1" 作为入口，counterAttack2 通过 Animator transition 触发
        counterAttackState = new Player_AttackCounterState(this, stateMachine, "counterAttack1");
    }

    protected override void Start()
    {
        base.Start();
        // 初始状态：待机
        stateMachine.Initialize(idleState);
    }

    // 覆写 Entity.Death，触发死亡事件并切到死亡状态
    public override void EntityDeath()
    {
        base.EntityDeath();
        OnPlayerADeath?.Invoke();
        stateMachine.ChangeState(deathState);
    }

    private void OnEnable()
    {
        input.Enable();

        // InputSystem 事件驱动 moveInput，而不是每帧轮询
        // 这样 moveInput 在 performed 和 canceled 之外的时间保持稳定
        input.Player.Movement.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };
        input.Player.Movement.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };
    }

    private void OnDisable()
    {
        input.Disable();
        // 必须解注册，否则重复 OnEnable/OnDisable 会堆叠事件订阅
        input.Player.Movement.performed -= ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };
        input.Player.Movement.canceled -= ctx =>
        {
            moveInput = Vector2.zero;
        };
    }

    // ---------- 延迟进入攻击状态（协程辅助） ----------
    // 连击排队需要等待当前攻击动画退出后再切换，因此延迟一帧
    public void EnterAttackWithDelay()
    {
        if (queueAttackCo != null)
        {
            StopCoroutine(queueAttackCo);
        }
        queueAttackCo = StartCoroutine(EnterAttackWithDelayCo());
    }

    private IEnumerator EnterAttackWithDelayCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(basicAttackState);
    }
}