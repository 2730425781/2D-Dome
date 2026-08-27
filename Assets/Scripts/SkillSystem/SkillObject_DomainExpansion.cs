using UnityEngine;

/// <summary>
/// 领域技能生成的"领域实体"：一个在玩家周围展开、维持、收缩、销毁的圆形区域。
/// 
/// 为什么独立成一个临时物体而不是由 Skill_DomainExpansion 直接控制：
/// 技能类（Skill_Base）挂在玩家身上负责"能力与升级"，而领域是有独立生命周期的
/// 世界物体（自己缩放、自己检测敌人），二者职责分离，符合 SkillObject_Base 的
/// "技能实体"模式。领域通过触发器检测进入/离开的敌人，而不是像其他技能实体那样
/// 做范围伤害扫描——因为减速是需要"进入/离开边界"来管理开始/结束的持续状态。
/// </summary>
public class SkillObject_DomainExpansion : SkillObject_Base
{
    // 领域参数不序列化配置：由 SetUpDomain 在生成时从技能类读取，
    // 因为这些值取决于玩家当前解锁的升级分支（减速/碎片/回响），运行时才确定
    private Skill_DomainExpansion domainManager;
    private float expandSpeed;
    private float duration;
    private float slowDownPercent;
    private Vector3 targetScale;
    private bool isShrinking;

    /// <summary>
    /// 生成后立即由 Skill_DomainExpansion.CreateDomain 调用，把技能侧配置注入领域实例。
    /// 为什么用传参初始化而不是 Awake/Inspector 序列化：领域是共享预制体，
    /// 具体数值（大小/时长/减速比例）只在该技能解锁了某个升级分支后才确定，
    /// 必须在生成时刻由技能类注入。
    /// </summary>
    public void SetUpDomain(Skill_DomainExpansion domainManager)
    {
        this.domainManager = domainManager;
        float maxSize = domainManager.maxDomainSize;
        duration = domainManager.GetDomainDuration();
        slowDownPercent = domainManager.GetSlowPercent();
        expandSpeed = domainManager.expandSpeed;

        targetScale = Vector3.one * maxSize;
        // 用 Invoke 延迟调度收缩：duration 因升级分支而异（减速/碎片/回响时长不同），
        // 只能在拿到具体时长后安排；nameof 保证字符串与方法名保持同步
        Invoke(nameof(ShrinkDomain), duration);
    }

    private void Update()
    {
        // 缩放由每帧驱动：Lerp 依赖 deltaTime 平滑推进，且无需显式停止/清理协程
        HandleScale();
    }

    /// <summary>
    /// 把 localScale 朝 targetScale 插值，实现"展开→维持→收缩"的平滑过渡。
    /// 用 Lerp 而非直接赋值：直接赋值会让领域瞬移到目标大小，缺少扩张的视觉过程。
    /// </summary>
    private void HandleScale()
    {
        float sizeDiffrence = Mathf.Abs(transform.localScale.x - targetScale.x);

        // 0.1f 是"已基本到位"的容差：小于它停止缩放，避免每帧微调造成的抖动；
        // 同时它也作为"收缩完成"的判定阈值（见下方销毁条件）
        bool shouldChangeScale = sizeDiffrence > 0.1f;
        if (shouldChangeScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, expandSpeed * Time.deltaTime);
        }
        // 收缩完成时才销毁：必须先 ClearTargets 让领域内敌人停止减速——
        // Destroy 不会触发 OnTriggerExit，不显式清理减速会永久残留
        if (isShrinking && sizeDiffrence < 0.1f)
        {
            domainManager.ClearTargets();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Invoke 到期的回调：把目标缩放改为 0 并标记收缩中，之后 HandleScale 会把它收掉。
    /// </summary>
    private void ShrinkDomain()
    {
        targetScale = Vector3.zero;
        isShrinking = true;
    }

    /// <summary>
    /// 敌人进入领域：登记到技能类的追踪列表（供维持期间的施法选目标），并施加减速。
    /// 用触发器事件而非范围扫描：减速是持续状态，进入/离开边界天然对应施加/移除。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null) return;

        domainManager.AddTarget(enemy);
        // canOverrideSlowEffect = true：同一敌人重复进入/多个领域叠加时，
        // 新的减速必须覆盖旧的，否则会被 Entity.SlowDownEntity 的"已有减速则忽略"挡掉
        enemy.SlowDownEntity(duration, slowDownPercent, true);
    }

    /// <summary>
    /// 敌人离开领域立即移除减速：减速只在领域范围内生效，离开后不应再受状态影响。
    /// </summary>
    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.StopSlowDown();
    }
}
