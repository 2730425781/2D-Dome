using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家专属属性扩展：在 Entity_Stats 基础上增加"限时 Buff"系统。
/// Buff 本质是给 Stat 挂带来源标识的 modifier：
/// Stat.RemoveModifier(source) 会按来源一次性清掉，所以 Buff 必须记录 source 才能精确回收。
/// activeBuff 列表用于去重：同一来源的 Buff 未结束前不允许再次施加（CanApplyBuff 先检查），
/// 避免"重复使用同一个消耗品导致 modifier 无限叠加"。
/// 用协程而不是 Update 计时：Buff 生命周期是"等待固定时长后清理"的一次性任务，
/// 协程写法更贴合且不需要状态机字段。
/// </summary>
public class Player_Stats : Entity_Stats
{
    private Inventory_Player inventory;
    // 正在生效的 Buff 来源集合，用于防止同一来源重复叠加
    private List<string> activeBuff = new List<string>();

    protected override void Awake()
    {
        // 提前缓存引用：BuffCo 结束时需要通知背包刷新 UI，而 Awake 保证先于任何 Buff 调用执行
        inventory = GetComponent<Inventory_Player>();
    }

    public bool CanApplyBuff(string source)
    {
        // 去重守卫：同源 Buff 未结束时返回 false，防止 modifier 无限叠加
        return !activeBuff.Contains(source);
    }

    public void ApplyBuff(BuffEffectDate[] buffs, float duration, string source)
    {
        // 统一入口：把 Buff 效果转成交互协程，调用方无需关心计时与清理细节
        StartCoroutine(BuffCo(buffs, duration, source));
    }

    private IEnumerator BuffCo(BuffEffectDate[] buffs, float duration, string source)
    {
        // 先登记来源再挂 modifier，确保同帧内的重复调用也会被 CanApplyBuff 拦截
        activeBuff.Add(source);

        foreach (var buff in buffs)
        {
            // 每种属性挂一个带 source 的 modifier，到期后按 source 一并移除
            GetStatValueByType(buff.type).AddModifier(buff.value, source);
        }

        // 阻塞协程直到 Buff 时长结束
        yield return new WaitForSeconds(duration);

        foreach (var buff in buffs)
        {
            // 按 source 移除而不是逐个减 value——source 是唯一的清理键
            GetStatValueByType(buff.type).RemoveModifier(source);
        }

        // 通知背包/属性 UI 刷新，让玩家立刻看到 Buff 结束后的数值变化
        inventory.TriggerUpdateUI();
        // 最后释放来源，允许之后再次施加同类 Buff
        activeBuff.Remove(source);
    }
}
