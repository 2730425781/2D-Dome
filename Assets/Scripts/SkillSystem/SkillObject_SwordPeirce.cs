using UnityEngine;

/// <summary>
/// 穿刺剑：可以穿透 N 个目标继续飞行，穿够数量或碰到地面才停下。
/// </summary>
public class SkillObject_SwordPeirce : SkillObject_Sword
{
    private int amountToPierce;

    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);
        // 从技能管理器读取穿透次数，保证剑本体上不重复配置
        amountToPierce = swordManager.amountToPierces;
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        bool groundHit = collider.gameObject.layer == LayerMask.NameToLayer("Ground");

        // 穿透次数耗尽，或者碰到地面 → 停止
        if (amountToPierce <= 0 || groundHit)
        {
            DamageEnemiesInRadius(transform, 0.5f, false);
            StopSword(collider);
            return;
        }

        // 每次穿过一个敌人就消耗一次穿透次数，但剑继续飞行
        amountToPierce--;
        DamageEnemiesInRadius(transform, 0.5f, false);
    }
}
