using System;
using UnityEngine;

/// <summary>
/// 资源类属性：最大生命值与生命恢复。
/// 与主要属性分开存放，是因为这是所有实体共用的"生存底线"字段，
/// 而主要属性里的体力提供的额外生命会叠加到 GetMaxHealth() 的最终结果上。
/// </summary>
[Serializable]
public class Stat_ResourceGroup
{
    // 基础最大生命值，最终值 = 基础值 + 体力 * 5（见 GetMaxHealth）
    public Stat maxHealth;
    // 每次恢复周期回复的生命量（见 Entity_Health.RegenerateHealth）
    public Stat healthRegen;
}
