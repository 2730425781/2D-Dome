using UnityEngine;

/// <summary>
/// 技能重置效果：使用后退还所有已花费的技能点。
/// 为什么把“洗点”做成物品效果而不是技能树自带的免费按钮：
/// 让重置成为一种需要消耗资源的操作，玩家必须付出物品代价才能重新规划加点，
/// 避免随意洗点破坏养成节奏。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/技能重置效果", fileName = "技能重置效果设置-")]
public class ItemEffectSO_RefundSkill : ItemEffectDateSO
{
    public override void ExecuteEffect()
    {
        // 主动效果一次性结算，临时查找 UI 实例即可
        UI ui = FindAnyObjectByType<UI>();
        ui.skillTreeUI.RefundAllSkills();
    }
}
