using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/对话数据/新对话者", fileName = "对话者-")]
public class DialogueSpeakerSO : ScriptableObject
{
    public string speakerName;
    public Sprite speakerPortrait;
}
