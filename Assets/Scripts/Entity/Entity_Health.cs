using UnityEngine;

/// <summary>
/// 实体生命值组件：管理血量、受伤击退、死亡判定。
/// 
/// 为什么处理击退逻辑放在这里而不是单独的 KnockBack 组件：
/// 击退的方向和力度跟伤害量强相关（重击击退更强），放在一起避免传参耦合。
/// </summary>
public class Entity_Health : MonoBehaviour, IDamagable
{
    private Entity_VFX entityVFX;
    private Entity entity;

    [Header("生命值设置")]
    [SerializeField] protected float currentHp;
    [SerializeField] protected float maxHp = 100;
    [SerializeField] protected bool isDead;

    [Header("伤害击退")]
    [SerializeField] private Vector2 knockBackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 heavyKnockBackPower = new Vector2(7, 7);
    [SerializeField] private float knockBackDuration = 0.2f;
    [SerializeField] private float heavyKnockBackDuration = 0.5f;

    [Header("重击伤害")]
    [SerializeField] private float heavyDamageThreshold = 0.3f; // 单次损失血量超过 maxHp 的 30% 即为重击

    protected virtual void Awake()
    {
        entityVFX = GetComponent<Entity_VFX>();
        entity = GetComponent<Entity>();
        currentHp = maxHp;
    }

    /// <summary>
    /// 受伤入口：
    /// 1. 死亡后不再受伤（防止死亡动画播放期间持续触发）
    /// 2. 根据伤害来源方向计算击退方向
    /// 3. 重击/轻击使用不同的击退力和持续时间
    /// 4. 触发受击视觉特效
    /// 5. 扣除血量，归零时触发死亡
    /// </summary>
    public virtual void TakeDamage(float damage, Transform damageDealer)
    {
        if (isDead)
        {
            return;
        }

        Vector2 knockBack = CalulateKnockBack(damage, damageDealer);
        float duration = CalculateDuration(damage);
        entity?.ReciveKnockBack(knockBack, duration);
        entityVFX?.PlayOnDamageVFX();
        ReduceHp(damage);
    }

    protected void ReduceHp(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        // 通知 Entity 做死亡逻辑（状态切换等），而不是在这里直接操作状态机
        // 这样 Entity 的派生类（Player/Enemy）可以各自定制死亡行为
        entity.EntityDeath();
    }

    /// <summary>
    /// 计算击退方向：永远让目标朝远离伤害来源的方向飞出去。
    /// 重击使用更大的力和更长的持续时间。
    /// </summary>
    private Vector2 CalulateKnockBack(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;
        Vector2 knockBack = IsHeavyDamage(damage) ? heavyKnockBackPower : knockBackPower;
        knockBack.x = knockBack.x * direction;
        return knockBack;
    }

    private float CalculateDuration(float damage) => IsHeavyDamage(damage) ? heavyKnockBackDuration : knockBackDuration;

    private bool IsHeavyDamage(float damage) => damage / maxHp > heavyDamageThreshold;
}