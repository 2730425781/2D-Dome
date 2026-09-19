using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/对话数据/对话设置", fileName = "对话设置-")]
public class DialogueLineSO : ScriptableObject
{
    [Header("对话信息")]
    public string dialogueGroupName;
    public DialogueSpeakerSO speaker;

    [Header("对话文本")]
    [TextArea] public string[] textLine;

    [Header("对话行为")]
    [TextArea] public string actionLine;
    public DialogueActionType actionType;
    public DialogueLineSO[] choiceLines;

    // [Header("回答设置")]
    // public bool playerCanAnswer;
    // public DialogueLineSO[] answerLine;

    public string GetfirstLine() => textLine[0];

    public string GetRandomLine()
    {
        // 文本数组为空时 Random.Range(0, 0) 会越界，返回空串让调用方安全显示空对话
        if (textLine == null || textLine.Length == 0) return "";

        return textLine[Random.Range(0, textLine.Length)];
    }
}
