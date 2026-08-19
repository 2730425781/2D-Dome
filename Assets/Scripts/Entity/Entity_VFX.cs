using System.Collections;
using UnityEngine;

/// <summary>
/// 实体视觉特效组件：负责受伤闪白等视觉反馈。
/// 
/// 独立成组件而不是分散在各状态中的原因：
/// 1. 受伤闪白是跨状态的通用行为，不应被状态切换打断
/// 2. 协程管理避免闪白中途被重复触发时出现诡异闪烁
/// </summary>
public class Entity_VFX : MonoBehaviour
{
    protected SpriteRenderer sr;
    private Entity entity;

    [Header("伤害效果")]
    [SerializeField] private Material onDamageMaterial;   // 受伤时替换的材质（闪白）
    [SerializeField] private float onDamageVFXDuration = 0.2f;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private Color hitVFXColor = Color.white;
    [SerializeField] private GameObject critHitVFX;
    [Header("元素效果颜色")]
    [SerializeField] private Color chillVFX = Color.cyan;
    [SerializeField] private Color burnVFX = Color.red;
    [SerializeField] private Color shockVFX = Color.yellow;
    private Color originalHitVFXColor;

    private Material originalMaterial;
    private Coroutine onDamageVFXCoroutine;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
        originalHitVFXColor = hitVFXColor;
    }

    public void PlayOnStatusVFX(float duration, ElementType element)
    {
        if (element == ElementType.Ice)
        {
            StartCoroutine(PlayStatusVFXCo(duration, chillVFX));
        }
        if (element == ElementType.Fire)
        {
            StartCoroutine(PlayStatusVFXCo(duration, burnVFX));
        }
        if (element == ElementType.Lightning)
        {
            StartCoroutine(PlayStatusVFXCo(duration, shockVFX));
        }
    }

    public void StopAllVFX()
    {
        StopAllCoroutines();
        sr.color = Color.white;
        sr.material = originalMaterial;
    }

    private IEnumerator PlayStatusVFXCo(float duration, Color effectColor)
    {
        float tickInterval = 0.25f;
        float timer = 0;

        Color lightColor = effectColor * 1.2f;
        Color darkColor = effectColor * 0.8f;

        bool toggle = false;
        while (timer < duration)
        {
            sr.color = toggle ? lightColor : darkColor;
            toggle = !toggle;

            yield return new WaitForSeconds(tickInterval);
            timer = timer + tickInterval;
        }
        sr.color = Color.white;
    }

    public void CreateOnHitVFX(Transform target, bool isCrit, ElementType element)
    {
        GameObject hitPrefab = isCrit ? critHitVFX : hitVFX;
        GameObject vfx = Instantiate(hitPrefab, target.position, Quaternion.identity);

        vfx.GetComponentInChildren<SpriteRenderer>().color = GetElementColor(element);  //根据元素种类来改变打击效果的颜色

        if (entity.facingDir == -1 && isCrit)
        {
            vfx.transform.Rotate(0, 180, 0);
        }
    }

    public Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Ice:
                return chillVFX;
            case ElementType.Fire:
                return burnVFX;
            case ElementType.Lightning:
                return shockVFX;
            default:
                return originalHitVFXColor;
        }
    }

    /// <summary>
    /// 播放受伤闪白。
    /// 每次调用都会重置之前的协程，保证最新一次受伤的闪白总是完整播放。
    /// </summary>
    public void PlayOnDamageVFX()
    {
        if (onDamageVFXCoroutine != null)
        {
            StopCoroutine(onDamageVFXCoroutine);
        }
        StartCoroutine(OnDamageVFXCo());
    }

    private IEnumerator OnDamageVFXCo()
    {
        sr.material = onDamageMaterial;
        yield return new WaitForSeconds(onDamageVFXDuration);
        sr.material = originalMaterial;
    }
}
