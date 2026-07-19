using System;
using UnityEngine;

public class Enemy : Entity
{
    // ---------- 状态实例 ----------
    public Enemy_IdleState idleState;
    public Enemy_MoveState moveState;
    public Enemy_AttackState attackState;
    public Enemy_BattleState battleState;
    public Enemy_DeathState deathState;
    public Enemy_StunnedState stunnedState;

    [Header("战斗设置")]
    public float battleMoveSpeed = 2.8f;      // 战斗时追踪玩家的移动速度（比巡逻快）
    public float attackDistance = 2;           // 攻击距离阈值
    public float battleTimeDuration = 5;       // 脱离战斗后多久切回巡逻
    public float minRetreatDistance = 1;        // 距离玩家过近时后撤的最小距离
    public Vector2 retreatValocity;             // 后撤速度

    [Header("反击设置")]
    public float stunneduration = 1;           // 被反击后的眩晕时长
    public Vector2 stynnedVelocity = new Vector2(7, 7); // 被反击击飞速度
    protected bool canBeCounned;               // 当前是否处于可被反击的窗口

    [Header("移动设置")]
    public float idleTime = 2;                 // 待机持续时间
    public float moveSpeed = 1.4f;             // 巡逻速度
    [Range(0, 2)]
    public float moveAnimSpeedMultiplier = 1;   // 移动动画播放速度倍率

    [Header("玩家检测")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform playerCheck;          // 玩家检测点
    [SerializeField] private float playerCheckDistance = 10;  // 玩家检测距离
    public Transform player { get; private set; }            // 当前追踪的玩家

    /// <summary>
    /// 开启/关闭反击窗口。由 Enemy_AnimationTriggers 在动画事件中调用。
    /// 攻击动画的特定帧开启窗口，其他帧关闭。
    /// </summary>
    public void EnableCounterWindow(bool enable) => canBeCounned = enable;

    public override void EntityDeath()
    {
        base.EntityDeath();
        stateMachine.ChangeState(deathState);
    }

    /// <summary>
    /// 当玩家死亡时触发的回调：敌人停止战斗行为，回到待机巡逻。
    /// 通过 Player.OnPlayerADeath 事件订阅，不由状态机直接管理。
    /// </summary>
    private void HandlePlayerDeath()
    {
        player = null;
        stateMachine.ChangeState(idleState);
    }

    /// <summary>
    /// 尝试进入战斗状态。
    /// 如果当前已经在 battle 或 attack 状态则不切换——避免重复调用导致状态刷新。
    /// </summary>
    public void TryToBattle(Transform player)
    {
        if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
        {
            return;
        }
        this.player = player;
        stateMachine.ChangeState(battleState);
    }

    /// <summary>
    /// 获取当前追踪的玩家 Transform。
    /// 第一次调用时初始化引用，避免在 Awake 中强依赖 Player。
    /// </summary>
    public Transform GetPlayerTransform()
    {
        if (player == null)
        {
            player = PlayerDetection().transform;
        }
        return player;
    }

    /// <summary>
    /// 朝 facingDir 方向发射玩家检测射线。
    /// 返回 hit 结构体，调用方可以判断是否检测到玩家以及距离。
    /// 射线同时检测 playerLayer 和 groundLayer——避免墙体遮挡时穿越墙体检测到玩家。
    /// </summary>
    public RaycastHit2D PlayerDetection()
    {
        RaycastHit2D hit = Physics2D.Raycast(playerCheck.position, Vector2.right * facingDir, playerCheckDistance, playerLayer | groundLayer);
        // 如果第一个碰撞体不是 Player（比如先碰到墙），视为未检测到
        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return default;
        }
        return hit;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + facingDir * playerCheckDistance, playerCheck.position.y));
    }

    // 订阅玩家死亡事件，避免敌人对着尸体继续战斗
    private void OnEnable()
    {
        Player.OnPlayerADeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        Player.OnPlayerADeath -= HandlePlayerDeath;
    }
}