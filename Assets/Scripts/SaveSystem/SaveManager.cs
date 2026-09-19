using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 存档管理器：支持多个存档槽位（文件）。
/// 每个槽位对应独立文件 &lt;fileName&gt;_&lt;slot&gt;.json（如 YS_1.json / YS_2.json），
/// 这样可以在不同存档之间切换/创建，不互相覆盖。
/// 内存中始终只有一份 gameData，代表"当前槽位"的数据；切换槽位即加载/保存到对应文件。
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private FileDataHandler dataHandler;
    private GameData gameData;
    private List<ISaveable> allSaveable;

    [SerializeField] private bool encryptData = true;
    [SerializeField] private int slotCount = 3;
    [SerializeField] private string fileName = "YS";

    /// <summary>当前操作的存档槽位（从 1 开始）。</summary>
    public int CurrentSlot { get; private set; } = 1;
    public int SlotCount => slotCount;

    private void Awake()
    {
        instance = this;
    }

    private IEnumerator Start()
    {
        //Debug.Log(Application.persistentDataPath);
        allSaveable = FindISaveables();

        yield return null;
        LoadGame(1);   // 默认加载 1 号槽
    }

    // 为指定槽位构造独立的文件处理器：每个槽位对应一个独立文件，互不覆盖
    private FileDataHandler GetDataHandler(int slot)
    {
        string slotFile = $"{GetBaseFileName()}_{slot}.json";
        return new FileDataHandler(Application.persistentDataPath, slotFile, encryptData);
    }

    // 规范化基础文件名：去掉可能残留的 .json 后缀，避免出现 "YS.json_1.json" 这种双扩展名
    private string GetBaseFileName()
    {
        return fileName.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase)
            ? fileName.Substring(0, fileName.Length - 5)
            : fileName;
    }

    public void SaveGame(int slot)
    {
        if (gameData == null) gameData = new GameData();

        foreach (var saveable in allSaveable)
        {
            saveable.SaveData(ref gameData);
        }

        GetDataHandler(slot).SaveData(gameData);
        CurrentSlot = slot;
    }

    // 兼容旧调用：保存到当前槽位
    public void SaveGame() => SaveGame(CurrentSlot);

    public void LoadGame(int slot)
    {
        GameData loaded = GetDataHandler(slot).LoadData();
        CurrentSlot = slot;

        if (loaded == null)
        {
            Debug.Log("槽位 " + slot + " 不存在已保存的数据");
            gameData = new GameData();
            return;
        }

        gameData = loaded;
        foreach (var saveable in allSaveable)
        {
            saveable.LoadData(gameData);
        }
    }

    /// <summary>开始新游戏：清空当前槽位的数据（下次保存才会真正写入该槽位文件）。</summary>
    public void NewGame(int slot)
    {
        CurrentSlot = slot;
        gameData = new GameData();
    }

    /// <summary>判断某槽位是否存在存档（供存档槽位 UI 显示"有/无"）。</summary>
    public bool HasSaveData(int slot) => GetDataHandler(slot).LoadData() != null;

    public GameData GetGameData() => gameData;

    public void DeleteSaveData(int slot)
    {
        GetDataHandler(slot).Delete();
        if (slot == CurrentSlot)
            gameData = null;

        LoadGame(slot);
    }

    private void OnApplicationQuit()
    {
        if (gameData != null)
        {
            SaveGame();
        }
    }

    private List<ISaveable> FindISaveables()
    {
        return
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
            .OfType<ISaveable>()
            .ToList();
    }
}
