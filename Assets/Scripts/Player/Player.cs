using System;
using System.Collections;
using UnityEngine;


/// </summary>
/// 为什么在 OnEnable/OnDisable 中注册/注销 InputSystem 事件，而非 Awake：
/// 1. OnEnable 确保每次 GameObject 激活时输入系统正常运行
///    （Unity 禁用对象后 Input 也停止工作）
/// 2. 在 OnDisable 中注销事件避免重复订阅：
///    如果不注销，每次 OnEnable 都会叠加一个新订阅，导致 moveInput 行为异常
/// 3. 用 lambda 而不是成员方法注册是为了和 InputSystem 的 InputAction 签名匹配，
///    用 -= 注销时必须传入同一个 lambda 实例——这要求 lambda 在字段级保持引用
/// 4. 使用 performed/canceled 事件而非每帧轮询：
///    InputSystem 的事件驱动模型比每帧 ReadValue 更高效，
///    moveInput 在没有输入变化的帧之间保持上次的值不变
/// </summary>
[RequireComponent(typeof(Player_Combat))]
public class Player : Entity
{
    public UI ui { get; private set; }
    // 玩家死亡时通知所有订阅者（比如敌人停止战斗行为）
    public static event Action OnPlayerDeath;
    public PlayerInputSet input { get; private set; }
    public Player_SkillManager skillManager { get; private set; }
    public Player_VFX vfx { get; private set; }
    public Entity_Health health { get; private set; }
    public Entity_StatusHandler statusHandler { get; private set; }
    public Player_Combat combat { get; private set; }

    #region 玩家状态变量

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
    public Player_CounterAttackState counterAttackState { get; private set; }
    public Player_SwordThrowState swordThrowState { get; private set; }
    public Player_DomainExpansionState domainExpansionState { get; private set; }
    #endregion

    [Header("攻击设置")]
    public Vector2[] attackVelocity;           // 基础攻击各段的前冲速度（x影响位移，y影响高度）
    public Vector2 jumpAttackVelocity;         // 跳劈时向下斜冲的速度
    public float attackVelocityDuration = 0.1f; // 攻击速度只维持短暂瞬间，避免角色一直向前滑
    public float comboResetTime = 0.3f;        // 连击间隔超过此值则重置回第一段
    private Coroutine queueAttackCo;

    [Header("领域技能设置")]
    public float riseSpeed = 25;
    public float maxRiseDistance = 3;

    [Header("移动设置")]
    public float moveSpeed;                    // 地面移动速度
    public float jumpForce = 5;                // 跳跃力
    public float inAirMoveMultiplier = 0.65f;  // 空中移动倍率<1，让空中操控感更弱
    public float wallSlideSlowMultiplier = 0.65f; // 未使用
    public float wallSlideSpeed = 2f;          // 壁滑最大下落速度（防止物理碰撞求解把y速度压到0）
    public Vector2 wallJumpForce;              // 蹬墙跳速度
    [Space]
    public float dashDuration = 0.25f;         // 冲刺持续时间
    public float dashSpeed = 20;               // 冲刺速度

    public Vector2 moveInput { get; private set; }  // 当前帧的移动输入，被 OnEnable 中的 InputSystem 事件驱动
    public Vector2 mousePosition { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        input = new PlayerInputSet();

        ui = FindAnyObjectByType<UI>();
        vfx = GetComponent<Player_VFX>();
        health = GetComponent<Entity_Health>();
        combat = GetComponent<Player_Combat>();
        skillManager = GetComponent<Player_SkillManager>();
        statusHandler = GetComponent<Entity_StatusHandler>();

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
        counterAttackState = new Player_CounterAttackState(this, stateMachine, "counterAttack1");
        swordThrowState = new Player_SwordThrowState(this, stateMachine, "swordThrow");
        domainExpansionState = new Player_DomainExpansionState(this, stateMachine, "jumpfall");

    }

    protected override void Start()
    {
        base.Start();
        // 初始状态：待机
        stateMachine.Initialize(idleState);
    }

    public void TeleportPlayer(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
        float originalMoveSpeed = moveSpeed;
        float originalJumpForce = jumpForce;
        float originalAnimSpeed = animator.speed;
        Vector2 originalWallJump = wallJumpForce;
        Vector2 originalJumpAttack = jumpAttackVelocity;
        Vector2[] originalAttackVelocity = new Vector2[attackVelocity.Length];
        Array.Copy(attackVelocity, originalAttackVelocity, attackVelocity.Length);

        float speedMultiplier = 1 - slowMultiplier;

        moveSpeed *= speedMultiplier;
        jumpForce *= speedMultiplier;
        animator.speed *= speedMultiplier;
        wallJumpForce *= speedMultiplier;
        jumpAttackVelocity *= speedMultiplier;
        for (int i = 0; i < attackVelocity.Length; i++)
        {
            attackVelocity[i] *= speedMultiplier;
        }

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed;
        jumpForce = originalJumpForce;
        animator.speed = originalAnimSpeed;
        wallJumpForce = originalWallJump;
        jumpAttackVelocity = originalJumpAttack;
        for (int i = 0; i < attackVelocity.Length; i++)
        {
            attackVelocity[i] = originalAttackVelocity[i];
        }
    }


    // 覆写 Entity.Death，触发死亡事件并切到死亡状态
    public override void EntityDeath()
    {
        base.EntityDeath();
        OnPlayerDeath?.Invoke();
        stateMachine.ChangeState(deathState);
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Mouse.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();

        // InputSystem 事件驱动 moveInput，而不是每帧轮询
        // 这样 moveInput 在 performed 和 canceled 之外的时间保持稳定
        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Spell.performed += ctx => skillManager.shard.TryUseSkill();
        input.Player.Spell.performed += ctx => skillManager.timeEcho.TryUseSkill();

        input.Player.Interact.performed += ctx => TryInteract();

        input.Player.ToggleSkillTreeUI.performed += ctx => ui.ToggleSkillTreeUI();
        input.Player.ToggleInventoryUI.performed += ctx => ui.ToggleInvemtoryUI();
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

    private void TryInteract()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        Collider2D[] objectsAround = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        foreach (var target in objectsAround)
        {
            IInteractable interactable = target.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target.transform;
            }
        }
        if (closest == null) return;

        closest.GetComponent<IInteractable>().Interact();
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
