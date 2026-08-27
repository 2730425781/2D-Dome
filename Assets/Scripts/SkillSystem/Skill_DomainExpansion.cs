using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 领域展开技能（挂在玩家身上的能力类，继承 Skill_Base）。
/// 
/// 为什么单独一个技能类：领域有三个互斥的升级分支（减速 / 碎片连发 / 回响连发），
/// 各自有独立的时长、减速比例、施法次数。本类集中管理"按分支取值"，
/// 让领域实体（SkillObject_DomainExpansion）和玩家状态只需调用
/// 无分支差异的公共方法（GetDomainDuration / GetSlowPercent 等），
/// 分支判断不会泄漏到调用方。
/// </summary>
public class Skill_DomainExpansion : Skill_Base
{
    [SerializeField] private GameObject domainPrefab;
    [Header("领域设置")]
    public float maxDomainSize = 10;
    public float expandSpeed = 3;
    [Header("减速领域升级")]
    [SerializeField] private float slowDownPercent = 0.8f;
    [SerializeField] private float slowDownDuration = 5;
    [Header("碎片法术领域升级")]
    [SerializeField] private int shardToCast = 10;
    [SerializeField] private float shardCastingSlowDown = 1;
    [SerializeField] private float shardCastingDuration = 5;
    [Header("回响法术领域升级")]
    [SerializeField] private int echoToCast = 8;
    [SerializeField] private float echoCastingSlowDown = 1;
    [SerializeField] private float echoCastingDuration = 6;
    //[SerializeField] private float healthToRestoreWithEcho = 0.05f;
    // 上面按分支分组的序列化参数只在对应 upgradeType 下生效（见下方按分支取值的
    // GetDomainDuration / GetSlowPercent / GetSpellsToCast），以下是运行时状态：
    private float spellCastTimer;
    private float spellPerSecond;

    // 领域实体通过触发器把进入的敌人登记到这里，技能在领域存续期间从中挑选施法目标
    private List<Enemy> trappedTarget = new List<Enemy>();
    private Transform currentTarget;

    /// <summary>
    /// 生成领域实体并在玩家位置展开（由玩家状态或 PlayerState 的即时分支调用）。
    /// 为什么生成入口放在技能类：实体生成后需要技能自身的配置（大小/时长/分支），
    /// 只有技能类持有这些数据，因此由它负责创建并初始化实体。
    /// </summary>
    public void CreateDomain()
    {
        // 施法速率在生成时算一次：连发分支要求在领域存续期内恰好打完配置的总次数，
        // 之后 DoSpellCasting 用 1 / spellPerSecond 做均匀间隔
        spellPerSecond = GetSpellsToCast() / GetDomainDuration();

        GameObject doamin = Instantiate(domainPrefab, transform.position, Quaternion.identity);
        // 传入 this：领域实体是共享预制体不存配置，运行时数值全部从技能实例读取
        doamin.GetComponent<SkillObject_DomainExpansion>().SetUpDomain(this);
    }

    /// <summary>
    /// 领域存续期间由玩家状态（Player_DomainExpansionState.Update）每帧驱动施法。
    /// 为什么由状态逐帧驱动而不是实体自己循环：施法要持续整个领域生命周期，
    /// 与状态机的进入/退出天然对齐，状态结束施法自然停止。
    /// </summary>
    public void DoSpellCasting()
    {
        spellCastTimer -= Time.deltaTime;

        // 每次施法前重新找目标：领域内敌人可能死亡/离开，重新查找保证打到活目标
        if (currentTarget == null)
        {
            currentTarget = FindTargetDomain();
        }

        if (currentTarget != null && spellCastTimer <= 0)
        {
            CastSpell(currentTarget);
            // 施法后把计时器重置为固定间隔，并清空目标以强制下一次重新选择
            spellCastTimer = 1 / spellPerSecond;
            currentTarget = null;
        }
    }

    /// <summary>
    /// 按当前升级分支生成对应法术（回响或碎片）。两个分支互斥，用两个 if 分别判断。
    /// </summary>
    private void CastSpell(Transform target)
    {
        if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            // 随机左右偏移：回响不精确重叠在目标身上，观感更自然，也避免多个回响堆叠
            Vector3 offset = Random.value < 0.5f ? new Vector2(1, 0) : new Vector2(-1, 0);
            skillManager.timeEcho.CreateTimeEcho(target.position + offset);
        }
        if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            // true = shardCanMove：碎片会飞向目标，连发分支的定位就是持续轰炸当前目标
            skillManager.shard.CreateRawShard(target, true);
        }
    }

    /// <summary>
    /// 从领域内敌人里随机挑一个作为下一次施法目标。
    /// 为什么随机而不是固定目标：连发伤害应分摊到多个敌人，随机分布避免集火单个目标；
    /// 顺带用 RemoveAll 清理已死亡/消失的敌人，防止对失效目标施法。
    /// </summary>
    private Transform FindTargetDomain()
    {
        trappedTarget.RemoveAll(target => target == null || target.health.isDead);

        if (trappedTarget.Count == 0) return null;

        int index = Random.Range(0, trappedTarget.Count);
        return trappedTarget[index].transform;
    }

    /// <summary>
    /// 该技能是否需要"玩家悬浮"演出：只有连发分支需要玩家进入专门的领域状态
    /// （悬浮期间持续施法）；减速分支直接原地展开、玩家可自由移动。
    /// 供 PlayerState.Update 决定是即时施放还是切换到领域状态。
    /// </summary>
    public bool InstantDomain()
    {
        return upgradeType != SkillUpgradeType.Domain_EchoSpam
            && upgradeType != SkillUpgradeType.Domain_ShardSpam;
    }

    /// <summary>
    /// 按当前升级分支返回领域存续时长。
    /// 为什么把"分支→参数"的映射集中在这里：调用方（实体、状态）无需感知分支差异，
    /// 新增分支时只需修改这一处映射。
    /// </summary>
    public float GetDomainDuration()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
        {
            return slowDownDuration;
        }
        else if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            return shardCastingDuration;
        }
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            return echoCastingDuration;
        }
        return 0;
    }

    /// <summary>
    /// 返回当前分支对应的减速比例。
    /// 连发分支也带减速（shardCastingSlowDown / echoCastingSlowDown）：
    /// 让目标更难逃出领域，保证连发法术能持续命中。
    /// </summary>
    public float GetSlowPercent()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
        {
            return slowDownPercent;
        }
        else if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            return shardCastingSlowDown;
        }
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            return echoCastingSlowDown;
        }
        return 0;
    }

    // 连发分支的施法总数：减速分支没有施法行为，返回 0 会让 spellPerSecond 为 0
    private int GetSpellsToCast()
    {
        if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            return shardToCast;
        }
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            return echoToCast;
        }
        return 0;
    }

    /// <summary>
    /// 由领域实体的触发器登记进入的敌人，供施法选目标使用。
    /// </summary>
    public void AddTarget(Enemy target)
    {
        trappedTarget.Add(target);
    }

    /// <summary>
    /// 领域销毁前由实体调用：让所有登记敌人停止减速并清空列表。
    /// 为什么列表由技能类持有而非领域实体：施法选目标与减速清理都发生在这里，
    /// 且实体销毁后技能仍要继续存在，列表必须跟着技能走。
    /// </summary>
    public void ClearTargets()
    {
        foreach (var enemy in trappedTarget)
        {
            enemy.StopSlowDown();
        }
        trappedTarget = new List<Enemy>();
    }
}
