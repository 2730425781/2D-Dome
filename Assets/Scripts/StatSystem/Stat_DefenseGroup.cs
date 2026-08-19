using System;
using UnityEngine;

[Serializable]
public class Stat_DefenseGroup
{
    [Header("物理防御")]
    [InspectorName("护甲")]
    [Tooltip("减少受到的物理伤害")]
    public Stat armor;

    [InspectorName("闪避")]
    [Tooltip("闪避物理攻击的几率")]
    public Stat evasion;

    [Header("元素抗性")]
    [InspectorName("火焰抗性")]
    [Tooltip("减少受到的火焰伤害")]
    public Stat fireRes;

    [InspectorName("冰冻抗性")]
    [Tooltip("减少受到的冰冻伤害")]
    public Stat iceRes;

    [InspectorName("雷电抗性")]
    [Tooltip("减少受到的雷电伤害")]
    public Stat lightningRes;
}