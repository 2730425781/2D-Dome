using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 地面增益拾取物：玩家触碰后获得若干属性增益，持续 buffDuration 秒。
///
/// 为什么做成世界中的触发器对象而不是背包消耗品：这类 Buff 更接近
/// "场景里的强化道具"（有漂浮表现、靠近即生效），与从背包使用的
/// ItemEffect 是两种不同的交付方式，各自独立。
///
/// 为什么应用逻辑在 Player_Stats 而不是本类：Buff 的生效/移除是持续
/// buffDuration 秒的协程（见 Player_Stats.BuffCo），若放在本对象上，
/// ApplyBuff 后立刻 Destroy 会把协程一并杀掉；由玩家自己管理才能
/// 活过拾取物的销毁。
/// </summary>
public class Object_Buff : MonoBehaviour
{
    // 由触发器碰撞者身上的 Player_Stats 填充
    private Player_Stats statsToModify;

    [Header("Buff设置")]
    // 用数组而非单个效果：一个拾取物可以同时给多个属性加值
    [SerializeField] private BuffEffectDate[] buffs;
    // Buff 的唯一身份，作为属性修饰符的 source 键（见 Player_Stats.CanApplyBuff）。
    // 两个同名拾取物会被视为同一个 Buff，无法同时生效——命名必须互不相同
    [SerializeField] private string buffName;
    //[SerializeField] private float buffValue = 5;
    // 增益持续时间（秒）：4 秒属于短时爆发型增益，节奏上接近"吃了马上变强"
    [SerializeField] private float buffDuration = 4;
    [Header("Buff显示设置")]
    // 漂浮表现参数：1.0 速度 / 0.1 幅度，让道具轻微上下浮动以引起注意
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float floatRange = 0.1f;
    // 浮动基准点：位置每帧在变，必须先记住初始位置，
    // 否则按"当前位置+偏移"叠加会让道具越飘越远
    private Vector3 startPosition;

    /// <summary>
    /// 在 Awake 记录浮动基准点：浮动从第一帧 Update 就开始，
    /// 必须在此之前拿到未偏移的初始位置。
    /// </summary>
    private void Awake()
    {
        startPosition = transform.position;
    }

    /// <summary>
    /// 正弦浮动让道具"活"一点，吸引玩家注意；用正弦而非线性移动，
    /// 保证动画首尾平滑、无跳变。
    /// </summary>
    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = startPosition + new Vector3(0, yOffset);
    }

    /// <summary>
    /// 触碰即生效。注意：这里没有对 statsToModify 判空，依赖场景中只有
    /// 带 Player_Stats 的玩家会碰到它，否则会空引用崩溃。
    /// Destroy 放在 CanApplyBuff 通过之后：同名 Buff 生效期间拾取物保留在原地，
    /// 等效果结束后玩家还能再捡一次（生效中不会刷新持续时间）。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 只有带 Player_Stats 的角色（玩家）才能吃 Buff
        statsToModify = collider.GetComponent<Player_Stats>();

        // 同名 Buff 生效期间禁止叠加，防止无限刷新/数值越叠越高
        if (statsToModify.CanApplyBuff(buffName))
        {
            // 应用与移除都在玩家侧异步完成，拾取物销毁不影响 Buff 继续计时
            statsToModify.ApplyBuff(buffs, buffDuration, buffName);
            Destroy(gameObject);
        }
    }
}
