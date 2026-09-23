using UnityEngine;

/// <summary>
/// 元素类型枚举：伤害、抗性、状态效果共用同一套取值，
/// 避免各处分别用字符串/数字导致对不上的问题。
/// None 表示无元素（纯物理），GetElementalDamage 在最高元素伤害 <= 0 时返回 None。
/// </summary>
public enum ElementType
{
    None,
    Fire,
    Ice,
    Lightning
}
