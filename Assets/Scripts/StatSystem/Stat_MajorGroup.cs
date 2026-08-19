using System;
using UnityEngine;

[Serializable]
public class Stat_MajorGroup
{
    [Header("主要属性")]
    [InspectorName("敏捷")]
    [Tooltip("影响攻击速度和闪避")]
    public Stat agility;

    [InspectorName("力量")]
    [Tooltip("影响物理伤害和暴击威力")]
    public Stat strength;

    [InspectorName("智力")]
    [Tooltip("影响元素伤害")]
    public Stat intelligence;

    [InspectorName("体力")]
    [Tooltip("每点体力提供 5 点额外生命值")]
    public Stat vitality;
}