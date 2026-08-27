using UnityEngine;

/// <summary>
/// 主动治疗效果：使用物品时按最大生命值的百分比恢复生命。
/// 为什么这里是主动结算而不是订阅事件：
/// 药水是一次性消费，使用的那一刻结算即可，不需要长期持有玩家引用，
/// 所以直接在 ExecuteEffect 里临时查找场景中的玩家。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/治疗效果", fileName = "治疗效果设置-")]
public class ItemEffectSO_Heal : ItemEffectDateSO
{
    // 恢复比例：最大生命值的 10%。
    // 用百分比而非固定值，让治疗量随玩家成长，避免后期药水变成“毛毛雨”。
    [SerializeField] private float healPervent = 0.1f;

    public override void ExecuteEffect()
    {
        // 主动效果不经过 Subscribe，这里临时查找玩家实例
        Player player = FindAnyObjectByType<Player>();

        // 以最大生命值为基数，保证比例治疗始终换算成有意义的绝对数值
        float healAmount = player.stats.GetMaxHealth() * healPervent;

        player.health.IncreaseHealth(healAmount);
    }
}
