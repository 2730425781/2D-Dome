using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 对话面板：逐字显示一句对话，玩家点击推进，并在本行配置了动作时（如打开商店）执行动作。
///
/// 点击的两段语义：
/// 1. 文字还在逐字显示 → 点击 = 立刻显示整句（跳过打字机效果），不执行动作
/// 2. 文字已显示完   → 点击 = 执行本行 DialogueActionType 配置的动作
/// </summary>
public class UI_Dialogue : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    private UI ui;

    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI[] dialogueChoicesText;

    [Space]
    [SerializeField] private float textSpeed = 0.1f;
    private string fullTextToShow;
    private Coroutine typeTextCo;
    private bool waitingToConfirm = false;

    private DialogueLineSO currentLine;
    private DialogueLineSO[] currentChoices;
    private DialogueLineSO selectedChoice;
    private int selectedChoiceIndex;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
    }

    public void PlayDialogueLine(DialogueLineSO line)
    {
        // 切到新一行前先给上一行收尾：否则上一行的打字协程还在跑，
        // 两个协程会往同一个文本里交错追加字符，显示成两份文本混在一起
        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            typeTextCo = null;
        }

        currentLine = line;
        currentChoices = line.choiceLines;
        waitingToConfirm = false;
        HideAllChoices();

        if (line == null)
        {
            if (dialogueText != null) dialogueText.text = "";
            return;
        }

        if (speakerPortrait != null && line.speaker != null)
            speakerPortrait.sprite = line.speaker.speakerPortrait;

        if (speakerName != null && line.speaker != null)
            speakerName.text = line.speaker.name;

        fullTextToShow = line.GetRandomLine();
        typeTextCo = StartCoroutine(TypeTextCo(fullTextToShow));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 文字还在逐字显示：第一下先把整句显示完，不执行动作
        if (typeTextCo != null)
        {
            CompleteTyping();
            return;
        }

        // 文字已显示完：再点一下执行本行配置的动作
        if (!waitingToConfirm)
        {
            waitingToConfirm = true;
            return;
        }

        HandleNextAction();
        waitingToConfirm = false;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {

    }


    public void NanigateChoice(int direction)
    {
        if (currentChoices == null || currentChoices.Length <= 1)
            return;

        selectedChoiceIndex += direction;
        selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex, 0, currentChoices.Length - 1);
        ShowChoices();
    }

    /// <summary>跳过打字机效果：立即显示整句，并进入"等待确认"状态。</summary>
    private void CompleteTyping()
    {
        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);

            // 关键：必须把协程句柄清空。
            // 原来的实现在这里只 StopCoroutine 而没有置 null，TypeTextCo 自然跑完后也不清，
            // 于是 typeTextCo 永远非 null，OnPointerDown 每次都走进"还在打字"的分支，
            // waitingToConfirm 永远为 false，HandleNextAction 永远执行不到——
            // 这正是"对话里选择打开商店却毫无反应"的根因
            typeTextCo = null;
        }

        if (dialogueText != null)
            dialogueText.text = fullTextToShow;

        waitingToConfirm = true;
    }

    private void ShowChoices()
    {
        for (int i = 0; i < dialogueChoicesText.Length; i++)
        {
            if (i < currentChoices.Length)
            {
                DialogueLineSO choice = currentChoices[i];
                string choiceText = choice.GetfirstLine();

                dialogueChoicesText[i].text = choice.GetfirstLine();
                dialogueChoicesText[i].gameObject.SetActive(true);
            }
            else
            {
                dialogueChoicesText[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideAllChoices()
    {
        foreach (var choiceText in dialogueChoicesText)
        {
            choiceText.gameObject.SetActive(false);
        }
    }

    private IEnumerator TypeTextCo(string text)
    {
        if (dialogueText != null) dialogueText.text = "";

        foreach (var letter in text)
        {
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        // 自然显示完毕同样要清句柄并进入等待确认，这样玩家点一下即可执行动作
        typeTextCo = null;
        waitingToConfirm = true;
    }

    private void HandleNextAction()
    {
        if (currentLine == null || ui == null) return;

        switch (currentLine.actionType)
        {
            case DialogueActionType.OpenShop:
                // 先回到游戏界面（会关掉所有面板，含对话面板本身），再打开商店
                ui.SwitchToGameUI();
                ui.OpenMerchantUI(true);
                break;

            case DialogueActionType.PlayerMakeChoice:
                if (selectedChoice == null)
                {
                    selectedChoiceIndex = 0;
                    ShowChoices();
                }
                break;

            case DialogueActionType.CloseDialogue:
                ui.CloseDialogueUI();
                break;

            case DialogueActionType.None:
                break;

            default:
                // 其余动作（OpenQuest / OpenCraft / GetQuestReward / PlayerMakeChoice）还需要
                // 对话之外的上下文（NPC 的任务列表、铁匠仓库等），尚未接入。
                // 打日志而不是静默什么都不做，避免以后又出现"点了没反应却查不出原因"
                Debug.LogWarning("对话动作尚未实现: " + currentLine.actionType);
                break;
        }
    }
}
