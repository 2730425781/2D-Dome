using System;
using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/Buff效果", fileName = "Buff效果设置-")]
public class ItemEffectSO_Buff : ItemEffectDateSO
{
    [SerializeField] private BuffEffectDate[] buffs;
    [SerializeField] private float duration;
    [SerializeField] private string source = Guid.NewGuid().ToString();

    Player_Stats playerStats;

    public override bool CanBeUsed()
    {
        if (playerStats == null)
        {
            playerStats = FindAnyObjectByType<Player_Stats>();
        }

        if (playerStats.CanApplyBuff(source))
        {
            return true;
        }
        else
        {
            Debug.Log("相同的Buff效果不能同时生效");
            return false;
        }
    }

    public override void ExecuteEffect()
    {
        playerStats.ApplyBuff(buffs, duration, source);
    }
}
