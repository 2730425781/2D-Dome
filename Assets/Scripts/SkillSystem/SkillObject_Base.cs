using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有技能实体（Shard、剑、Time Echo 等）的公共基类。
/// 
/// 为什么要有这个基类：
/// 1. 伤害结算（范围检测 + TakeDamage + 状态效果 + 命中特效）是各种技能实体
///    都需要的通用逻辑，抽出来避免每个技能都复制一份。
/// 2. 技能实体是临时生成的 GameObject，用"扫描范围"而不是物理接触判定伤害，
///    这样技能实体飞行/旋转时不需要依赖碰撞体，行为更可控。
/// </summary>
public class SkillObject_Base : MonoBehaviour
{
    [SerializeField] private GameObject onHitVFX;

    [Space]
    [SerializeField] protected LayerMask layerMask;       // 可被伤害的目标 Layer
    [SerializeField] protected Transform targetCheck;      // 伤害检测的中心点
    [SerializeField] protected float checkRadius = 1f;     // 伤害检测半径

    protected Rigidbody2D rb;
    protected Animator animator;
    protected Entity_Stats playerStats;         // 从技能管理器传入的玩家属性，用于计算伤害
    protected DamageScaleData damageScaleDate;  // 技能升级后附加的伤害缩放配置
    protected ElementType usedElement;          // 最近一次命中使用的元素，供 VFX 等使用
    protected bool targetGoHit;                 // 最近一次命中是否成功打中目标
    protected Transform lastTarget;             // 最近一次命中的目标，供 Time Echo 复制等使用

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // 记录本技能实例已经伤害过的目标。
    // 为什么需要：有些技能（Shard 爆炸、剑命中）同一帧可能同时命中同一个敌人，
    // 用 HashSet 去重可以避免对同一目标重复结算伤害。
    private HashSet<Collider2D> damageTargets = new HashSet<Collider2D>();

    /// <summary>
    /// 以 t 为中心、radius 为半径，扫描所有在 layerMask 上的碰撞体。
    /// 用 OverlapCircleAll 而不是物理接触：技能实体可以高速移动，
    /// 接触检测容易"穿透"目标，范围扫描更稳定。
    /// </summary>
    protected Collider2D[] GetEnemiesAround(Transform t, float radius)
    {
        return Physics2D.OverlapCircleAll(t.position, radius, layerMask);
    }

    /// <summary>
    /// 对范围内敌人结算一次伤害。
    /// damageOnceOnly = true 时同一目标只结算一次（去重生效）；
    /// false 时允许反复伤害（例如旋转剑每帧扫过敌人）。
    /// </summary>
    protected void DamageEnemiesInRadius(Transform t, float radius, bool damageOnceOnly)
    {
        foreach (var target in GetEnemiesAround(t, radius))
        {
            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null)
            {
                continue;
            }

            if (damageTargets.Contains(target) && damageOnceOnly)
            {
                continue;
            }

            // 每次命中都重新算一次伤害：攻击力/元素等可能受 buff 影响
            AttackDate attackDate = playerStats.GetAttackDate(damageScaleDate);
            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

            float physDamage = attackDate.physcalDamage;
            float elemDamage = attackDate.elementDamage;
            ElementType element = attackDate.element;

            targetGoHit = damagable.TakeDamage(physDamage, elemDamage, element, transform);

            if (damageOnceOnly)
            {
                damageTargets.Add(target);
            }

            // 命中后附加元素状态效果（烧伤/冻结/感电等）；没有状态处理器则跳过
            if (element != ElementType.None)
            {
                statusHandler?.ApplyStatusEffect(element, attackDate.effectDate);
            }

            if (targetGoHit)
            {
                lastTarget = target.transform;
                Instantiate(onHitVFX, target.transform.position, Quaternion.identity);
            }

            usedElement = element;
        }
    }

    /// <summary>
    /// 在 targetCheck 周围 10 单位内找最近的敌人。
    /// 为什么固定 10：用于"追踪最近敌人"的技能（Shard 追踪、Time Echo 朝向），
    /// 不需要配置，固定一个合理范围即可。
    /// </summary>
    protected Transform FindClosestTarget()
    {
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var enemy in GetEnemiesAround(targetCheck, 10f))
        {
            float distance = Vector2.Distance(targetCheck.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestTarget = enemy.transform;
                closestDistance = distance;
            }
        }

        return closestTarget;
    }

    protected virtual void OnDrawGizmos()
    {
        if (targetCheck == null)
        {
            targetCheck = transform;
        }

        Gizmos.DrawWireSphere(targetCheck.position, checkRadius);
    }
}
