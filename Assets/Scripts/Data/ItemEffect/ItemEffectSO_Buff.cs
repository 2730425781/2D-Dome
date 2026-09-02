using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Buff 效果：给玩家属性添加持续一段时间的加成或减益。
/// 为什么需要 CanBeUsed 预检：相同来源的 Buff 不能叠加，
/// 必须先确认当前没有同名 Buff 生效再消耗物品，否则会出现“物品用掉却没加上效果”。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/Buff效果", fileName = "Buff效果设置-")]
public class ItemEffectSO_Buff : ItemEffectDateSO
{
    [SerializeField] private BuffEffectDate[] buffs;
    [SerializeField] private float duration;
    // 每个效果资源实例生成唯一 ID 作为 Buff 的“来源标识”。
    // 为什么用 GUID：不同物品（即使引用同一份效果资源）需要能各自生效，
    // 若用固定字符串，所有同类物品的 Buff 会互相顶替。
    private string source = "Buff - " + Guid.NewGuid();


    public override bool CanBeUsed(Player player)
    {
        if (player.stats.CanApplyBuff(source))
        {
            this.player = player;
            return true;
        }
        else
        {
            // 相同来源的 Buff 还在生效，直接拒绝使用并提示玩家
            Debug.Log("相同的Buff效果不能同时生效");
            return false;
        }
    }

    public override void ExecuteEffect()
    {
        // 用唯一 source 注册 Buff：到期后 Player_Stats 的协程会自动移除修正并刷新背包 UI
        player.stats.ApplyBuff(buffs, duration, source);
        player = null;
    }
}
