using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 反弹剑：命中第一个敌人后不会停下，而是继续弹向下一个目标。
/// 
/// 为什么继承 SkillObject_Sword 而不是重写：
/// 基础剑的回收、朝向、SetupSword 等通用逻辑已经实现，这里只需要
/// 覆盖"命中后如何行动"这一部分（弹跳而不是停下）。
/// </summary>
public class SkillObject_SwordBounce : SkillObject_Sword
{
    private float bounceSpeed;
    private int bounceCount;

    private Collider2D[] enemyTargets;
    private Transform nextTarget;
    private List<Transform> selectedBefore = new List<Transform>();

    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        animator.SetTrigger("spin");
        // 先调用基类完成物理发射、伤害数据等通用初始化，
        // 再覆盖为反弹剑专用的参数
        base.SetupSword(swordManager, direction);

        // 弹跳参数不写在剑上，而是从 Skill_SwordThrow 读取：
        // 这样数值只在技能管理器上配置一次，所有反弹剑共享
        bounceSpeed = swordManager.bounceSpeed;
        bounceCount = swordManager.bounceCount;
    }

    protected override void Update()
    {
        // 先处理回收：一旦 shouldComeback = true，回收优先级高于弹跳
        HanldeComeback();
        HandleBounce();
    }

    private void HandleBounce()
    {
        if (nextTarget == null) return;

        // 用 MoveTowards 手动移动而不是依赖 Rigidbody：
        // 命中第一个敌人后 rb.simulated 已被关闭，
        // 继续用物理会让剑再次触发 OnTriggerEnter2D，造成重复伤害
        transform.position = Vector2.MoveTowards(transform.position, nextTarget.position, bounceSpeed * Time.deltaTime);

        // 到达目标附近（0.75f 以内）视为"完成一次弹跳"
        if (Vector2.Distance(transform.position, nextTarget.position) < 0.75f)
        {
            // 到达点结算伤害；半径比触发时略大，覆盖目标周围的敌人
            DamageEnemiesInRadius(transform, 1f, false);
            BounceToNextTarget();

            // 弹跳次数耗尽，或者已经没有可选目标 → 回收剑
            if (bounceCount == 0 || nextTarget == null)
            {
                nextTarget = null;
                GetSwordBackToPlayer();
            }
        }
    }

    private void BounceToNextTarget()
    {
        // 先选下一个目标再扣次数：
        // bounceCount 表示"还能跳几次"，扣到 0 时下一跳已经定好，
        // 由 HandleBounce 统一决定是否真正执行
        nextTarget = GetNextTarget();
        bounceCount--;
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        // 只在第一次命中时扫描一次敌人快照：
        // 之后物理已关闭，EnemiesAround 依赖固定的 enemyTargets，
        // 避免每次弹跳都重新扫描、误把新进入范围的敌人也算进来
        if (enemyTargets == null)
        {
            enemyTargets = GetEnemiesAround(transform, 10);
            rb.simulated = false;
        }

        // 初始命中造成伤害（不消耗弹跳次数）：
        // 这次命中是"抛出命中"，不是"弹跳到达"，所以不该扣 bounceCount
        DamageEnemiesInRadius(transform, 0.5f, true);
        // 把第一个命中的敌人记为已访问，避免第一次选目标就弹回自己
        selectedBefore.Add(collider.transform);

        // 只剩一个敌人或次数为 0 → 不需要弹跳，直接回收
        if (enemyTargets.Length <= 1 || bounceCount == 0)
        {
            GetSwordBackToPlayer();
        }
        else
        {
            nextTarget = GetNextTarget();
            // 目标已经全部死亡时不再弹跳，直接回收
            if (nextTarget == null)
            {
                GetSwordBackToPlayer();
            }
        }
    }

    private Transform GetNextTarget()
    {
        List<Transform> validTarget = GetValidTargets();
        // 没有可用目标（敌人已被全部击杀）时返回 null，避免 Random.Range 空列表崩溃
        if (validTarget.Count == 0) return null;

        int randomIndex = Random.Range(0, validTarget.Count);

        Transform nextTarget = validTarget[randomIndex];
        selectedBefore.Add(nextTarget);

        return nextTarget;
    }

    private List<Transform> GetValidTargets()
    {
        List<Transform> validTargets = new List<Transform>();
        List<Transform> aliveTargets = GetAliveTargets();

        foreach (var enemy in aliveTargets)
        {
            // 跳过已经弹过的敌人，避免同一目标被连续命中
            if (enemy != null && !selectedBefore.Contains(enemy.transform))
            {
                validTargets.Add(enemy.transform);
            }
        }

        if (validTargets.Count > 0)
        {
            return validTargets;
        }
        else
        {
            // 所有存活敌人都被弹过 → 清空历史重新开始，
            // 让剑在剩下的敌人之间继续循环
            // 注意：这里会把刚打过的目标也放回候选池，可能"弹向自己"，
            // 属于已知行为，如果不需要循环可改为直接返回空列表
            selectedBefore.Clear();
            return aliveTargets;
        }
    }

    private List<Transform> GetAliveTargets()
    {
        List<Transform> aliveTargets = new List<Transform>();

        foreach (var enemy in enemyTargets)
        {
            // 敌人被击杀后其碰撞体变为 Unity 假 null：
            // 用重载后的 != 过滤掉，避免继续朝已销毁的目标移动
            if (enemy != null)
            {
                aliveTargets.Add(enemy.transform);
            }
        }

        return aliveTargets;
    }
}
