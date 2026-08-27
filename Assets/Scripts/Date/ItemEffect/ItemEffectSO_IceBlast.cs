using UnityEngine;

/// <summary>
/// 冰暴效果（被动保命）：玩家受伤且生命跌破阈值时，自动对周围敌人释放一次冰系爆发。
/// 为什么用“受伤事件 + 血线阈值”而不是主动使用：
/// 它是护身符式的紧急手段，需要在最危急的瞬间自动生效，玩家来不及手动操作，
/// 因此设计成订阅受伤事件来驱动。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/冰暴效果", fileName = "冰暴效果设置-")]
public class ItemEffectSO_IceBlast : ItemEffectDateSO
{
    [SerializeField] private ElementalEffectDate effectDate;
    [SerializeField] private float iceDamage;
    [SerializeField] private LayerMask enemyLayer;

    [Space]
    [SerializeField] private float cooldown;
    // 生命低于 25% 时触发：阈值太高会让效果过早浪费，太低则玩家可能等不到触发就阵亡
    [SerializeField] private float healthPercentTriggler = 0.25f;
    // 上次触发时间；初始 -999 表示“从未触发”，保证第一次受伤就能生效而不被冷却挡住
    private float lastTimeUsed = -999;
    [SerializeField] private GameObject iceBlastVFX;
    [SerializeField] private GameObject onHitVFX;

    public override void ExecuteEffect()
    {
        // 两个条件必须同时满足：冷却已过 + 血量跌破阈值。
        // 冷却限制触发频率，防止连续小伤害把效果变成无限连发
        bool noCooldown = Time.time >= lastTimeUsed + cooldown;
        bool reachedThreshold = player.health.GetHealthPercentage() <= healthPercentTriggler;

        if (noCooldown && reachedThreshold)
        {
            lastTimeUsed = Time.time;
            // 先播放大范围冰爆特效，再结算伤害，视觉与伤害同步发生
            player.vfx.CreateEffectOf(iceBlastVFX, player.transform);
            DamageEnemiesWithIce();
        }
    }
    private void DamageEnemiesWithIce()
    {
        // 以玩家为中心 2 单位半径做圆形范围检测。
        // 用 OverlapCircleAll 一次性取回所有敌人，比逐帧逐个检测更省
        Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, 2f, enemyLayer);

        foreach (var enemy in enemies)
        {
            // 只有实现 IDamagable 的物体才会受伤（空气墙、装饰物没有该接口，直接跳过）
            IDamagable damagable = enemy.GetComponent<IDamagable>();

            if (damagable == null) continue;

            // 物理伤害传 0：冰暴是纯冰系伤害，不走物理减伤
            bool enemyGotHit = damagable.TakeDamage(0, iceDamage, ElementType.Ice, player.transform);
            // 附带冰系状态（冰冻/减速）；用 ?. 防止敌人没有状态处理器时空引用
            Entity_StatusHandler statusHandler = enemy.GetComponent<Entity_StatusHandler>();

            statusHandler?.ApplyStatusEffect(ElementType.Ice, effectDate);

            if (enemyGotHit)
            {
                // /Debug.Log("11111");
                // 命中反馈特效：让玩家直观看到这次爆发实际打中了谁
                player.vfx.CreateEffectOf(onHitVFX, enemy.transform);
            }
        }
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        // 被动效果：装备期间持续监听受伤事件，受伤即检查是否触发冰暴
        player.health.OnTakingDamage += ExecuteEffect;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        // 必须取消订阅并清空引用：否则卸下装备后事件仍回调，造成重复触发或空引用
        player.health.OnTakingDamage -= ExecuteEffect;
        player = null;
    }
}
