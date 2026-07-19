using UnityEngine;

public class Player_AirState : PlayerState
{
    public Player_AirState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // 空中水平移动：
        // - 如果已检测到墙体 → x 速度归零，避免推入墙体导致物理异常
        // - 否则按输入方向施加空气减速后的水平速度
        if (player.wallDetected)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }
        else if (player.moveInput.x != 0)
        {
            float airSpeed = player.moveSpeed * player.inAirMoveMulyiplier;
            player.SetVelocity(player.moveInput.x * airSpeed, rb.linearVelocity.y);
        }

        // 空中攻击 → 切换至跳劈状态
        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.jumpAttackState);
        }
    }
}