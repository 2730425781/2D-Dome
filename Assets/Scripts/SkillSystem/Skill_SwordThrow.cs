using UnityEngine;

/// <summary>
/// 投剑技能：根据技能树解锁的升级分支，生成对应的剑（普通/穿刺/旋转/反弹）。
/// 
/// 为什么技能管理器里要配四个预制体和一个 throwPower：
/// 每个升级分支需要不同的剑实体和抛掷力度，集中在这里配置，
/// 生成剑时根据 upgradeType 选择正确的组合。
/// </summary>
public class Skill_SwordThrow : Skill_Base
{
    private SkillObject_Sword currentSword;
    private float currentThrowPower;

    [Header("普通剑升级")]
    [SerializeField] private GameObject swordPrefab;
    [Range(0, 10)]
    [SerializeField] private float regularThrowPower = 5f;

    [Header("穿刺剑升级")]
    [SerializeField] private GameObject pierceSwordPerfab;
    public int amountToPierces = 3;
    [Range(0, 10)]
    [SerializeField] private float pierceThrowPower = 5f;

    [Header("旋转剑升级")]
    [SerializeField] private GameObject spinSwordPrefab;
    public int maxDistance = 5;
    public float attacksPerSecond = 3;
    public float maxSpinDuration = 3f;
    [Range(0, 10)]
    [SerializeField] private float spinThrowPower = 5f;

    [Header("反弹剑升级")]
    [SerializeField] private GameObject bounceSwordPrefab;
    public int bounceCount = 5;
    public float bounceSpeed = 12;
    [Range(0, 10)]
    [SerializeField] private float bounceThrowPower = 5f;

    [Header("运动轨迹")]
    [SerializeField] private GameObject predictionDot;
    [SerializeField] private int numberOfDots = 20;
    [SerializeField] private float spaceBetweenDots = 0.05f;
    private float swordgravity;
    private Transform[] dots;
    private Vector2 confirmedDirection;

    protected override void Awake()
    {
        base.Awake();
        dots = GenerateDots();
        // 读取剑的重力系数：预测轨迹时用来计算重力偏移
        swordgravity = swordPrefab.GetComponent<Rigidbody2D>().gravityScale;
    }

    public override bool CanUseSkill()
    {
        UpdateThrowPower();

        // 场上已经有一把剑：不生成第二把，而是把旧剑召回
        if (currentSword != null)
        {
            currentSword.GetSwordBackToPlayer();
            return false;
        }

        return base.CanUseSkill();
    }

    public override void TryUseSkill()
    {
        GameObject swordPrefab = GetSwordPrefab();
        GameObject newSword = Instantiate(swordPrefab, dots[1].position, Quaternion.identity);

        currentSword = newSword.GetComponent<SkillObject_Sword>();
        currentSword.SetupSword(this, GetThorwDir());

        SetSkillOnCoolDown();
    }

    private GameObject GetSwordPrefab()
    {
        // 按当前升级分支返回对应预制体
        if (Unlocked(SkillUpgradeType.SwordThrow))
        {
            return swordPrefab;
        }
        if (Unlocked(SkillUpgradeType.SwordThrow_Pierce))
        {
            return pierceSwordPerfab;
        }
        if (Unlocked(SkillUpgradeType.SwordThrow_Spin))
        {
            return spinSwordPrefab;
        }
        if (Unlocked(SkillUpgradeType.SwordThrow_Bounce))
        {
            return bounceSwordPrefab;
        }
        else
        {
            Debug.Log("未拥有该技能");
            return null;
        }
    }

    private void UpdateThrowPower()
    {
        // 不同升级分支的抛掷力度不同（比如旋转剑更重）
        switch (upgradeType)
        {
            case SkillUpgradeType.SwordThrow:
                currentThrowPower = regularThrowPower;
                break;
            case SkillUpgradeType.SwordThrow_Pierce:
                currentThrowPower = pierceThrowPower;
                break;
            case SkillUpgradeType.SwordThrow_Spin:
                currentThrowPower = spinThrowPower;
                break;
            case SkillUpgradeType.SwordThrow_Bounce:
                currentThrowPower = bounceThrowPower;
                break;
        }
    }

    private Vector2 GetThorwDir() => confirmedDirection * (currentThrowPower * 10);

    /// <summary>
    /// 显示预测轨迹点（蓄力瞄准时用）。
    /// </summary>
    public void PredictTrajectory(Vector2 direction)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].position = GetTrajectoryPoint(direction, i * spaceBetweenDots);
        }
    }

    private Vector2 GetTrajectoryPoint(Vector2 direction, float t)
    {
        float scaledThrowPower = currentThrowPower * 10;

        Vector2 initialVelocity = direction * scaledThrowPower;
        Vector2 gravityEffect = 0.5f * Physics2D.gravity * swordgravity * (t * t);
        Vector2 gredictedPoint = (initialVelocity * t) + gravityEffect;
        Vector2 playerPosition = transform.root.position;

        return playerPosition + gredictedPoint;
    }

    public void ConfirmTrajectory(Vector2 direction) => confirmedDirection = direction;

    public void EnableDots(bool enable)
    {
        foreach (Transform t in dots)
        {
            t.gameObject.SetActive(enable);
        }
    }

    private Transform[] GenerateDots()
    {
        Transform[] newDots = new Transform[numberOfDots];

        for (int i = 0; i < numberOfDots; i++)
        {
            newDots[i] = Instantiate(predictionDot, transform.position, Quaternion.identity, transform).transform;
            newDots[i].gameObject.SetActive(false);
        }

        return newDots;
    }
}
