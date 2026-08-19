using UnityEngine;

/// <summary>
/// 玩家主动技能的公共基类（Dash、Shard、Time Echo、Sword Throw）。
/// 
/// 为什么和 SkillObject_Base 分开：
/// Skill_Base 是"技能能力"（挂在玩家身上、管理冷却和升级），
/// SkillObject_Base 是"技能实体"（扔出去的剑、碎片等临时对象）。
/// 能力负责生成实体，实体负责伤害结算。
/// </summary>
public class Skill_Base : MonoBehaviour
{
    public Player player { get; private set; }
    public DamageScaleDate damageScaleDate { get; private set; }
    public Player_SkillManager skillManager { get; private set; }

    [Header("技能详情")]
    [SerializeField] protected SkillType skillType;        // 技能身份，用于技能树查询
    [SerializeField] protected SkillUpgradeType upgradeType; // 当前已解锁的升级分支
    [SerializeField] protected float cooldown;              // 冷却时长（秒）
    private float lastTimeToUsed;                           // 上次使用时间戳

    protected virtual void Awake()
    {
        // 用 GetComponentInParent 自动找玩家和技能管理器：
        // 技能组件挂在玩家子物体上，避免在 Inspector 里手动拖引用
        player = GetComponentInParent<Player>();
        skillManager = GetComponentInParent<Player_SkillManager>();
        damageScaleDate = new DamageScaleDate();
    }

    public virtual void TryUseSkill()
    {
        // 基类默认什么都不做；具体行为由各技能子类覆写
    }

    /// <summary>
    /// 技能树解锁时调用，把升级数据应用到技能上。
    /// </summary>
    public void SetSkillUpgrade(UpgradeDate upgrade)
    {
        upgradeType = upgrade.upgradeType;
        cooldown = upgrade.cooldown;
        damageScaleDate = upgrade.damageScaleDate;
        ResetCoolDown();
    }

    public virtual bool CanUseSkill()
    {
        // 升级类型是 None 说明还没在技能树解锁，不能使用
        if (upgradeType == SkillUpgradeType.None)
        {
            return false;
        }

        if (OnCoolDown())
        {
            Debug.Log("技能冷却中");
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 判断当前升级是否恰好等于某个升级分支。
    /// 为什么用相等而不是包含：一个技能一次只能处于一个升级分支，
    /// 子类用这个帮助方法决定是否执行某个效果。
    /// </summary>
    protected bool Unlocked(SkillUpgradeType upgradeCheck) => upgradeType == upgradeCheck;

    // lastTimeToUsed == 0 表示从未使用过，首次使用不算冷却
    protected bool OnCoolDown() => lastTimeToUsed == 0 ? false : Time.time <= lastTimeToUsed + cooldown;
    public void SetSkillOnCoolDown() => lastTimeToUsed = Time.time;
    public void ReduceCoolDownBy(float coolDownReduction) => lastTimeToUsed += coolDownReduction;
    public void ResetCoolDown() => lastTimeToUsed = Time.time - cooldown;
}
