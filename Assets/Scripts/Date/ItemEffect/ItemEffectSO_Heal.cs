using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/治疗效果", fileName = "治疗效果设置-")]
public class ItemEffectSO_Heal : ItemEffectDateSO
{
    [SerializeField] private float healPervent = 0.1f;

    public override void ExecuteEffect()
    {
        Player player = FindAnyObjectByType<Player>();

        float healAmount = player.stats.GetMaxHealth() * healPervent;

        player.health.IncreaseHealth(healAmount);
    }
}
