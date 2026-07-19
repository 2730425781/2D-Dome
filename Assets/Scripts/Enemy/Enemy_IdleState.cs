using UnityEngine;

/// <summary>
/// 敌人待机状态：静止不动等待 idleTime 后切换到巡逻状态。
/// 这样做让敌人有自然的巡逻节奏，而不是一直来回走个不停。
/// </summary>
public class Enemy_IdleState : Enemy_GroundState
{
    public Enemy_IdleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 待机时停住水平速度，避免残速滑行
        // 下面这行被注释掉是因为敌人待机时 Rigidbody 应该已经自然停住了，
        // 如果遇到滑行问题可以取消注释
        // enemy.SetVelocity(0, rb.linearVelocity.y);
        stateTimer = enemy.idleTime;
    }

    public override void Update()
    {
        base.Update();
        // 待机时间到 → 开始巡逻
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}