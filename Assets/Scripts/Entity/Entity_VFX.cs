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
    private SpriteRenderer sr;

    [Header("伤害效果")]
    [SerializeField] private Material onDamageMaterial;   // 受伤时替换的材质（闪白）
    [SerializeField] private float onDamageVFXDuration = 0.2f;

    private Material originalMaterial;
    private Coroutine onDamageVFXCoroutine;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
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