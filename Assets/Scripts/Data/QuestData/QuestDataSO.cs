#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// 任务奖励的领取方式/地点。
/// None = 不绑定 NPC，进度达标后自动发放。
/// 注意：枚举在 Unity 里按 int 序列化，所以把原来的拼写错误 Blackmisth 改成 Blacksmith
/// 不会影响已有资产（仍读作 1）。
/// </summary>
public enum RewardType
{
    Merchant,     // 找商人领取
    Blacksmith,   // 找铁匠领取
    None          // 无需找 NPC：达标即自动发放
}

/// <summary>
/// 单个任务的数据定义。
///
/// 为什么 questTargetID 用 string 而不是枚举：
/// 任务目标既可能是敌人（匹配 Enemy.questTargetID），也可能是 NPC（匹配 Object_NPC.npcTargetQuestID），
/// 后续还可能是收集物/地点。用 string 让"击杀/对话/其它"共用同一套目标标识，
/// 新增一种目标类型不需要改数据结构，也不需要重排枚举值。
///
/// 为什么同时保留 rewardType 与 autoGrantReward：
/// 绝大多数任务要回 NPC 领奖（rewardType 决定地点），但"新手引导类"任务
/// 希望一达标就发奖、不必多跑一趟。两个字段并存可覆盖两种流程。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/任务数据/新任务", fileName = "任务设置-")]
public class QuestDataSO : ScriptableObject
{
    public string questSaveID;
    [Space]
    public string questName;
    [TextArea] public string questGoal;
    [TextArea] public string description;

    [Header("任务目标")]
    // 与 Enemy.questTargetID 或 Object_NPC.npcTargetQuestID 对应：累计达成 requiredAmount 次即完成
    public string questTargetID;
    public int requiredAmount = 1;

    [Header("任务奖励设置")]
    // 领奖地点。rewardType == None 表示该任务不绑定 NPC
    public RewardType rewardType;
    // 勾选后无视 rewardType：进度达标立即自动发放奖励
    public bool autoGrantReward;
    public int goldReward;
    public Inventory_Item[] rewardItems;

    /// <summary>
    /// 达标后走"自动发放"还是"必须回 NPC 领取"。
    /// 两种模式都要支持：None 表示任务本就不绑定 NPC，走自动发放；
    /// autoGrantReward 则让绑定了 NPC 的任务也能直接发放。
    /// </summary>
    public bool GrantsAutomatically => autoGrantReward || rewardType == RewardType.None;

    /// <summary>奖励物品数组中的有效条目数（跳过空引用/缺数据的槽，供 UI 安全遍历）。</summary>
    public int GetSafeRewardCount()
    {
        if (rewardItems == null) return 0;

        int count = 0;
        for (int i = 0; i < rewardItems.Length; i++)
        {
            // 数据里可能残留空槽，直接跳过而不是让 UI 去判空
            if (rewardItems[i] == null || rewardItems[i].itemDate == null) continue;
            count++;
        }
        return count;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // 资产尚未落盘时 path 为空，此时写 ID 会把已有 ID 清成空串，
        // 导致任务存档对应关系丢失
        string path = AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            questSaveID = AssetDatabase.AssetPathToGUID(path);
        }
#endif
        // 数量至少为 1：配成 0 会让任务"一接取就完成"，属于明显笔误
        requiredAmount = Mathf.Max(1, requiredAmount);
    }
}
