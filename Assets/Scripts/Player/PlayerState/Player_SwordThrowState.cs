using UnityEngine;

/// <summary>
/// 投剑状态：玩家瞄准时显示轨迹点，松开/按下后朝鼠标方向扔出剑。
/// 
/// 为什么单独做一个状态而不是直接调用技能：
/// 投剑需要"瞄准 → 确认方向 → 播放动画"这整个流程，
/// 状态机负责管理流程和动画参数，技能脚本只负责生成剑实体。
/// </summary>
public class Player_SwordThrowState : PlayerState
{
    private Camera mainCamera;

    public Player_SwordThrowState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        skillManager.swordThrow.EnableDots(true);

        // 缓存主相机：鼠标屏幕坐标需要转换到世界坐标
        if (mainCamera != Camera.main)
        {
            mainCamera = Camera.main;
        }
    }

    public override void Update()
    {
        base.Update();

        Vector2 dirToMouse = DiretionToMouse();
        player.HandleFlip(dirToMouse.x);
        // 每帧刷新预测轨迹，让玩家看到剑的飞行路径
        skillManager.swordThrow.PredictTrajectory(dirToMouse);

        // 瞄准时角色固定不动
        player.SetVelocity(0, rb.linearVelocity.y);

        if (input.Player.Attack.WasPressedThisFrame())
        {
            animator.SetBool("swordThrowPerformed", true);

            // 确认方向后隐藏轨迹点，交给动画事件实际投剑
            skillManager.swordThrow.EnableDots(false);
            skillManager.swordThrow.ConfirmTrajectory(dirToMouse);
        }

        // 松开蓄力键或动画结束 → 退出状态
        if (input.Player.RangeAttack.WasReleasedThisFrame() || triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        animator.SetBool("swordThrowPerformed", false);
        skillManager.swordThrow.EnableDots(false);
    }

    /// <summary>
    /// 计算从玩家指向鼠标的单位方向。
    /// </summary>
    private Vector2 DiretionToMouse()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(player.mousePosition);

        Vector2 direction = mousePosition - playerPosition;

        return direction.normalized;
    }
}
