using UnityEngine;

/// <summary>
/// 敌人眩晕状态：被反击后触发。
/// 进入时：
/// 1. 关闭反击窗口（防止眩晕期间还能被反击）
/// 2. 隐藏攻击预警图标
/// 3. 给一个朝向反方向的击飞速度（被打飞的效果）
/// 4. 计时器到时后恢复到 Idle 状态
/// </summary>
public class Enemy_StunnedState : EnemyState
{
    private Enemy_VFX enemy_VFX;

    public Enemy_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemy_VFX = enemy.GetComponent<Enemy_VFX>();
    }

    public override void Enter()
    {
        base.Enter();
        // 关闭反击窗口和提示——眩晕期间不可能再触发反击
        enemy.EnableCounterWindow(false);
        enemy_VFX.EnableAttackAlert(false);

        stateTimer = enemy.stunneduration;

        // 向 facingDir 反方向飞，造成"被打飞"的视觉效果
        rb.linearVelocity = new Vector2(enemy.stynnedVelocity.x * -enemy.facingDir, enemy.stynnedVelocity.y);
    }

    public override void Update()
    {
        base.Update();
        // 眩晕时间结束 → 恢复正常巡逻
        if (stateTimer <= 0)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}