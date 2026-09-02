using UnityEngine;

/// <summary>
/// 消耗品数据：继承 ItemDateSO 的空子类。
/// 为什么需要一个空类而不是直接用 ItemDateSO：
/// 用类型本身作为“可消耗”的标记，背包/UI 靠类型区分物品种类，
/// 以后要在消耗品上加专属字段（如使用音效）时也无需改动基类。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/消耗物品", fileName = "消耗物品设置-")]
public class ConsumableItemDateSO : ItemDataSO
{

}
