using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/冰暴效果", fileName = "冰暴效果设置-")]
public class ItemEffectSO_IceBlast : ItemEffectDateSO
{
    [SerializeField] private ElementalEffectDate effectDate;
    [SerializeField] private float iceDamage;
    [SerializeField] private LayerMask enemyLayer;

    [Space]
    [SerializeField] private float cooldown;
    [SerializeField] private float healthPercentTriggler = 0.25f;
    private float lastTimeUsed = -999;
    [SerializeField] private GameObject iceBlastVFX;
    [SerializeField] private GameObject onHitVFX;

    public override void ExecuteEffect()
    {
        bool noCooldown = Time.time >= lastTimeUsed + cooldown;
        bool reachedThreshold = player.health.GetHealthPercentage() <= healthPercentTriggler;

        if (noCooldown && reachedThreshold)
        {
            lastTimeUsed = Time.time;
            player.vfx.CreateEffectOf(iceBlastVFX, player.transform);
            DamageEnemiesWithIce();
        }
    }
    private void DamageEnemiesWithIce()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, 2f, enemyLayer);

        foreach (var enemy in enemies)
        {
            IDamagable damagable = enemy.GetComponent<IDamagable>();

            if (damagable == null) continue;

            bool enemyGotHit = damagable.TakeDamage(0, iceDamage, ElementType.Ice, player.transform);
            Entity_StatusHandler statusHandler = enemy.GetComponent<Entity_StatusHandler>();

            statusHandler?.ApplyStatusEffect(ElementType.Ice, effectDate);

            if (enemyGotHit)
            {
                // /Debug.Log("11111");
                player.vfx.CreateEffectOf(onHitVFX, enemy.transform);
            }
        }
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        player.health.OnTakingDamage += ExecuteEffect;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        player.health.OnTakingDamage -= ExecuteEffect;
        player = null;
    }
}
