using UnityEngine;

/// <summary>
/// 碎片实体：由 Skill_Shard 生成，负责移动、爆炸和伤害结算。
/// </summary>
public class SkillObject_Shard : SkillObject_Base
{
    public event System.Action OnShardExploded;
    private Skill_Shard shardManager;
    [SerializeField] private GameObject vfxPrefab;

    private Transform target;
    private float speed;

    private void Update()
    {
        if (target == null) return;

        // 用 MoveTowards 手动追踪目标而不是物理：
        // 碎片是"发射后不管"的投射物，手动移动不受物理碰撞干扰
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    public void MoveToClosestTarget(float speed, Transform newTarget = null)
    {
        target = newTarget == null ? FindClosestTarget() : newTarget;
        this.speed = speed;
    }

    /// <summary>
    /// 标准碎片初始化：读取技能管理器配置并设置定时爆炸。
    /// </summary>
    public void SetupShard(Skill_Shard shardManager)
    {
        this.shardManager = shardManager;
        playerStats = shardManager.player.stats;
        damageScaleDate = shardManager.damageScaleDate;

        float detonationTime = shardManager.GetDetonateTime();
        Invoke(nameof(Explode), detonationTime);
    }

    /// <summary>
    /// 带追踪选项的初始化（供冲刺技能等外部调用）。
    /// </summary>
    public void SetupShard(Skill_Shard shardManager, float detonationTime, bool canMove, float shardSpeed, Transform target = null)
    {
        this.shardManager = shardManager;
        playerStats = shardManager.player.stats;
        damageScaleDate = shardManager.damageScaleDate;

        Invoke(nameof(Explode), detonationTime);
        if (canMove)
        {
            MoveToClosestTarget(shardSpeed, target);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() == null) return;

        // 碰到敌人立即爆炸，不需要等定时器
        Explode();
    }

    public void Explode()
    {
        DamageEnemiesInRadius(transform, checkRadius, true);
        GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
        // 特效颜色跟随最近一次命中的元素，让玩家能直观看到属性伤害
        vfx.GetComponentInChildren<SpriteRenderer>().color = shardManager.player.vfx.GetElementColor(usedElement);

        OnShardExploded?.Invoke();
        Destroy(gameObject);
    }
}
