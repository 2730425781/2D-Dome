using UnityEngine;

/// <summary>
/// 玩家战斗组件。继承自 Entity_ComBat（通用攻击检测）。
/// 扩展了反击（Counter）逻辑：在反击窗口内检测可反击目标并触发 HandleCounter。
/// 
/// 为什么单独拆一个类而不是放在 PlayerState 里：
/// CounterAttack2() 需要访问 Collider2D[]（检测范围内所有敌人），
/// 而 Entity_ComBat 已经封装了 GetDetectedColliders()，直接复用。
/// </summary>
public class Player_Combat : Entity_ComBat
{
    [Header("反击设置")]
    [SerializeField] private float counterDuration;  // 反击窗口持续时间

    /// <summary>
    /// 检测范围内是否存在实现了 ICounterable 的目标。
    /// 返回 true 表示成功反击，供 Player_AttackCounterState 判断是否播放 counterAttack2。
    /// 之所以设计为返回 bool 而不是直接改动画参数，是为了让状态机自己决策。
    /// </summary>
    public bool CounterAttack2()
    {
        bool hasCounter = false;
        foreach (var target in GetDetectedColliders())
        {
            ICounterable counterable = target.GetComponent<ICounterable>();
            if (counterable != null)
            {
                counterable.HandleCounter();
                hasCounter = true;
            }
        }
        return hasCounter;
    }

    public float GetCounterDuration() => counterDuration;
}