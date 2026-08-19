using UnityEngine;

/// <summary>
/// 玩家状态的基类。
/// 之所以从 EntityState 再派生一层 PlayerState，是因为：
/// 1. Player 有专门的 PlayerInputSet，跟通用的 Input 解耦
/// 2. 冲刺检测逻辑放在这里，所有玩家地面/空中状态共享
/// 3. 动画参数同步（xVelocity/yVelocity）统一在 UpdateAnimationParameters 中完成
/// </summary>
public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;
    protected Player_SkillManager skillManager;

    public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.player = player;
        animator = player.animator;
        rb = player.rb;
        input = player.input;
        stats = player.stats;
        skillManager = player.skillManager;
    }

    public override void Update()
    {
        base.Update();

        // 通用冲刺检测：按下冲刺键且条件满足时切到冲刺状态
        // 放在这里而不是每个子类里重复写，避免遗漏
        if (input.Player.Dash.WasPressedThisFrame() && CanDash())
        {
            skillManager.dash.SetSkillOnCoolDown();
            stateMachine.ChangeState(player.dashState);
        }

        if (input.Player.Skill_R.WasPressedThisFrame() && skillManager.domainExpansion.CanUseSkill())
        {
            if (skillManager.domainExpansion.InstantDomain())
            {
                skillManager.domainExpansion.CreateDomain();
            }
            else
            {
                stateMachine.ChangeState(player.domainExpansionState);
            }

            skillManager.domainExpansion.SetSkillOnCoolDown();
        }
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        // 同步 x/y 速度到 Animator，供 JumpFall blend tree 使用
        // blend tree 通过 yVelocity 在 jump（>0）和 fall（<0）之间混合
        animator.SetFloat("xVelocity", rb.linearVelocity.x);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    /// <summary>
    /// 冲刺条件：贴墙时不能冲刺（撞墙取消），已经在冲刺中也不再触发。
    /// </summary>
    private bool CanDash()
    {
        if (!skillManager.dash.CanUseSkill())
            return false;
        if (player.wallDetected)
            return false;
        if (stateMachine.currentState == player.dashState || stateMachine.currentState == player.domainExpansionState)
            return false;

        return true;
    }
}