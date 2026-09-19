using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 实体生命值组件：管理血量、受伤击退、死亡判定。
/// 
/// 为什么处理击退逻辑放在这里而不是单独的 KnockBack 组件：
/// 击退的方向和力度跟伤害量强相关（重击击退更强），放在一起避免传参耦合。
/// </summary>
public class Entity_Health : MonoBehaviour, IDamagable
{
    public event Action OnTakingDamage;
    public event Action OnHealthUpdate;

    private Slider healthBar;
    private Entity entity;
    private Entity_VFX entityVFX;
    private Entity_Stats entityStats;
    private Entity_DropManager dropManager;

    [Header("生命设置")]
    [SerializeField] protected float currentHealth;
    public bool isDead { get; private set; }
    protected bool canTakeDamage = true;

    [Header("生命值再生设置")]
    [SerializeField] private float regenInterval = 1;
    [SerializeField] private bool canRegenerateHealth = true;
    public float lastDamageTaken { get; private set; }

    [Header("伤害击退")]
    [SerializeField] private Vector2 knockBackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 heavyKnockBackPower = new Vector2(7, 7);
    [SerializeField] private float knockBackDuration = 0.2f;
    [SerializeField] private float heavyKnockBackDuration = 0.5f;

    [Header("重击伤害")]
    [SerializeField] private float heavyDamageThreshold = 0.3f; // 单次损失血量超过 maxHp 的 30% 即为重击

    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        entityVFX = GetComponent<Entity_VFX>();
        entityStats = GetComponent<Entity_Stats>();
        healthBar = GetComponentInChildren<Slider>();
        dropManager = GetComponent<Entity_DropManager>();

        SetupHealth();
    }

    private void SetupHealth()
    {
        if (entityStats != null)
        {
            currentHealth = entityStats.GetMaxHealth();
            OnHealthUpdate += UpdateHealthBar;

            UpdateHealthBar();
            InvokeRepeating(nameof(RegenerateHealth), 0, regenInterval);
        }
    }

    /// <summary>
    /// 受伤入口：
    /// 1. 死亡后不再受伤（防止死亡动画播放期间持续触发）
    /// 2. 根据伤害来源方向计算击退方向
    /// 3. 重击/轻击使用不同的击退力和持续时间
    /// 4. 触发受击视觉特效
    /// 5. 扣除血量，归零时触发死亡
    /// </summary>
    public virtual bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isDead || !canTakeDamage)
        {
            return false;
        }
        if (AttackEvaded())
        {
            return false;
        }

        Entity_Stats attackerAtats = damageDealer.GetComponent<Entity_Stats>();
        float armorReduction = attackerAtats != null ? attackerAtats.GetArmorReduction() : 0;

        float mitigation = entityStats != null ? entityStats.GetArmorMitigation(armorReduction) : 0;
        float finalPhysicalDamage = damage * (1 - mitigation);

        float resistance = entityStats != null ? entityStats.GetElementalResistance(element) : 0;
        float finalElementalDamage = elementalDamage * (1 - resistance);

        TakeKnockBack(damageDealer, finalPhysicalDamage);
        ReduceHealth(finalPhysicalDamage + finalElementalDamage);

        lastDamageTaken = finalPhysicalDamage + finalElementalDamage;

        OnTakingDamage?.Invoke();
        return true;
    }

    private void TakeKnockBack(Transform damageDealer, float finalDamage)
    {
        Vector2 knockBack = CalculateKnockBack(finalDamage, damageDealer);
        float duration = CalculateDuration(finalDamage);
        entity?.ReceiveKnockBack(knockBack, duration);
    }

    public void SetCanTakedamage(bool canTakeDamage) => this.canTakeDamage = canTakeDamage;

    private bool AttackEvaded()
    {
        if (entityStats == null)
        {
            return false;
        }
        else
        {
            return UnityEngine.Random.Range(0, 100) < entityStats.GetEvasion();
        }
    }

    private void RegenerateHealth()
    {
        if (!canRegenerateHealth || entityStats == null) return;

        float regenAmount = entityStats.resources.healthRegen.GetValue();
        IncreaseHealth(regenAmount);
    }

    public void IncreaseHealth(float healAmount)
    {
        if (isDead) return;

        float newHealth = currentHealth + healAmount;
        float maxHealth = entityStats.GetMaxHealth();
        currentHealth = Mathf.Min(newHealth, maxHealth);

        OnHealthUpdate?.Invoke();
    }

    public void ReduceHealth(float damage)
    {
        currentHealth -= damage;
        entityVFX?.PlayOnDamageVFX();
        OnHealthUpdate?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        // 通知 Entity 做死亡逻辑（状态切换等），而不是在这里直接操作状态机
        // 这样 Entity 的派生类（Player/Enemy）可以各自定制死亡行为
        entity?.EntityDeath();
        dropManager?.DropItems();
    }

    public float GetHealthPercentage() => currentHealth / entityStats.GetMaxHealth();

    public void SetHealthPercentage(float percent)
    {
        float maxHealth = entityStats.GetMaxHealth();
        currentHealth = Mathf.Clamp(percent * maxHealth, 0, maxHealth);

        OnHealthUpdate?.Invoke();
    }

    public float GetCurrentHealth() => currentHealth;

    private void UpdateHealthBar()
    {
        if (healthBar == null || entityStats == null)
        {
            return;
        }
        healthBar.value = currentHealth / entityStats.GetMaxHealth();
    }

    /// <summary>
    /// 计算击退方向：永远让目标朝远离伤害来源的方向飞出去。
    /// 重击使用更大的力和更长的持续时间。
    /// </summary>
    private Vector2 CalculateKnockBack(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;
        Vector2 knockBack = IsHeavyDamage(damage) ? heavyKnockBackPower : knockBackPower;
        knockBack.x = knockBack.x * direction;
        return knockBack;
    }

    private float CalculateDuration(float damage) => IsHeavyDamage(damage) ? heavyKnockBackDuration : knockBackDuration;

    private bool IsHeavyDamage(float damage)
    {
        if (entityStats == null)
        {
            return false;
        }
        else
        {
            return damage / entityStats.GetMaxHealth() > heavyDamageThreshold;
        }
    }
}
