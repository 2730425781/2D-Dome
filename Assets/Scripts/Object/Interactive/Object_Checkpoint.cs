using System;
using UnityEngine;

/// <summary>
/// 检查点：玩家触碰时激活并记录重生点。
///
/// 为什么在触发时遍历所有检查点：保证任意时刻只有一个检查点处于激活
/// 状态（视觉上只有当前的重生点亮着），且"最近触碰的那个"被记录为 savedCheckpoint。
/// 实现 ISaveable：把当前重生点写入/恢复存档数据。
/// </summary>
public class Object_Checkpoint : MonoBehaviour, ISaveable, IInteractable
{
    [SerializeField] private string checkpointID;
    [SerializeField] private Transform respawnPoint;
    public bool isActive { get; private set; }
    private Animator animator;
    private AudioSource fireAudioSource;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        fireAudioSource = GetComponent<AudioSource>();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(checkpointID))
        {
            checkpointID = Guid.NewGuid().ToString();
        }
#endif
    }

    public string GetCheckpointID() => checkpointID;

    public Vector3 GetPosition()
    {
        if (respawnPoint == null)
        {
            return transform.position;
        }
        else
        {
            return respawnPoint.position;
        }
    }

    public void ActivateCheckpoint(bool active)
    {
        isActive = active;
        animator.SetBool("isActive", active);

        if (isActive && !fireAudioSource.isPlaying)
        {
            fireAudioSource.Play();
        }

        if (!isActive)
        {
            fireAudioSource.Stop();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interact();
    }

    public void LoadData(GameData data)
    {
        bool active = data.unlockedCheckpoints.TryGetValue(checkpointID, out active);
        ActivateCheckpoint(active);
    }

    public void SaveData(ref GameData data)
    {
        if (!isActive)
        {
            return;
        }

        if (!data.unlockedCheckpoints.ContainsKey(checkpointID))
        {
            data.unlockedCheckpoints.Add(checkpointID, true);
        }
    }

    public void Interact()
    {
        ActivateCheckpoint(true);
    }

}
