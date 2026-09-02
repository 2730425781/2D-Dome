using UnityEngine;

/// <summary>
/// 检查点：玩家触碰时激活并记录重生点。
///
/// 为什么在触发时遍历所有检查点：保证任意时刻只有一个检查点处于激活
/// 状态（视觉上只有当前的重生点亮着），且"最近触碰的那个"被记录为 savedCheckpoint。
/// 实现 ISaveable：把当前重生点写入/恢复存档数据。
/// </summary>
public class Object_Checkpoint : MonoBehaviour, ISaveable
{
    private Object_Checkpoint[] allCheckpoints;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        // 一次性缓存所有检查点：每次触发都要把其它检查点关掉，避免反复查找
        allCheckpoints = FindObjectsByType<Object_Checkpoint>();
    }

    public void ActivateCheckpoint(bool active)
    {
        animator.SetBool("isActive", active);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 先关掉所有其它检查点：只保留当前触碰的这个为激活态
        foreach (var point in allCheckpoints)
        {
            point.ActivateCheckpoint(false);
        }

        // 把本次触碰位置写入存档数据（SaveManager 在下次 SaveGame 时统一落盘）
        SaveManager.instance.GetGameData().savedCheckpoint = (Vector2)transform.position;
        ActivateCheckpoint(true);
    }

    public void LoadData(GameData data)
    {
        // 从存档恢复：只有存档记录的那个检查点保持激活，并把玩家传送回该处
        bool active = data.savedCheckpoint == transform.position;
        ActivateCheckpoint(active);
        if (active)
        {
            Player.instance.TeleportPlayer(transform.position);
        }
    }

    public void SaveData(ref GameData data)
    {
        // 检查点的重生点由 OnTriggerEnter2D 直接写入 gameData，这里无需额外保存
    }
}
