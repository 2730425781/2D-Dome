using UnityEngine;

/// <summary>
/// 领域展开技能对应的玩家状态：角色向上腾空并悬浮，期间展开领域。
/// 
/// 为什么需要专门的状态而不是复用跳跃/下落状态：领域期间要关闭重力、固定腾空
/// 距离、持续施法且全程无敌不可被打断，这些行为与其他空中状态差异太大。
/// 复用 "jumpfall" 动画参数（见 Player.Awake），无需新增动画状态。
/// </summary>
public class Player_DomainExpansionState : PlayerState
{
    // 进入状态时的快照：腾空距离以"进入时的位置"为基准，退出时必须恢复原始重力
    private Vector2 originalPosition;
    private float originalGravity;
    private float finalRiseDistance;

    // 阶段标记：isLevitating 区分"上升中/悬浮中"，createdDomain 保证领域只生成一次
    private bool isLevitating;
    private bool createdDomain;

    public Player_DomainExpansionState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    /// <summary>
    /// 进入领域状态：先快照位置与重力，再以 riseSpeed 向上冲。
    /// 为什么立刻设置无敌：悬浮期间玩家无法移动，若可受伤则击退会打断领域
    /// （击退会覆盖速度、可能切状态），整个演出期间关闭受击，退出时恢复。
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        originalPosition = player.transform.position;
        originalGravity = rb.gravityScale;
        finalRiseDistance = GetRiseDistance();

        player.SetVelocity(0, player.riseSpeed);
        player.health.SetCanTakedamage(false);
    }

    /// <summary>
    /// 每帧：先判断是否已腾空到目标距离，到达则切换为悬浮；悬浮期间驱动领域
    /// 持续施法，并靠 stateTimer 倒计时决定何时结束（领域时长耗尽）。
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (Vector2.Distance(originalPosition, player.transform.position) >= finalRiseDistance && !isLevitating)
        {
            Levitate();
        }
        if (isLevitating)
        {
            // 悬浮期间每帧驱动施法循环：领域实体负责展开/收缩，技能类负责施法节奏
            skillManager.domainExpansion.DoSpellCasting();

            // stateTimer 在 Levitate 中被设为领域时长，归零代表领域寿命已尽：
            // 恢复重力并回到待机，领域实体随后自行收缩销毁
            if (stateTimer <= 0)
            {
                isLevitating = false;
                rb.gravityScale = originalGravity;
                stateMachine.ChangeState(player.idleState);
            }
        }
    }

    /// <summary>
    /// 退出状态：恢复受击能力，并重置 createdDomain。
    /// 为什么重置放在 Exit 而不是 Enter：Exit 是所有退出路径（正常结束/被打断）的
    /// 统一收尾点，下次再进入领域时可以重新生成领域。
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        createdDomain = false;
        player.health.SetCanTakedamage(true);
    }

    /// <summary>
    /// 从上升切换到悬浮：速度清零、关闭重力，把 stateTimer 设为领域时长，
    /// 并（仅一次）生成领域实体。
    /// </summary>
    private void Levitate()
    {
        isLevitating = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        stateTimer = skillManager.domainExpansion.GetDomainDuration();

        // createdDomain 守卫：悬浮期间只生成一次领域，后续帧不再重复创建
        if (createdDomain == false)
        {
            createdDomain = true;
            skillManager.domainExpansion.CreateDomain();
        }
    }

    /// <summary>
    /// 计算本次腾空的目标距离：向头顶打一条射线，若上方有地形则只升到地形下方
    /// （-1 是安全间隙，避免头部顶进天花板），无阻挡则升满 maxRiseDistance。
    /// 用 groundLayer 检测是因为天花板与地面共用同一地形图层，无需另配新 LayerMask。
    /// </summary>
    private float GetRiseDistance()
    {
        RaycastHit2D hit2D = Physics2D.Raycast(player.transform.position, Vector2.up, player.maxRiseDistance, player.groundLayer);

        return hit2D.collider != null ? hit2D.distance - 1 : player.maxRiseDistance;
    }
}
