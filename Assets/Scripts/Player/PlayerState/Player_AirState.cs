using UnityEngine;

public class Player_AirState : PlayerState
{
    public Player_AirState(Player player, StateMachine stateMachine, string animboolName) : base(player, stateMachine, animboolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // 空中水平移动？        // - 如果已检测到墙体 → x 速度归零，避免推入墙体导致物理异常        // - 否则按输入方向施加空气减速后的水平速度
            // 墙检测避免推入墙体导致物理抖动；见类注释
            if (player.wallDetected)
            {
                player.SetVelocity(0, rb.linearVelocity.y);
        }
        else if (player.moveInput.x != 0)
        {
            float airSpeed = player.moveSpeed * player.inAirMoveMultiplier;
            player.SetVelocity(player.moveInput.x * airSpeed, rb.linearVelocity.y);
        }

        // 空中攻击 → 切换至跳劈状态        // 不用 WasPressedThisDynamicUpdate，因为空中攻击不需要连击备帧，标准帧间隔就够了
        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.jumpAttackState);
        }
    }
}
/// 空中状态的基类（Fall / Jump 的父类）。
/// 
/// 为什么墙检测放在 AirState 而非每帧在 Entity 层面处理：
/// 墙体检测只对空中状态有意义——地面状态不会滑墙，
/// 把逻辑放在这里避免在 Entity 层面加额外的状态分支。
/// 
/// 为什么用 player.wallDetected 设速度为 0：
/// 空中贴墙时如果不归零，输入方向会让人物推入墙体，
/// 物理引擎会产生抖动/穿透。归零后自然过渡到壁滑状态。
/// </summary>
