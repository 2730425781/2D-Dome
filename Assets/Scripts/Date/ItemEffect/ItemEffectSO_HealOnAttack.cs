using UnityEngine;

/// <summary>
/// 物理吸血：每次对敌人造成物理伤害时按比例回复生命。
/// 这是被动效果，所以重写 Subscribe/Unsubscribe 订阅战斗事件，
/// 而不是像治疗药水那样在 ExecuteEffect 里一次性结算——
/// 吸血必须挂在“每次命中”上才能持续生效。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/物理吸血", fileName = "物理吸血设置-")]
public class ItemEffectSO_HealOnAttack : ItemEffectDateSO
{
    // 回复比例：造成的物理伤害 × 20%。
    // 用百分比而非固定值，让吸血收益随玩家伤害成长而同步成长，后期不会失效。
    [SerializeField] private float percentHealOnDamage = 0.2f;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        // 订阅“造成物理伤害”事件：命中即触发回血，无需每帧检测
        player.combat.OnDoingPhysicalDamage += HealOnDamage;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        // 必须取消订阅：否则卸下装备后事件仍会回调到已失效的效果，造成重复回血甚至空引用
        player.combat.OnDoingPhysicalDamage -= HealOnDamage;
        // 清空引用，避免悬挂引用被误用
        player = null;
    }

    private void HealOnDamage(float damage)
    {
        // 用“结算后的实际伤害”做基数，比面板攻击力更贴近真实输出
        player.health.IncreaseHealth(damage * percentHealOnDamage);
    }
}
