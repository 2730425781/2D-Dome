using UnityEngine;

/// <summary>
/// 宝箱：实现 IDamagable，受击后播放开启动画、添加物理效果。
/// 
/// 实现 IDamagable 而不是做一个单独的"交互"接口：
/// 攻击键的检测逻辑（Entity_Combat）已经统一使用 IDamagable，
/// 宝箱复用这个接口意味着不用为"可交互物体"再写一套检测系统。
/// 
/// 击退和旋转（knockBack + angularVelocity）是为了让宝箱开启时有一点物理反馈，
/// 视觉上更有打击感，不是功能必需的。
/// </summary>
public class Object_Chest : MonoBehaviour, IDamagable
{
    private Rigidbody2D rb => GetComponentInChildren<Rigidbody2D>();
    private Animator animator => GetComponentInChildren<Animator>();
    private Entity_VFX vfx => GetComponent<Entity_VFX>();

    [Header("开启效果")]
    [SerializeField] private Vector2 knockBack;

    public bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        vfx.PlayOnDamageVFX();            // 受击闪白
        animator.SetBool("openChest", true); // 触发开启动画
        rb.linearVelocity = knockBack;      // 被击退（视觉反馈）
        rb.angularVelocity = Random.Range(-100, 100); // 随机旋转，显得更自然

        return true;
    }
}
