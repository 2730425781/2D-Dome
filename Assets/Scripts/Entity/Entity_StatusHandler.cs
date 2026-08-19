using System.Collections;
using UnityEngine;

/// <summary>
/// 实体状态效果处理器。管理元素状态（如冰冻）的施加、持续和结束。
/// 
/// 为什么独立成组件而不是塞在 Entity 里：
/// 1. 状态效果是按需施加的，不是所有实体都需要
/// 2. 使用 GetComponent 按需获取，不占用所有实体的 Update 时间
/// 3. 当前效果排他设计（同一时间只能有一种元素效果）
/// </summary>
public class Entity_StatusHandler : MonoBehaviour
{
    private Entity entity;
    private Entity_VFX entityVFX;
    private Entity_Stats entityStats;
    private Entity_Health entityHealth;
    private ElementType currentEffect = ElementType.None;
    [Header("雷电效果设置")]
    [SerializeField] private GameObject lightningStrikeVFX;
    [SerializeField] private float currentCharge;
    [SerializeField] private float maximumCharge = 1;
    private Coroutine shockCo;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        entityVFX = GetComponent<Entity_VFX>();
        entityStats = GetComponent<Entity_Stats>();
        entityHealth = GetComponent<Entity_Health>();
    }

    public void RemoveAllNegetiveEffect()
    {
        StopAllCoroutines();
        currentEffect = ElementType.None;
        entityVFX.StopAllVFX();
    }

    public void ApplyStatusEffect(ElementType element, ElementalEffectDate effectDate)
    {
        switch (element)
        {
            case ElementType.Ice when CanBeApplied(ElementType.Ice):
                ApplyChillEffect(effectDate.chillDuration, effectDate.chillSlowMultiplier);
                break;
            case ElementType.Fire when CanBeApplied(ElementType.Fire):
                ApplyBurnEffect(effectDate.burnDuration, effectDate.totalBurnDamage);
                break;
            case ElementType.Lightning when CanBeApplied(ElementType.Lightning):
                ApplyShockEffect(effectDate.shockDuration, effectDate.shockDamage, effectDate.shockCharge);
                break;
            default:
                break;
        }
    }

    private void ApplyShockEffect(float duration, float damage, float charge)
    {
        float lightningResistance = entityStats.GetElementalResistance(ElementType.Lightning);
        float finalCharge = charge * (1 - lightningResistance);
        currentCharge += finalCharge;

        if (currentCharge >= maximumCharge)
        {
            DoLightningStrike(damage);
            StopShockEffect();
            return;
        }
        if (shockCo != null)
        {
            StopCoroutine(shockCo);
        }
        shockCo = StartCoroutine(ShockEffectCo(duration));
    }

    private void StopShockEffect()
    {
        currentEffect = ElementType.None;
        currentCharge = 0;
        entityVFX.StopAllVFX();
    }

    private void DoLightningStrike(float damage)
    {
        Instantiate(lightningStrikeVFX, transform.position, Quaternion.identity);
        entityHealth.ReduceHealth(damage);
    }

    private IEnumerator ShockEffectCo(float duration)
    {
        currentEffect = ElementType.Lightning;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Lightning);

        yield return new WaitForSeconds(duration);
        StopShockEffect();
    }

    private void ApplyBurnEffect(float duration, float fireDamage)
    {
        float fireResistance = entityStats.GetElementalResistance(ElementType.Fire);
        float finalDamage = fireDamage * (1 - fireResistance);
        StartCoroutine(BurnEffectCO(duration, finalDamage));
    }

    private IEnumerator BurnEffectCO(float duration, float totalDamage)
    {
        currentEffect = ElementType.Fire;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Fire);

        int tickersPerSecond = 2;
        int tickCount = Mathf.RoundToInt(tickersPerSecond * duration);
        float damagePerTick = totalDamage / tickCount;
        float tickInterval = 1f / tickersPerSecond;

        for (int i = 0; i < tickCount; i++)
        {
            entityHealth.ReduceHealth(damagePerTick);
            yield return new WaitForSeconds(tickInterval);
        }
        currentEffect = ElementType.None;
    }

    /// <summary>
    /// 施加冰冻效果。持续时间受目标的冰冻抗性影响。
    /// 抗性越高，持续时间越短。
    /// </summary>
    private void ApplyChillEffect(float duration, float slowMultiplier)
    {
        float iceResistance = entityStats.GetElementalResistance(ElementType.Ice);
        float finalDuration = duration * (1 - iceResistance);
        StartCoroutine(ChillEffectCo(finalDuration, slowMultiplier));
    }

    private IEnumerator ChillEffectCo(float duration, float slowMultiplier)
    {
        entity.SlowDownEntity(duration, slowMultiplier);
        currentEffect = ElementType.Ice;
        entityVFX.PlayOnStatusVFX(duration, currentEffect);

        yield return new WaitForSeconds(duration);

        currentEffect = ElementType.None;
    }

    /// <summary>
    /// 当前是否可以施加指定元素效果。
    /// 排他设计：同一时间只能有一种元素效果。
    /// </summary>
    public bool CanBeApplied(ElementType element)
    {
        if (element == ElementType.Lightning && currentEffect == ElementType.Lightning)
        {
            return true;
        }
        return currentEffect == ElementType.None;
    }
}
