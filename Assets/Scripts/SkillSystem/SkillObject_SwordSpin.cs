using UnityEngine;

/// <summary>
/// 旋转剑：飞到最大距离后停在原地旋转，周期性伤害周围敌人，
/// 持续 maxSpinDuration 秒后自动飞回玩家。
/// </summary>
public class SkillObject_SwordSpin : SkillObject_Sword
{
    private int maxDistance;
    private float attacksPerSecond;
    private float attackTimer;

    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);

        animator?.SetTrigger("spin");

        maxDistance = swordManager.maxDistance;
        attacksPerSecond = swordManager.attacksPerSecond;

        // 到时间后强制回收，避免剑一直停在场上
        Invoke(nameof(GetSwordBackToPlayer), swordManager.maxSpinDuration);
    }

    protected override void Update()
    {
        HandleAttack();
        HandleStopping();
        HanldeComeback();
    }

    private void HandleStopping()
    {
        float diatanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 超过最大距离后关闭物理：让剑停在当前位置旋转
        if (diatanceToPlayer > maxDistance && rb.simulated)
        {
            rb.simulated = false;
        }
    }

    private void HandleAttack()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            // 周期性伤害：不限制同一目标只吃一次（旋转剑扫过就造成伤害）
            DamageEnemiesInRadius(transform, 1, false);
            attackTimer = 1 / attacksPerSecond;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        // 碰到任何东西都立刻停下旋转，不改变位置
        rb.simulated = false;
    }
}
