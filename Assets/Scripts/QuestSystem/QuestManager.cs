using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>任务在 UI 上的状态。</summary>
public enum QuestState
{
    NotAccepted,    // 未接取：可接受
    InProgress,     // 进行中：显示进度
    ReadyToClaim,   // 已达标、奖励尚未发放
    Claimed         // 奖励已发放（含 NPC 领取与自动发放）
}

/// <summary>
/// 任务管理器：任务状态的唯一权威来源（接取 / 进度 / 完成 / 发奖 / 存档）。
///
/// 为什么用"持久化单例 + 自举"而不是像 UI 那样挂在某个场景：
/// 任务进度天然跨场景（在 Level_0 接的任务，进度要在 Level_1 的击杀里继续累计），
/// 所以必须 DontDestroyOnLoad；而本项目有多个关卡场景，手动挂载漏一个关卡就会出现
/// "这一关击杀不计数、存档也收集不到任务进度"。用 RuntimeInitializeOnLoadMethod 自举
/// 能保证在任何场景启动时、且在 SaveManager.Start 收集 ISaveable 之前实例就已存在。
///
/// 为什么进度用"事件上报"而不是每帧扫描敌人：
/// 击杀与对话都是离散事件，在发生点调用 RegisterProgress 一次即可，
/// 不必每帧遍历场景中的敌人做 ID 匹配。
/// </summary>
public class QuestManager : MonoBehaviour, ISaveable
{
    public static QuestManager instance;

    /// <summary>任务状态发生变化（接取 / 进度 / 发奖）时触发，供 UI 刷新。</summary>
    public event Action OnQuestChange;

    [Tooltip("可选：任务数据库。留空时会自动从场景中的任务发布者(NPC)反查任务资产，不影响功能")]
    [SerializeField] private QuestDataBaseSO questDatabase;

    // ---------- 运行时状态（由存档恢复）----------
    private readonly HashSet<string> activeQuests = new HashSet<string>();
    private readonly Dictionary<string, int> questProgress = new Dictionary<string, int>();
    private readonly HashSet<string> claimedQuests = new HashSet<string>();

    // questSaveID -> 任务资产 缓存，避免每次都去数据库 / NPC 里线性查找
    private readonly Dictionary<string, QuestDataSO> resolvedQuests = new Dictionary<string, QuestDataSO>();

    /// <summary>
    /// 自举：保证任务管理器在任何关卡场景都存在。
    /// instance 可能残留上一轮播放中已销毁的对象（本项目关闭了域重载），
    /// Unity 重载过的 == 能把"已销毁"判成 null，正好用它做存活判断。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        var go = new GameObject("QuestManager (Runtime)");
        go.AddComponent<QuestManager>();
    }

    private void Awake()
    {
        // 单例 + 跨场景常驻；重复实例直接销毁，避免同一份任务状态被写两份存档
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // ---------- 查询 ----------

    public QuestState GetState(QuestDataSO quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questSaveID))
            return QuestState.NotAccepted;

        string id = quest.questSaveID;
        if (claimedQuests.Contains(id))
            return QuestState.Claimed;

        if (!activeQuests.Contains(id))
            return QuestState.NotAccepted;

        return GetProgress(quest) >= RequiredOf(quest) ? QuestState.ReadyToClaim : QuestState.InProgress;
    }

    public int GetProgress(QuestDataSO quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questSaveID))
            return 0;

        return questProgress.TryGetValue(quest.questSaveID, out int value) ? value : 0;
    }

    public bool IsActive(QuestDataSO quest) => quest != null && activeQuests.Contains(quest.questSaveID);

    public bool IsClaimed(QuestDataSO quest) => quest != null && claimedQuests.Contains(quest.questSaveID);

    private static int RequiredOf(QuestDataSO quest) => Mathf.Max(1, quest.requiredAmount);

    // ---------- 接取 ----------

    /// <summary>接取任务。已接取或已领奖的任务会被拒绝（返回 false）。</summary>
    public bool AcceptQuest(QuestDataSO quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questSaveID))
            return false;

        if (activeQuests.Contains(quest.questSaveID) || claimedQuests.Contains(quest.questSaveID))
            return false;

        activeQuests.Add(quest.questSaveID);

        if (!questProgress.ContainsKey(quest.questSaveID))
            questProgress[quest.questSaveID] = 0;

        resolvedQuests[quest.questSaveID] = quest;

        // 目标数量配置为 0 之类的异常数据在这里被兜住：接取时先判定一次是否已达标
        TryAutoGrant(quest);

        OnQuestChange?.Invoke();
        return true;
    }

    // ---------- 进度 ----------

    /// <summary>
    /// 上报一次目标事件（击杀敌人 / 与 NPC 对话 / 后续其它类型）。
    /// targetID 与已接取任务的 questTargetID 比对，命中则累加进度。
    /// </summary>
    public void RegisterProgress(string targetID, int amount = 1)
    {
        if (string.IsNullOrEmpty(targetID) || amount <= 0) return;

        // 先拷贝一份 ID 再遍历：TryAutoGrant 可能改动集合，直接遍历会抛"集合被修改"
        var ids = new List<string>(activeQuests);
        bool changed = false;

        foreach (string id in ids)
        {
            var quest = ResolveQuest(id);
            if (quest == null || quest.questTargetID != targetID) continue;

            int required = RequiredOf(quest);
            int current = questProgress.TryGetValue(id, out int value) ? value : 0;
            if (current >= required) continue;       // 已达标，不再累加

            questProgress[id] = Mathf.Min(current + amount, required);
            changed = true;

            TryAutoGrant(quest);
        }

        if (changed) OnQuestChange?.Invoke();
    }

    // ---------- 发奖 ----------

    /// <summary>
    /// 领取奖励（"回 NPC 领取"的流程）。只有已达标且尚未发放过才生效。
    /// 自动发放的任务在达标时已发放，这里会因 claimedQuests 命中而被挡下。
    /// </summary>
    public bool ClaimReward(QuestDataSO quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questSaveID)) return false;
        if (claimedQuests.Contains(quest.questSaveID)) return false;
        if (!activeQuests.Contains(quest.questSaveID)) return false;
        if (GetProgress(quest) < RequiredOf(quest)) return false;
        if (!GrantRewards(quest)) return false;

        OnQuestChange?.Invoke();
        return true;
    }

    /// <summary>达标且配置为自动发放时，立即发奖。</summary>
    private void TryAutoGrant(QuestDataSO quest)
    {
        if (quest == null || !quest.GrantsAutomatically) return;
        if (claimedQuests.Contains(quest.questSaveID)) return;
        if (GetProgress(quest) < RequiredOf(quest)) return;

        GrantRewards(quest);
    }

    /// <summary>
    /// 实际发放奖励。返回是否发放成功。
    /// 找不到玩家背包（例如在主菜单触发）时返回 false 且不标记已领取，
    /// 这样任务会停留在"可领取"，玩家进关卡后仍能通过按钮拿到奖励，不会凭空丢失。
    /// </summary>
    private bool GrantRewards(QuestDataSO quest)
    {
        var inventory = Player.instance != null ? Player.instance.playerInventory : FindAnyObjectByType<Inventory_Player>();
        if (inventory == null)
        {
            Debug.LogWarning("任务奖励发放失败：场景中没有玩家背包。任务 " + quest.questName);
            return false;
        }

        claimedQuests.Add(quest.questSaveID);

        if (quest.goldReward != 0)
        {
            inventory.gold += quest.goldReward;
        }

        if (quest.rewardItems != null)
        {
            foreach (var template in quest.rewardItems)
            {
                if (template == null || template.itemDate == null) continue;

                // 资产里的奖励物品只是模板，必须 new 一个新实例再入包：
                // 直接加入会把模板实例塞进背包，多个任务引用同一资产时会共享堆叠数
                inventory.AddItem(new Inventory_Item(template.itemDate) { stackSize = Mathf.Max(1, template.stackSize) });
            }
        }

        inventory.TriggerUpdateUI();
        return true;
    }

    // ---------- 任务资产反查 ----------

    private QuestDataSO ResolveQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return null;

        // 缓存可能存着已被卸载的资产，Unity 的 == 会把它判成 null，此时重新解析
        if (resolvedQuests.TryGetValue(questID, out var cached) && cached != null) return cached;

        QuestDataSO found = questDatabase != null ? questDatabase.GetQuestByID(questID) : null;

        if (found == null)
        {
            // 兜底：没配数据库就从场景里所有任务发布者(NPC)身上找。
            // 这样即使忘了在 Inspector 里挂数据库，读档还原进度与自动发奖依然可用。
            foreach (var npc in FindObjectsByType<Object_NPC>(FindObjectsInactive.Include))
            {
                var quests = npc.Quests;
                if (quests == null) continue;

                foreach (var quest in quests)
                {
                    if (quest != null && quest.questSaveID == questID) { found = quest; break; }
                }
                if (found != null) break;
            }
        }

        if (found != null) resolvedQuests[questID] = found;
        return found;
    }

    // ---------- 存档 ----------

    public void SaveData(ref GameData data)
    {
        data.activeQuests.Clear();
        data.questProgress.Clear();
        data.claimedQuests.Clear();

        foreach (string id in activeQuests) data.activeQuests[id] = true;
        foreach (var pair in questProgress) data.questProgress[pair.Key] = pair.Value;
        foreach (string id in claimedQuests) data.claimedQuests[id] = true;
    }

    public void LoadData(GameData data)
    {
        activeQuests.Clear();
        questProgress.Clear();
        claimedQuests.Clear();
        resolvedQuests.Clear();

        if (data.activeQuests != null)
        {
            foreach (var pair in data.activeQuests)
            {
                if (pair.Value) activeQuests.Add(pair.Key);
            }
        }

        if (data.questProgress != null)
        {
            foreach (var pair in data.questProgress) questProgress[pair.Key] = pair.Value;
        }

        if (data.claimedQuests != null)
        {
            foreach (var pair in data.claimedQuests)
            {
                if (pair.Value) claimedQuests.Add(pair.Key);
            }
        }

        OnQuestChange?.Invoke();
    }
}
