using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 右侧任务详情面板：任务名 / 描述 / 目标 / 奖励物品，以及底部的"接受 / 领取奖励"按钮。
///
/// 按钮引用允许留空：Awake 会自己在子物体里找 UI_QuestButton（场景里已存在该按钮），
/// 这样新增的功能不必依赖手工拖引用，也避免漏配就整个面板不可用。
/// </summary>
public class UI_QuestPreviw : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextMeshProUGUI questDescription;
    [SerializeField] private TextMeshProUGUI questGoal;
    [SerializeField] private UI_QuestRewardSlot[] questReward;
    [SerializeField] private GameObject[] additionalObjects;

    [Header("操作按钮（留空自动在子物体中查找）")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;

    // 当前预览的任务与它所属的面板：按钮点击时把动作交回 UI_Quest，避免这里重复一份任务状态逻辑
    private UI_Quest owner;
    private QuestDataSO quest;

    private void Awake()
    {
        if (actionButton == null)
        {
            Transform buttonTransform = transform.Find("UI_QuestButton");
            if (buttonTransform != null) actionButton = buttonTransform.GetComponent<Button>();
            if (actionButton == null) actionButton = GetComponentInChildren<Button>(true);
        }

        if (actionButtonText == null && actionButton != null)
        {
            actionButtonText = actionButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (actionButton != null) actionButton.onClick.AddListener(OnActionButtonClicked);
    }

    private void OnActionButtonClicked()
    {
        if (owner != null) owner.OnActionButtonPressed();
    }

    public void SetupQuestPreviw(QuestDataSO previewQuest, UI_Quest questUI)
    {
        owner = questUI;
        quest = previewQuest;

        // 没有选中任何任务时清空面板，而不是留着上一个任务的文案
        if (previewQuest == null)
        {
            CleanUpQuestPreviw();
            return;
        }

        EnableAdditionalObject(true);
        EnableQuestRewardObjects(false);

        if (questName != null) questName.text = previewQuest.questName;
        if (questDescription != null) questDescription.text = previewQuest.description;
        if (questGoal != null) questGoal.text = previewQuest.questGoal;

        ShowRewards(previewQuest);
        RefreshActionButton(previewQuest);
    }

    /// <summary>按任务状态决定按钮文案与可点击性，让一个按钮承担"接受/领取"两种动作。</summary>
    private void RefreshActionButton(QuestDataSO previewQuest)
    {
        if (actionButton == null) return;

        var manager = QuestManager.instance;
        QuestState state = manager != null ? manager.GetState(previewQuest) : QuestState.NotAccepted;

        string label;
        bool interactable;

        switch (state)
        {
            case QuestState.NotAccepted:
                label = "接受任务";
                interactable = true;
                break;
            case QuestState.InProgress:
                label = "进行中 " + manager.GetProgress(previewQuest) + "/" + Mathf.Max(1, previewQuest.requiredAmount);
                interactable = false;
                break;
            case QuestState.ReadyToClaim:
                // 自动发放类任务若因当时没有背包而未能发放，会停留在可领取，
                // 此时按钮兜底让玩家手动领取，奖励不会丢
                label = previewQuest.GrantsAutomatically ? "领取奖励" : "完成任务";
                interactable = true;
                break;
            default:
                label = "已完成";
                interactable = false;
                break;
        }

        if (actionButtonText != null) actionButtonText.text = label;
        actionButton.interactable = interactable;

        // 不可点击时把按钮底图淡出，给出明确的视觉反馈
        var target = actionButton.targetGraphic;
        if (target != null)
        {
            Color color = target.color;
            color.a = interactable ? 1f : 0.5f;
            target.color = color;
        }
    }

    /// <summary>
    /// 显示奖励物品。
    /// 原来按 questDataSO.rewardItems.Length 直接索引 questReward[i]，
    /// 奖励条目多于奖励槽位（4 个）时会越界；这里改为受槽位数约束的安全遍历。
    /// </summary>
    private void ShowRewards(QuestDataSO previewQuest)
    {
        if (questReward == null || previewQuest.rewardItems == null) return;

        int slotIndex = 0;
        foreach (var reward in previewQuest.rewardItems)
        {
            if (reward == null || reward.itemDate == null) continue;
            if (slotIndex >= questReward.Length) break;

            UI_QuestRewardSlot slot = questReward[slotIndex];
            slotIndex++;
            if (slot == null) continue;

            slot.gameObject.SetActive(true);
            slot.UpdateSlot(reward);
        }
    }

    /// <summary>清空预览（没有选中任务时调用）。原来这个方法定义了却从未被调用。</summary>
    public void CleanUpQuestPreviw()
    {
        if (questName != null) questName.text = "";
        if (questDescription != null) questDescription.text = "";
        if (questGoal != null) questGoal.text = "";

        EnableAdditionalObject(false);
        EnableQuestRewardObjects(false);
    }

    private void EnableAdditionalObject(bool enable)
    {
        if (additionalObjects == null) return;

        foreach (var obj in additionalObjects)
        {
            if (obj != null) obj.SetActive(enable);
        }
    }

    private void EnableQuestRewardObjects(bool enable)
    {
        if (questReward == null) return;

        foreach (var obj in questReward)
        {
            if (obj != null) obj.gameObject.SetActive(enable);
        }
    }
}
