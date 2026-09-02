using UnityEngine;


/// <summary>
/// 角色基础属性数据：纯粹的数值配置，供玩家和敌人共享。
/// 为什么用 ScriptableObject 而不是场景组件上的字段：
/// 一份“角色数据设置”资源可被多个实体复用，
/// 策划在项目窗口集中调整数值即可，不需要打开场景逐个改。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/角色数据", fileName = "角色数据设置-")]
public class StatDataSO : ScriptableObject
{
    // 生命资源：maxHealth 决定血条上限，healthRegen 决定脱战后的持续回血速度
    [Header("Resources")]
    public float maxHealth = 100;
    public float healthRegen;

    // 物理进攻：attackSpeed 影响攻击间隔，critPower 是暴击伤害倍率（150 = 1.5 倍），
    // armorReduction 用于削减敌人护甲
    [Header("Offense - Phyiscal Damage")]
    public float attackSpeed = 1;
    public float damage = 10;
    public float critChance;
    public float critPower = 150;
    public float armorReduction;

    // 元素进攻：三种元素伤害独立计算，命中时分别与对应抗性抵消
    [Header("Offense - Elemental Damage")]
    public float fireDamage;
    public float iceDamage;
    public float lightningDamage;

    // 物理防御：armor 直接减伤，evasion 是闪避概率
    [Header("Defense - Phyiscal Damage")]
    public float armor;
    public float evasion;

    // 元素防御：三种元素抗性分别抵消对应元素伤害，互不影响
    [Header("Defense - Elemental Damage")]
    public float fireResistance;
    public float iceResistance;
    public float lightningResistance;

    // 主属性是成长基石：力量加成伤害与暴击威力、活力加成护甲与生命上限，
    // 因此加点/装备只需要堆主属性，派生数值会自动跟着涨
    [Header("Major Stats")]
    public float strength;
    public float agility;
    public float intelligence;
    public float vitality;
}
