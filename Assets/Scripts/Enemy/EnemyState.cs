using UnityEngine;

/// <summary>
/// 敌人状态基类。
/// 负责同步敌人相关的 Animator 参数：xVelocity（朝向）、battleAnimSpeedMultiplier（战斗动画速度）、moveAnimSpeedMultiplier（巡逻动画速度）。
/// 这两个速度倍率让敌人巡逻和战斗时的动画播放速度不同，视觉上区分行为模式。
/// </summary>
public class EnemyState : EntityState
{
    protected Enemy enemy;
    public EnemyState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.enemy = enemy;
        rb = enemy.rb;
        animator = enemy.animator;
        stats = enemy.stats;
    }
    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        float battleAnimSpeedMultiplier = enemy.battleMoveSpeed / enemy.moveSpeed;
        animator.SetFloat("xVelocity", rb.linearVelocity.x);
        animator.SetFloat("battleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        animator.SetFloat("moveAnimSpeedMultiplier", enemy.moveAnimSpeedMultiplier);
    }
}