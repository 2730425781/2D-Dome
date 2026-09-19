#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using UnityEngine;

/// <summary>
/// 任务数据库：集中持有所有任务资产，供运行时按 questSaveID 反查。
///
/// 为什么需要一个中央数据库：任务 ID 是"资产 GUID"这种与项目结构绑定的字符串，
/// 运行时（例如读档还原任务进度）无法凭 ID 直接拿到 QuestDataSO，
/// 必须有一份 ID → 资产的索引表。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/任务数据/任务数据库", fileName = "任务数据库")]
public class QuestDataBaseSO : ScriptableObject
{
    public QuestDataSO[] allQuest;

    public QuestDataSO GetQuestByID(string id)
    {
        if (string.IsNullOrEmpty(id) || allQuest == null) return null;
        return allQuest.FirstOrDefault(q => q != null && q.questSaveID == id);
    }

#if UNITY_EDITOR
    [ContextMenu("自动填充任务数据")]
    public void CollectQuestData()
    {
        // 必须按 QuestDataSO 类型过滤。原来误写成 "t:ItemDataSo"，
        // 搜出来的是物品资产，LoadAssetAtPath<QuestDataSO> 全部为 null，
        // 于是 allQuest 被填成空数组——数据库一直是空的根因。
        string[] guids = AssetDatabase.FindAssets("t:QuestDataSO");

        allQuest = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<QuestDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(quest => quest != null)
            .ToArray();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
