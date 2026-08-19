using UnityEngine;

/// <summary>
/// 投掷剑实体基类（普通/穿刺/旋转/反弹剑共用的飞行与回收逻辑）。
/// 
/// 为什么把剑做成基类：
/// 四种剑的差别只在"命中后做什么"，发射、朝向、回收、超时返回都相同，
/// 抽到基类避免每个子类复制一遍。
/// </summary>
public class SkillObject_Sword : SkillObject_Base
{
    protected Skill_SwordThrow swordManager;

    protected Transform playerTransform;
    protected bool shouldComeback;
    protected float comebackSpeed = 20;
    protected float maxAllowedDistance = 30;

    protected virtual void Update()
    {
        // 让剑的 sprite 朝向速度方向
        transform.right = rb.linearVelocity;
        HanldeComeback();
    }

    public virtual void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        // 给初速度：剑依靠物理向前飞，直到命中目标或被回收
        rb.linearVelocity = direction;

        this.swordManager = swordManager;
        playerTransform = swordManager.transform.root;

        playerStats = swordManager.player.stats;
        damageScaleDate = swordManager.damageScaleDate;
    }

    public void GetSwordBackToPlayer() => shouldComeback = true;

    protected void HanldeComeback()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (!shouldComeback) return;

        if (distance > maxAllowedDistance)
        {
            GetSwordBackToPlayer();
        }

        // 手动向玩家移动：回收阶段不再使用物理，避免碰撞干扰
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, comebackSpeed * Time.deltaTime);

        // 回到玩家身边后销毁剑
        if (distance < 0.5f)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        // 普通剑：第一次命中就停下并结算一次伤害
        StopSword(collider);
        DamageEnemiesInRadius(transform, 0.5f, true);
    }

    protected void StopSword(Collider2D collider)
    {
        // 关闭物理模拟：命中后剑不再受物理影响，由子类控制后续行为
        // （反弹剑会飞向下一个目标，旋转剑会原地旋转）
        rb.simulated = false;
        transform.parent = collider.transform;
    }
}
