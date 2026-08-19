using UnityEngine;

/// <summary>
/// 敌人攻击状态。播放攻击动画，动画事件触发伤害判定。
/// 攻击结束后由动画事件（CurrentStateTrigger）切回战斗状态。
/// 
/// 为什么不在这里加移动逻辑：
/// 攻击动画通常有固定的前摇帧，移动会让动画走样。
/// 攻击过程中的追踪由 BattleState 在攻击前完成定位。
/// </summary>
public class Enemy_AttackState : EnemyState
{
    public Enemy_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        SyncAttackSpeed();
    }


    public override void Update()
    {
        base.Update();
        // triggerCalled 由攻击动画最后一帧的事件触发
        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}