using UnityEngine;

/// <summary>
/// 玩家战斗组件。继承自 Entity_Combat（通用攻击检测）。
/// 扩展了反击（Counter）逻辑：在反击窗口内检测可反击目标并触发 HandleCounter。
/// 
/// 为什么单独拆一个类而不是放在 PlayerState 里：
/// CounterAttack2() 需要访问 Collider2D[]（检测范围内所有敌人），
/// 而 Entity_Combat 已经封装了 GetDetectedColliders()，直接复用。
/// </summary>
public class Player_Combat : Entity_Combat
{
    private float lastCounterTime = -999f;  // 上次反击的时间戳（-999 保证一开局就能用）

    /// <summary>
    /// 当前是否可以使用反击（冷却已过）。
    /// 用 Time.time 判断，跟状态机无关，跨状态迁移依然有效。
    /// </summary>
    public bool CanCounter => Time.time >= lastCounterTime + counterTimeReset;

    /// <summary>
    /// 标记反击已被使用，记录当前时间作为冷却起点。
    /// </summary>
    public void MarkCounterUsed() => lastCounterTime = Time.time;


    [Header("反击设置")]
    [SerializeField] private float counterRecovery = 0.1f;  // 反击窗口持续时间
    [SerializeField] private float counterTimeReset = 1.5f;  //反击冷却时间

    /// <summary>
    /// 检测范围内是否存在实现了 ICounterable 的目标。
    /// 返回 true 表示成功反击，供 Player_CounterAttackState 判断是否播放 counterAttack2。
    /// 之所以设计为返回 bool 而不是直接改动画参数，是为了让状态机自己决策。
    /// </summary>
    public bool CounterAttack2()
    {
        bool hasCounter = false;
        foreach (var target in GetDetectedColliders())
        {
            ICounterable counterable = target.GetComponent<ICounterable>();
            if (counterable == null)
            {
                continue;
            }
            if (counterable.CanBeCountered)
            {
                counterable.HandleCounter();
                hasCounter = true;
            }
        }
        return hasCounter;
    }

    public float GetCounterRecoveryDuration() => counterRecovery;
}
/// 为什么用 float 时间戳（Time.time）来判断冷却，而非 stateTimer：
/// stateTimer 在每次状态切换时会被 EntityState.Update() 的倒计时重置，
/// 而 CanCounter 的冷却需要跨状态维持（待机、移动、攻击全阶段生效），
/// 放在组件级别的 Time.time 比较上更合适，不依赖状态机生命周期。
/// 
/// CounterAttack2() 返回 bool 而非 void：
/// Player_CounterAttackState 需要知道是否反击成功来决定播放哪段动画，
/// 返回 bool 让调用方自己决策，避免在此处写死动画逻辑。
/// </summary>
