using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/技能点效果", fileName = "技能点效果设置-")]
public class ItemEffectSO_SkillPoint : ItemEffectDateSO
{
    [SerializeField] private int points;

    public override void ExecuteEffect()
    {
        UI ui = FindAnyObjectByType<UI>();
        ui.skillTreeUI.AddSkillPoints(points);
    }
}
