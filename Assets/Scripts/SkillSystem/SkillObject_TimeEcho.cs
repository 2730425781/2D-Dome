using UnityEngine;

/// <summary>
/// Time Echo 实体：玩家克隆体，能攻击、承伤，死后可能变成治疗精灵飞回玩家。
/// </summary>
public class SkillObject_TimeEcho : SkillObject_Base
{
    [SerializeField] private float wispSpeed = 15;
    [SerializeField] private GameObject onDeathVFX;
    [SerializeField] private LayerMask groundLayer;

    private bool shouldMoveToPlayer;
    private Transform palyerTransform;
    private Skill_TimeEcho echoManager;
    private TrailRenderer wispTrail;
    private Entity_Health palyerHealth;
    private SkillObject_Health echoHealth;
    private Player_SkillManager skillManager;
    private Entity_StatusHandler statusHandler;

    public int maxAttacks { get; private set; }

    public void SetUpEcho(Skill_TimeEcho echoManager)
    {
        this.echoManager = echoManager;
        playerStats = echoManager.player.stats;
        damageScaleDate = echoManager.damageScaleDate;
        maxAttacks = echoManager.GetMaxAttacks();
        palyerTransform = echoManager.transform.root;
        palyerHealth = echoManager.player.health;
        skillManager = echoManager.skillManager;
        statusHandler = echoManager.player.statusHandler;
        echoHealth = GetComponent<SkillObject_Health>();

        // 超时后自动消失（并触发 Wisp 等后续逻辑）
        Invoke(nameof(HandleDeath), echoManager.GetEchoDuration());
        FlipToTarget();

        wispTrail = GetComponentInChildren<TrailRenderer>();
        // 拖尾只在变成精灵时显示，平时关闭避免视觉干扰
        wispTrail.gameObject.SetActive(false);

        animator.SetBool("canAttack", maxAttacks > 0);
    }

    private void Update()
    {
        if (shouldMoveToPlayer)
        {
            HandleWispMovement();
        }
        else
        {
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
            StopHorizontalMovement();
        }
    }

    private void HandleWispMovement()
    {
        // 精灵直接飞向玩家，不用物理：保证一定能回到玩家身边
        transform.position = Vector2.MoveTowards(transform.position, palyerTransform.position, wispSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, palyerTransform.position) < 0.5f)
        {
            HandlePlayerTouch();
            Destroy(gameObject);
        }
    }

    private void HandlePlayerTouch()
    {
        // 回血 = 克隆体承受的伤害 * 治疗百分比
        float healAmount = echoHealth.lastDamageTaken * echoManager.GetPercentOfDamageHealed();
        palyerHealth.IncreaseHealth(healAmount);

        // 减少所有技能冷却
        float cooldoenToReduce = echoManager.GetCooldownReduceInSeconds();
        skillManager.ReduceAllSkillCooldown(cooldoenToReduce);

        // 净化负面效果
        if (echoManager.CanRemoveNegativeEffects())
        {
            statusHandler.RemoveAllNegetiveEffect();
        }
    }

    private void FlipToTarget()
    {
        Transform target = FindClosestTarget();

        if (target == null) return;

        if (target.position.x < transform.position.x)
        {
            transform.Rotate(0, 180, 0);
        }
    }

    public void PerformAttack()
    {
        DamageEnemiesInRadius(targetCheck, 1, true);

        if (!targetGoHit)
        {
            return;
        }

        // 命中后有几率在目标位置复制一个新克隆体
        bool canDuplicate = Random.value <= echoManager.GetDuplicateChance();
        float xoffset = transform.position.x < lastTarget.position.x ? 1 : -1;

        if (canDuplicate)
        {
            echoManager.CreateTimeEcho(lastTarget.position + new Vector3(xoffset, 0));
        }
    }

    public void HandleDeath()
    {
        Instantiate(onDeathVFX, transform.position, Quaternion.identity);

        // 根据升级分支决定死亡后是否变成精灵飞回玩家
        if (echoManager.ShouldBeWisp())
        {
            TurnIntoWisp();
        }
        else
            Destroy(gameObject);
    }

    private void TurnIntoWisp()
    {
        shouldMoveToPlayer = true;
        animator.gameObject.SetActive(false);
        wispTrail.gameObject.SetActive(true);
        rb.simulated = false;
    }

    private void StopHorizontalMovement()
    {
        // 落地后让克隆体停住（站在地面上），而不是继续被物理推动
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, groundLayer);

        if (hit.collider != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}
