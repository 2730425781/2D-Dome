using System;
using UnityEngine;

/// <summary>
/// 任务面板：左侧是"当前 NPC 提供的任务列表"，右侧是选中任务的详情与操作按钮，底部是玩家背包。
///
/// 为什么面板自己不做任务状态判断：
/// 状态（是否已接取/进度/是否已领奖）的唯一权威来源是 QuestManager，
/// 这里只负责把状态渲染出来并把玩家操作转发过去，避免两处各存一份状态而不一致。
/// </summary>
public class UI_Quest : MonoBehaviour
{
    [SerializeField] private UI_ItemSlotParent inventorySlots;
    [SerializeField] private UI_QuestPreviw questPreviw;

    private UI_QuestSlot[] questSlots;
    // 当前 NPC 发布的任务；列表可能比场景里的槽位少，也可能更多
    private QuestDataSO[] currentQuests;
    private QuestDataSO selectedQuest;

    private void Awake()
    {
        // true：面板初始是隐藏的，不带上未激活子物体就会拿不到槽位引用
        questSlots = GetComponentsInChildren<UI_QuestSlot>(true);
        if (questPreviw == null) questPreviw = GetComponentInChildren<UI_QuestPreviw>(true);
    }

    private void OnEnable()
    {
        // 订阅任务变化：击杀达标后即使面板开着也能立刻刷新"进行中 x/N → 可领取"
        if (QuestManager.instance != null) QuestManager.instance.OnQuestChange += RefreshAll;
    }

    private void OnDisable()
    {
        if (QuestManager.instance != null) QuestManager.instance.OnQuestChange -= RefreshAll;
    }

    /// <summary>由 UI.OpenQuestUI 调用，传入当前交互 NPC 提供的任务列表。</summary>
    public void SetupQuestUI(QuestDataSO[] questArray)
    {
        currentQuests = questArray ?? Array.Empty<QuestDataSO>();

        // 按"实际任务数"显隐槽位。
        // 原实现无条件 questSlots[i].SetQuestSlot(questArray[i])，
        // 任务数少于槽位数时直接数组越界，且多余槽位会残留旧内容。
        for (int i = 0; i < questSlots.Length; i++)
        {
            bool hasQuest = i < currentQuests.Length && currentQuests[i] != null;
            questSlots[i].gameObject.SetActive(hasQuest);

            if (hasQuest) questSlots[i].Setup(this, currentQuests[i]);
            else questSlots[i].SetQuestSlot(null);
        }

        RefreshInventory();

        // 默认预览第一个任务，避免打开面板右侧一片空白
        SelectQuest(currentQuests.Length > 0 ? currentQuests[0] : null);
    }

    /// <summary>点击左侧任务条目：在右侧显示该任务详情。</summary>
    public void SelectQuest(QuestDataSO quest)
    {
        selectedQuest = quest;
        if (questPreviw != null) questPreviw.SetupQuestPreviw(quest, this);
    }

    /// <summary>
    /// 右侧按钮按下：按当前状态执行"接受任务"或"领取奖励"。
    /// 按钮文案已由 UI_QuestPreviw 按状态生成，这里只做动作分发。
    /// </summary>
    public void OnActionButtonPressed()
    {
        var manager = QuestManager.instance;
        if (selectedQuest == null || manager == null) return;

        switch (manager.GetState(selectedQuest))
        {
            case QuestState.NotAccepted:
                manager.AcceptQuest(selectedQuest);
                break;
            case QuestState.ReadyToClaim:
                manager.ClaimReward(selectedQuest);
                break;
            default:
                return;     // 进行中 / 已完成：无可执行动作
        }

        RefreshAll();
    }

    /// <summary>
    /// 任务状态变化时统一刷新左右两侧。
    /// 只刷左侧列表是不够的：右侧按钮的文案与可点击性同样由任务状态决定，
    /// 进度达标后若不刷新，按钮会一直停在"进行中 x/N"而点不到"领取奖励"。
    /// </summary>
    private void RefreshAll()
    {
        RefreshSlots();
        RefreshInventory();

        if (questPreviw != null && selectedQuest != null)
        {
            questPreviw.SetupQuestPreviw(selectedQuest, this);
        }
    }

    /// <summary>按最新任务状态重画左侧列表文案。</summary>
    private void RefreshSlots()
    {
        if (questSlots == null || currentQuests == null) return;

        for (int i = 0; i < questSlots.Length; i++)
        {
            if (questSlots[i] == null || !questSlots[i].gameObject.activeSelf) continue;
            questSlots[i].SetQuestSlot(i < currentQuests.Length ? currentQuests[i] : null);
        }
    }

    private void RefreshInventory()
    {
        if (inventorySlots == null) return;

        // 领奖会往背包里塞物品，这里同步刷新底部背包格
        var inventory = Player.instance != null ? Player.instance.playerInventory : null;
        if (inventory != null) inventorySlots.UpdateSlots(inventory.itemList);
    }
}
