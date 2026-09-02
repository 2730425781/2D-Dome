using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品列表", fileName = "物品列表设置-")]
public class ItemListDateSO : ScriptableObject
{
    public ItemDataSO[] itemList;

    public ItemDataSO GetItemData(string saveID)
    {
        return itemList.FirstOrDefault(item => item != null && item.saveID == saveID);
    }

#if UNITY_EDITOR
    [ContextMenu("自动填充物品数据")]
    public void CollectItemsData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSo");

        itemList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null)
            .ToArray();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif

}
