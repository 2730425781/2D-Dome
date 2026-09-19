using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任务列表里的单个任务条目：显示任务名 / 状态 / 奖励预览图标，点击后在右侧显示详情。
///
/// 为什么用普通 Button 而不是复用 UI_ItemSlot：
/// 任务条目不是物品槽，没有拖拽/堆叠/悬停物品提示的需求；
/// 继承物品槽会平白引入 OnDrag/OnDrop 流程，反而要额外屏蔽。
/// </summary>
public class UI_QuestSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private Image[] rewardPreviw;
    // 可选的独立状态文本；场景里没配时会退化成把状态拼在任务名后面（见 SetQuestSlot）
    [SerializeField] private TextMeshProUGUI questStateText;
    [SerializeField] private Button button;

    private QuestDataSO questInSlot;
    private UI_Quest owner;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OnClicked);
    }

    /// <summary>由 UI_Quest 填充列表时调用，把"点击后该通知谁"的引用传进来。</summary>
    public void Setup(UI_Quest questUI, QuestDataSO quest)
    {
        owner = questUI;
        SetQuestSlot(quest);
    }

    private void OnClicked()
    {
        if (owner != null) owner.SelectQuest(questInSlot);
    }

    public void SetQuestSlot(QuestDataSO questDataSO)
    {
        questInSlot = questDataSO;

        // 未被使用的槽位要清干净，否则会残留上一次打开面板时的任务名/图标
        if (questDataSO == null)
        {
            if (questName != null) questName.text = "";
            if (questStateText != null) questStateText.text = "";
            HideAllRewardIcons();
            return;
        }

        string stateLabel = BuildStateLabel(questDataSO);

        if (questName != null)
        {
            // 没有独立状态文本时，把状态附在任务名后，保证玩家在任何情况下都看得到进度
            questName.text = string.IsNullOrEmpty(stateLabel)
                ? questDataSO.questName
                : questDataSO.questName + "（" + stateLabel + "）";
        }

        if (questStateText != null) questStateText.text = stateLabel;

        ShowRewardIcons(questDataSO);
    }

    /// <summary>把任务状态转成玩家能直接读懂的文案。</summary>
    private static string BuildStateLabel(QuestDataSO quest)
    {
        var manager = QuestManager.instance;
        if (manager == null) return "";

        switch (manager.GetState(quest))
        {
            case QuestState.NotAccepted:
                return "可接取";
            case QuestState.InProgress:
                return "进行中 " + manager.GetProgress(quest) + "/" + Mathf.Max(1, quest.requiredAmount);
            case QuestState.ReadyToClaim:
                return "可领取";
            case QuestState.Claimed:
                return "已完成";
            default:
                return "";
        }
    }

    private void HideAllRewardIcons()
    {
        if (rewardPreviw == null) return;

        foreach (var icon in rewardPreviw)
        {
            if (icon != null) icon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示奖励预览图标。
    /// 原来直接按 questDataSO.rewardItems.Length 索引 rewardPreviw[i]，
    /// 奖励条目多于图标槽位（4 个）时会数组越界；且空条目会走进去空引用。
    /// 这里改为：跳过空条目、并在图标槽位用尽时停止。
    /// </summary>
    private void ShowRewardIcons(QuestDataSO quest)
    {
        HideAllRewardIcons();

        if (rewardPreviw == null || quest.rewardItems == null) return;

        int iconIndex = 0;
        foreach (var reward in quest.rewardItems)
        {
            if (reward == null || reward.itemDate == null) continue;
            if (iconIndex >= rewardPreviw.Length) break;

            Image slot = rewardPreviw[iconIndex];
            iconIndex++;
            if (slot == null) continue;

            slot.gameObject.SetActive(true);
            slot.sprite = reward.itemDate.itemIcon;

            // 数量文本是子物体且可能没配，必须判空
            var amountText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (amountText != null) amountText.text = reward.stackSize.ToString();
        }
    }
}
