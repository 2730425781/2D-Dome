using System;
using UnityEngine;

/// <summary>
/// 实体战斗组件：负责检测攻击范围内的目标并施加伤害。
/// 
/// 设计为独立组件而不是放在 Entity 里：
/// 1. 攻击检测是可选功能（例如宝箱也可以攻击但不需要状态机）
/// 2. 不同实体的攻击逻辑可以扩展（例如 Player_Combat 增加反击检测）
/// 3. 通过 OverlapCircle 检测，不依赖碰撞接触，避免物理穿透时漏判
/// </summary>
// 注意：类名使用 Entity_Combat（标准拼写），
// 与子类 Player_Combat 保持一致。
public class Entity_Combat : MonoBehaviour
{
    public event Action<float> OnDoingPhysicalDamage;
    private Entity_Stats stats;
    private Entity_VFX entity_VFX;

    public DamageScaleData basicAttackScale;

    [Header("目标检测")]
    [SerializeField] private Transform targetCheck;          // 攻击检测点
    [SerializeField] private float targetCheckRadius = 1;     // 检测范围半径
    [SerializeField] private LayerMask targetCheckLayer;      // 可攻击目标所在的 Layer

    private void Awake()
    {
        entity_VFX = GetComponent<Entity_VFX>();
        stats = GetComponent<Entity_Stats>();
    }
    /// <summary>
    /// 由动画事件 AttackTrigger 调用。
    /// 遍历检测圈内所有目标 → 对实现了 IDamagable 的调用 TakeDamage。
    /// 用接口而非特定类型的好处：Chest、Enemy、Player 都可以受伤，不用写分支。
    /// </summary>
    public void PerformAttack()
    {
        foreach (var target in GetDetectedColliders())
        {
            IDamagable damagable = target.GetComponent<IDamagable>();
            if (damagable == null)
            {
                continue;
            }

            AttackDate attackDate = stats.GetAttackDate(basicAttackScale);
            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

            float physicalDamage = attackDate.physcalDamage;
            float elemDamage = attackDate.elementDamage;
            ElementType element = attackDate.element;

            bool targetGotHit = damagable.TakeDamage(physicalDamage, elemDamage, element, transform);
            //Debug.Log("造成了" + physDamage + "点物理伤害  " + elemDamage + "点" + element + "元素伤害");

            if (element != ElementType.None)
            {
                statusHandler?.ApplyStatusEffect(element, attackDate.effectDate);
            }

            if (targetGotHit)
            {
                OnDoingPhysicalDamage?.Invoke(physicalDamage);
                entity_VFX.CreateOnHitVFX(target.transform, attackDate.isCrit, element);
            }
        }
    }

    /// <summary>
    /// 供子类（Player_Combat）扩展使用，例如反击检测也需要遍历检测范围内的目标。
    /// protected 让子类可以访问但不对外暴露。
    /// </summary>
    protected Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, targetCheckLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}
