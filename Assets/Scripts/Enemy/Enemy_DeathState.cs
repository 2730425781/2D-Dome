using UnityEngine;

/// <summary>
/// 敌人死亡状态。
/// 
/// 几个设计选择的原因：
/// 1. 关闭 Animator 而不是播放死亡动画：敌人死亡时用一个简单的弹跳物理效果
///    代替动画，视觉上比播固定动画更有随机感。
/// 2. 关闭 Collider：防止死亡后还能被玩家攻击到。
/// 3. 增大 gravityScale：让尸体加速下落，不飘在半空。
/// 4. SwitchOffStateMachine：死亡后不再处理任何状态切换，避免误操作。
/// </summary>
public class Enemy_DeathState : EnemyState
{
    private Collider2D col;

    public Enemy_DeathState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        col = enemy.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        animator.enabled = false;       // 关闭 Animator，不再播放任何动画
        col.enabled = false;            // 关闭碰撞器，不再受物理碰撞影响
        rb.gravityScale = 12;           // 死亡后快速下落
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15); // 向上弹一下再落下
        stateMachine.SwitchOffStateMachine(); // 永久关闭状态机，防止后续意外切换
    }
}