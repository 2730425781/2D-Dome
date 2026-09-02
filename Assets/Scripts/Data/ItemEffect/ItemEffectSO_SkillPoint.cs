using UnityEngine;

/// <summary>
/// 技能点效果：使用物品后直接给玩家增加技能点，用于技能树升级。
/// 为什么走 UI 而不是改玩家数据：技能点的入口在技能树 UI 上，
/// AddSkillPoints 会同步刷新界面，直接调用它就能保证数据与显示一致，无需额外发刷新通知。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/技能点效果", fileName = "技能点效果设置-")]
public class ItemEffectSO_SkillPoint : ItemEffectDateSO
{
    [SerializeField] private int points;

    public override void ExecuteEffect()
    {
        // 主动效果一次性结算，临时查找 UI 实例即可，无需持有长期引用
        UI ui = FindAnyObjectByType<UI>();
        ui.skillTreeUI.AddSkillPoints(points);
    }
}
