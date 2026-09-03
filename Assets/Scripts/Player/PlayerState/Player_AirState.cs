using UnityEngine;

/// <summary>
/// 空中状态的基类（Fall / Jump / WallJump 的父类）。
///
/// 为什么墙检测放在 AirState 而非每帧在 Entity 层面处理：
/// 墙体检测只对空中状态有意义——地面状态不会滑墙，
/// 把逻辑放在这里避免在 Entity 层面加额外的状态分支。
///
/// 为什么用 player.wallDetected 设速度为 0：
/// 空中贴墙时如果不归零，输入方向会让人物推入墙体，
/// 物理引擎会产生抖动/穿透。归零后自然过渡到壁滑状态。
///
/// 为什么"上升锁定"用逐帧计数器而不是协程：
/// 各状态是普通类（非 MonoBehaviour），无法 StartCoroutine。
/// 好在状态机每帧由 Entity.Update 驱动 currentState.Update()，
/// 所以在 Update 里用 airInputLockTimer -= Time.deltaTime 即可实现
/// "锁定一段时间后自动恢复"，且能自然地在状态切换（如转入下落）时失效。
/// </summary>
public class Player_AirState : PlayerState
{
    // 空中水平输入锁定的剩余时间；>0 时忽略水平方向输入，让角色沿跳跃初速直上。
    protected float airInputLockTimer = 0f;

    public Player_AirState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    /// <summary>
    /// 开启一段"上升锁定"：锁定期间不响应水平方向输入，锁定结束后恢复空中转向。
    /// 只在起跳（或需要"起跳初段直上"的状态）里调用；下落等状态不调用即可保持默认操控。
    /// </summary>
    protected void LockAirInputFor(float duration)
    {
        airInputLockTimer = duration;
    }

    public override void Update()
    {
        base.Update();

        // 空中水平移动。上升锁定期间忽略水平方向输入，让角色沿跳跃初速直上；
        // 锁定结束才恢复 moveInput 对 x 速度的控制（转向/加速）。
        // 注意：只锁定方向操控，攻击等其它输入不受影响。
        if (airInputLockTimer > 0f)
        {
            airInputLockTimer -= Time.deltaTime;
        }
        else if (player.wallDetected)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }
        else if (player.moveInput.x != 0)
        {
            float airSpeed = player.moveSpeed * player.inAirMoveMultiplier;
            player.SetVelocity(player.moveInput.x * airSpeed, rb.linearVelocity.y);
        }

        // 空中攻击 → 切换至跳劈状态
        // 不用 WasPressedThisDynamicUpdate，因为空中攻击不需要连击备帧，标准帧间隔就够了
        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.jumpAttackState);
        }
    }

    public override void Enter()
    {
        base.Enter();
        animator.SetBool("inAir", true);
    }

    public override void Exit()
    {
        base.Exit();
        animator.SetBool("inAir", false);
    }
}
