using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/技能重置效果", fileName = "技能重置效果设置-")]
public class ItemEffectSO_RefundSkill : ItemEffectDateSO
{
    public override void ExecuteEffect()
    {
        UI ui = FindAnyObjectByType<UI>();
        ui.skillTreeUI.RefundAllSkills();
    }
}
