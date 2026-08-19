using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 碎片技能：根据已解锁的升级分支，生成具有不同行为的碎片。
/// 
/// 升级分支之间的行为是叠加的：
/// TryUseSkill 里逐个检查每个升级是否解锁，而不是 if-else，
/// 因为玩家可以同时拥有多个碎片升级（例如既能追踪又能传送）。
/// </summary>
public class Skill_Shard : Skill_Base
{
    private SkillObject_Shard currentShard;
    private Entity_Health playerHealth;
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private float detonateTime = 2f;
    [Header("技能升级")]
    [SerializeField] private float shardSpeed = 7f;
    [SerializeField] private int maxShardCharges = 3;
    private int currentCharges;
    private bool isReCharging;
    [SerializeField] private float shardExistDuration = 10f;
    [SerializeField] private float saveHpPercent;

    protected override void Awake()
    {
        base.Awake();
        currentCharges = maxShardCharges;
        // 用 GetComponentInParent 而不是 GetComponent：
        // 技能脚本挂在玩家子物体上，Entity_Health 在玩家（父级）上
        playerHealth = player.GetComponentInParent<Entity_Health>();
    }

    public override void TryUseSkill()
    {
        if (!CanUseSkill())
        {
            return;
        }
        if (upgradeType == SkillUpgradeType.None)
        {
            Debug.LogWarning("技能未解锁，无法使用");
            return;
        }

        // 每个已解锁的升级分支各自处理一段逻辑，互不排斥
        if (Unlocked(SkillUpgradeType.Shard))
        {
            HandleShard();
        }
        if (Unlocked(SkillUpgradeType.Shard_MoveToEnemy))
        {
            HandleShardMoving();
        }
        if (Unlocked(SkillUpgradeType.Shard_Multicast))
        {
            HandleShardMulticast();
        }
        if (Unlocked(SkillUpgradeType.Shard_Teleport))
        {
            HandleShardTeport();
        }
        if (Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
        {
            HandleShardHpRewind();
        }
    }

    private void HandleShardHpRewind()
    {
        // 第一次释放：记录当前生命百分比，传送分支会在互换位置时恢复这个值
        if (currentShard == null)
        {
            CreateShard();
            saveHpPercent = playerHealth.GetHealthPercentage();
        }
        else
        {
            SwapPlayerAndShard();
            playerHealth.SetHealthPercentage(saveHpPercent);
            SetSkillOnCoolDown();
        }
    }

    private void HandleShardTeport()
    {
        if (currentShard == null)
        {
            CreateShard();
        }
        else
        {
            SwapPlayerAndShard();
            SetSkillOnCoolDown();
        }
    }

    private void SwapPlayerAndShard()
    {
        Vector3 shardPosition = currentShard.transform.position;
        Vector3 playerPosition = player.transform.position;

        // 碎片传送到玩家位置并爆炸，玩家传送到碎片位置
        currentShard.transform.position = playerPosition;
        currentShard.Explode();
        player.TeleportPlayer(shardPosition);
    }

    private void HandleShardMulticast()
    {
        if (currentCharges <= 0) return;

        CreateShard();
        currentShard.MoveToClosestTarget(shardSpeed);
        currentCharges--;

        // 只启动一次充能协程，避免多个协程同时回充
        if (!isReCharging)
        {
            StartCoroutine(ShardRechargeCo());
        }
    }

    private IEnumerator ShardRechargeCo()
    {
        isReCharging = true;
        while (currentCharges < maxShardCharges)
        {
            yield return new WaitForSeconds(cooldown);
            currentCharges++;
        }
        isReCharging = false;
    }

    private void HandleShardMoving()
    {
        CreateShard();
        currentShard.MoveToClosestTarget(shardSpeed);
        SetSkillOnCoolDown();
    }

    private void HandleShard()
    {
        CreateShard();
    }

    public void CreateShard()
    {
        float finalDetonateTime = GetDetonateTime();

        if (shardPrefab == null)
        {
            Debug.LogWarning("Shard 预制体未在 Inspector 中赋值");
            return;
        }

        GameObject shard = Instantiate(shardPrefab, transform.position, Quaternion.identity);
        currentShard = shard.GetComponent<SkillObject_Shard>();

        if (currentShard == null)
        {
            Debug.LogWarning("Shard 预制体缺少 SkillObject_Shard 组件");
            Destroy(shard);
            return;
        }

        currentShard.SetupShard(this);

        // 传送/回血分支需要知道碎片何时爆炸（即传送完成），
        // 订阅事件以便在那时强制进入冷却
        if (Unlocked(SkillUpgradeType.Shard_Teleport) || Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
        {
            currentShard.OnShardExploded += ForceCooldown;
        }
    }

    /// <summary>
    /// 创建不带传送逻辑的普通碎片（冲刺技能等外部调用）。
    /// </summary>
    public void CreateRawShard(Transform target = null, bool shardCanMove = false)
    {
        bool canMove;
        if (shardCanMove)
        {
            canMove = shardCanMove;
        }
        else
        {
            canMove = Unlocked(SkillUpgradeType.Shard_MoveToEnemy) || Unlocked(SkillUpgradeType.Shard_Multicast);
        }
        GameObject shard = Instantiate(shardPrefab, transform.position, Quaternion.identity);
        shard.GetComponent<SkillObject_Shard>().SetupShard(this, detonateTime, canMove, shardSpeed, target);
    }

    public float GetDetonateTime()
    {
        // 传送碎片需要长时间存在，等待玩家再次施法来交换位置，
        // 所以爆炸时间更长
        if (Unlocked(SkillUpgradeType.Shard_Teleport) || Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
        {
            return shardExistDuration;
        }
        return detonateTime;
    }

    /// <summary>
    /// 碎片爆炸后触发冷却（传送/回血分支专用）。
    /// 用事件而不是直接调用的原因：碎片是异步爆炸的，
    /// 事件让技能在碎片真正消失时才知道传送流程结束。
    /// </summary>
    private void ForceCooldown()
    {
        if (!OnCoolDown())
        {
            SetSkillOnCoolDown();
            currentShard.OnShardExploded -= ForceCooldown;
        }
    }
}
