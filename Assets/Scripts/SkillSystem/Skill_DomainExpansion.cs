using System.Collections.Generic;
using UnityEngine;

public class Skill_DomainExpansion : Skill_Base
{
    [SerializeField] private GameObject domainPrefab;
    [Header("领域设置")]
    public float maxDomainSize = 10;
    public float expandSpeed = 3;
    [Header("减速领域升级")]
    [SerializeField] private float slowDownPercent = 0.8f;
    [SerializeField] private float slowDownDuration = 5;
    [Header("碎片法术领域升级")]
    [SerializeField] private int shardToCast = 10;
    [SerializeField] private float shardCastingSlowDown = 1;
    [SerializeField] private float shardCastingDuration = 5;
    [Header("回响法术领域升级")]
    [SerializeField] private int echoToCast = 8;
    [SerializeField] private float echoCastingSlowDown = 1;
    [SerializeField] private float echoCastingDuration = 6;
    //[SerializeField] private float healthToRestoreWithEcho = 0.05f;
    private float spellCastTimer;
    private float spellPerSecond;

    private List<Enemy> trappedTarget = new List<Enemy>();
    private Transform currentTarget;

    public void CreateDomain()
    {
        spellPerSecond = GetSpellsToCast() / GetDomainDuration();

        GameObject doamin = Instantiate(domainPrefab, transform.position, Quaternion.identity);
        doamin.GetComponent<SkillObject_DomainExpansion>().SetUpDomain(this);
    }

    public void DoSpellCasting()
    {
        spellCastTimer -= Time.deltaTime;

        if (currentTarget == null)
        {
            currentTarget = FindTargetDomain();
        }

        if (currentTarget != null && spellCastTimer <= 0)
        {
            CastSpell(currentTarget);
            spellCastTimer = 1 / spellPerSecond;
            currentTarget = null;
        }
    }

    private void CastSpell(Transform target)
    {
        if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            Vector3 offset = Random.value < 0.5f ? new Vector2(1, 0) : new Vector2(-1, 0);
            skillManager.timeEcho.CreateTimeEcho(target.position + offset);
        }
        if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            skillManager.shard.CreateRawShard(target, true);
        }
    }

    private Transform FindTargetDomain()
    {
        trappedTarget.RemoveAll(target => target == null || target.health.isDead);

        if (trappedTarget.Count == 0) return null;

        int index = Random.Range(0, trappedTarget.Count);
        return trappedTarget[index].transform;
    }

    public bool InstantDomain()
    {
        return upgradeType != SkillUpgradeType.Domain_EchoSpam
            && upgradeType != SkillUpgradeType.Domain_ShardSpam;
    }

    public float GetDomainDuration()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
        {
            return slowDownDuration;
        }
        else if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            return shardCastingDuration;
        }
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            return echoCastingDuration;
        }
        return 0;
    }

    public float GetSlowPercent()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
        {
            return slowDownPercent;
        }
        else if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            return shardCastingSlowDown;
        }
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            return echoCastingSlowDown;
        }
        return 0;
    }

    private int GetSpellsToCast()
    {
        if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            return shardToCast;
        }
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            return echoToCast;
        }
        return 0;
    }

    public void AddTarget(Enemy target)
    {
        trappedTarget.Add(target);
    }

    public void ClearTargets()
    {
        foreach (var enemy in trappedTarget)
        {
            enemy.StopSlowDown();
        }
        trappedTarget = new List<Enemy>();
    }
}
