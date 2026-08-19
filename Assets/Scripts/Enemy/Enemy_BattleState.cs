using UnityEngine;

/// <summary>
/// 敌人战斗状态：追踪玩家，进入攻击范围时切到攻击状态。
/// 
/// 设计要点：
/// 1. 进入时如果距离玩家过近会先后撤一步（ShouldRetreat），
///    避免敌人贴脸时不断原地抽搐。
/// 2. 有战斗超时机制（battleTimeDuration）：脱离玩家视线或玩家跑远后，
///    经过一段时间切回巡逻状态，而不是无限战斗。
/// </summary>
public class Enemy_BattleState : EnemyState
{
    private Transform player;
    private Transform lastTarget;
    private float lastTimeInBattle;  // 上次见到玩家的时间戳

    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        UpdateBattleTimer();
        if (player == null)
        {
            player = enemy.GetPlayerTransform();

            // 射线检测不到玩家（例如玩家在视线盲区）时，
            // 退回 Enemy_Health 的圆形范围检测，避免 player 为 null 导致敌人原地卡住
            if (player == null)
            {
                Enemy_Health health = enemy.GetComponent<Enemy_Health>();
                if (health != null)
                {
                    player = enemy.FindPlayerInRadius();
                }
            }
        }

        // 进入战斗时如果距离玩家太近，先向后跳开一段距离
        if (ShouldRetreat())
        {
            rb.linearVelocity =
                new Vector2(enemy.retreatVelocity.x * enemy.activeSlowMultiplier * -DirectionToPlayer(), enemy.retreatVelocity.y);

            enemy.HandleFlip(DirectionToPlayer());
        }
    }

    public override void Update()
    {
        base.Update();

        // 当前帧如果看到了玩家，刷新战斗计时器
        if (enemy.PlayerDetection())
        {
            UpdateTarget();
            UpdateBattleTimer();
        }

        // 战斗超时（太久没见过玩家）→ 切回巡逻
        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
        }

        // 进入攻击范围且能看到玩家 → 攻击
        if (WithinAttackRange() && enemy.PlayerDetection())
        {
            stateMachine.ChangeState(enemy.attackState);
        }
        else
        {
            // 还没到攻击范围 → 朝玩家方向移动
            enemy.SetVelocity(enemy.GetBattleSpeed() * DirectionToPlayer(), rb.linearVelocity.y);
        }
    }

    private void UpdateTarget()
    {
        if (!enemy.PlayerDetection()) return;

        Transform newTarget = enemy.PlayerDetection().transform;

        if (newTarget != lastTarget)
        {
            lastTarget = newTarget;
            player = newTarget;
        }
    }

    private void UpdateBattleTimer() => lastTimeInBattle = Time.time;
    private bool BattleTimeIsOver() => Time.time > lastTimeInBattle + enemy.battleTimeDuration;
    private bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;
    private bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;

    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        else
        {
            return Mathf.Abs(player.position.x - enemy.transform.position.x);
        }
    }

    private int DirectionToPlayer()
    {
        if (player == null)
            return 0;

        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }
}