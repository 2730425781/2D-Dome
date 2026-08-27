using UnityEngine;

/// <summary>
/// 玩家专属动画事件接收器。
/// 为什么在 Entity_AnimationTriggers 之外再派生子类：
/// 基础类只处理通用事件（攻击判定、状态触发），投掷飞剑是玩家独有技能，
/// 不能把玩家逻辑塞进通用基类，否则敌人也会被迫实现。
/// 用 GetComponentInParent 而不是 GetComponent：本脚本挂在带 Animator 的子物体上，
/// 而 Player 组件在根物体上（与 Entity_AnimationTriggers.Awake 同理）。
/// </summary>
public class Player_AnimationTriggers : Entity_AnimationTriggers
{
    private Player player;

    protected override void Awake()
    {
        // 先调 base 拿到 Entity 引用，再拿 Player 引用；都在 Awake 缓存，避免事件触发时每帧查找
        base.Awake();
        player = GetComponentInParent<Player>();
    }

    // 由投剑动画片段中的 Animation Event 调用；能否释放由 TryUseSkill 内部判断（冷却/条件），动画只管发信号
    private void ThrowSword() => player.skillManager.swordThrow.TryUseSkill();
}
