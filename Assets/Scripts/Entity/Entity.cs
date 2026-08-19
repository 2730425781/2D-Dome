using System;
using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public event Action OnFlipped;
    // ---------- 核心组件 ----------
    protected StateMachine stateMachine;
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Entity_Stats stats { get; private set; }

    // ---------- 朝向 ----------
    // 用 facingDir 代替 bool + 方向的组合判断，facingDir 直接用于射线方向和速度方向
    private bool facingRight = true;
    public int facingDir { get; private set; } = 1;  // 1=朝右, -1=朝左

    // ---------- 碰撞检测 ----------
    // 用两个 Transform 做墙体检测而不是一个：防止角色头顶或脚下刚好有缝隙时误判
    [Header("碰撞检测")]
    [SerializeField] private float groundCheckDistance;  // 地面检测射线长度
    [SerializeField] private float wallCheckDistance;    // 墙体检测射线长度
    [SerializeField] private Transform groundCheck;      // 地面检测点（通常在角色脚底）
    [SerializeField] private Transform primaryWallCheck; // 墙体检测点（上），防止头顶悬空误判
    [SerializeField] private Transform secondaryWallCheck; // 墙体检测点（下），可空
    public LayerMask groundLayer;    // 地面/墙体所在的 Layer
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }

    // ---------- 击退 ----------
    // 击退期间禁止 SetVelocity 覆盖速度，确保击退动画不被玩家输入打断
    private bool isKnocked;
    private Coroutine knockBackCo;
    private Coroutine slowDownCo;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Entity_Stats>();

        stateMachine = new StateMachine();
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        // 每帧先更新碰撞检测，再执行当前状态的 Update
        // 顺序重要：如果先执行状态再检测，状态拿到的碰撞数据就落后一帧
        HandleCollisionDetection();
        stateMachine.UpdateActiveState();
    }

    /// <summary>
    /// 把动画事件触发器转发给当前状态
    /// </summary>
    public void CurrentStateAnimationTrigger()
    {
        stateMachine.currentState.AnimationTrigger();
    }

    public virtual void EntityDeath()
    {
        // 由子类覆写，例如 Player 切到死亡状态，Enemy 也有自己的死亡流程
    }

    public virtual void SlowDownEntity(float duration, float slowMultiplier, bool canOverrideSlowEffect = false)
    {
        if (slowDownCo != null)
        {
            if (canOverrideSlowEffect)
                StopCoroutine(slowDownCo);
            else
                return;
        }
        slowDownCo = StartCoroutine(SlowDownEntityCo(duration, slowMultiplier));
    }

    protected virtual IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
        yield return null;
    }

    public virtual void StopSlowDown()
    {
        slowDownCo = null;
    }

    /// 接收击退。用协程打断正在进行的击退，保证新击退总是覆盖旧击退。
    /// 击退期间 isKnocked = true，SetVelocity 会被跳过，防止玩家在击退中还能操控移动。
    /// </summary>
    public void ReceiveKnockBack(Vector2 knockBack, float duration)
    {
        if (knockBackCo != null)
        {
            StopCoroutine(knockBackCo);
        }
        knockBackCo = StartCoroutine(KnockBackCo(knockBack, duration));
    }

    private IEnumerator KnockBackCo(Vector2 knockBack, float duration)
    {
        isKnocked = true;
        rb.linearVelocity = knockBack;
        yield return new WaitForSeconds(duration);
        rb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }

    /// <summary>
    /// 设置刚体速度，同时根据 x 方向自动翻转朝向。
    /// 击退期间跳过——击退速度应由 KnockBackCo 控制，玩家输入不应覆盖。
    /// </summary>
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isKnocked)
        {
            return;
        }
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    /// <summary>
    /// 当 x 速度方向与当前朝向相反时翻转。
    /// 抽出为独立方法而不是塞在 SetVelocity 里的原因：有些场景只需要翻转不需要设速度。
    /// </summary>
    public void HandleFlip(float xVelocity)
    {
        if (facingRight && xVelocity < 0)
        {
            Flip();
        }
        else if (!facingRight && xVelocity > 0)
        {
            Flip();
        }
    }

    /// <summary>
    /// 翻转人物（Y 轴旋转 180°），同时更新 facingDir
    /// 为什么用 transform.Rotate 而不是 scale.x *= -1：
    /// 子物体（如 wallCheck 检测点、Sprite）会自动跟随旋转，不需要手动维护它们的朝向
    /// </summary>
    public void Flip()
    {
        transform.Rotate(0.0f, 180.0f, 0.0f);
        facingRight = !facingRight;
        facingDir = facingDir * -1;

        OnFlipped?.Invoke();
    }

    /// <summary>
    /// 每帧检测地面和墙体。
    /// secondaryWallCheck 可以为 null（简化版敌人可能只有一个检测点）
    /// </summary>
    private void HandleCollisionDetection()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        if (secondaryWallCheck != null)
        {
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, groundLayer)
                        && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, groundLayer);
        }
        else
        {
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, groundLayer);
        }
    }

    /// <summary>
    /// 在 Scene 视图中绘制碰撞检测射线（仅开发时可见）
    /// 这样调碰撞参数时不用运行就能看到检测范围
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance, 0));
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
        if (secondaryWallCheck != null)
        {
            Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
        }
    }
}