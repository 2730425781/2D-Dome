using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/物理吸血", fileName = "物理吸血设置-")]
public class ItemEffectSO_HealOnAttack : ItemEffectDateSO
{
    [SerializeField] private float percentHealOnDamage = 0.2f;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        player.combat.OnDoingPhysicalDamage += HealOnDamage;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        player.combat.OnDoingPhysicalDamage -= HealOnDamage;
        player = null;
    }

    private void HealOnDamage(float damage)
    {
        player.health.IncreaseHealth(damage * percentHealOnDamage);
    }
}
