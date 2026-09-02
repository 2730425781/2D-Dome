using System;
using UnityEngine;

/// <summary>
/// 单个 Buff 的数据：给哪个属性加多少数值。
/// 为什么是 [Serializable] 普通类而不是 ScriptableObject：
/// 它作为 BuffEffectDate[] 的成员内嵌在 Buff 效果资源里，
/// 需要在 Inspector 中逐项编辑，独立成资源反而无法被数组复用。
/// </summary>
[Serializable]
public class BuffEffectDate
{
    // 修改的属性类型：通过 StatType 映射到玩家身上具体的 Stat 实例
    public StatType type;
    // 数值：正数增益、负数减益；最终由 Stat.AddModifier 决定加算方式
    public float value;
}
