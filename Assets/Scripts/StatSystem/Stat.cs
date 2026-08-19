using System;
using System.Collections.Generic;
using UnityEngine;


/// </summary>
/// Stat 使用"脏标记"缓存模式：
/// - GetValue() 返回缓存值，仅在 needToReset 为 true 时重新计算
/// - AddModifier() / RemoveModifier() 设置 needToReset = true
/// - 这样做的原因是 Stat 可能被多个系统每帧读取（如 UI、伤害计算），
///   但修改（增减 modifier）频率远低于读取，缓存能避免每帧循环 modifiers。
/// 没有做成 getter 自动计算的原因：
/// MonoBehaviour 的 Update 循环中频繁调用会创建大量临时 LINQ 分配，
/// 手动管理缓存让性能更可控。
/// </summary>
[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();

    private bool needToReset = true;
    private float finalValue;

    public float GetValue()
    {
        if (needToReset)
        {
            finalValue = GetFinalValue();
            needToReset = false;
        }
        return finalValue;
    }

    public void AddModifier(float value, string source)
    {
        StatModifier modToAdd = new StatModifier(value, source);
        modifiers.Add(modToAdd);
        needToReset = true;
    }

    public void RemoveModifier(string source)
    {
        modifiers.RemoveAll(modifier => modifier.source == source);
        needToReset = true;
    }

    private float GetFinalValue()
    {
        float finalValue = baseValue;

        foreach (var modifier in modifiers)
        {
            finalValue += modifier.value;
        }
        return finalValue;
    }

    public void SetBaseValue(float value) => baseValue = value;
}

[Serializable]
public class StatModifier
{
    public float value;
    public string source;

    public StatModifier(float value, string source)
    {
        this.value = value;
        this.source = source;
    }
}
