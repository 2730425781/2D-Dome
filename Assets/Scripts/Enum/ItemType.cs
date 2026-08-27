using UnityEngine;

/// <summary>
/// 物品分类：装备槽（武器/防具/饰品）按此匹配 slotType，
/// 材料不进入装备/使用流程，消耗品走一次性使用逻辑，
/// UI（tooltip、右键菜单）也依赖此枚举决定展示与交互分支。
/// 用枚举而不是字符串，保证 Inspector 配置和代码判断不会出现拼写不一致。
/// </summary>
public enum ItemType
{
    Material,
    Weapon,
    Armor,
    Trinket,
    Consumable,
}
