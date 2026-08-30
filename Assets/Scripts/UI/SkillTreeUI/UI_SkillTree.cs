using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 技能树：集中管理玩家剩余技能点，并驱动节点间连线刷新。
/// 为什么单独一个组件而不是由各 UI_TreeNode 自己管理技能点：
/// 技能点是所有节点共享的资源（解锁扣点、重置退点），必须只有一个
/// 数据源，节点只能通过方法读写，避免各节点持有的点数互相不同步。
/// </summary>
public class UI_SkillTree : MonoBehaviour
{
    [SerializeField] private int skillPoints;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private UI_TreeConnectionHandler[] parentNodes;
    [SerializeField] private UI_TreeNode[] allTreeNodes;

    public Player_SkillManager skillManager { get; private set; }

    // 点数读写全部收口在这三个方法：
    // 节点不直接改 skillPoints，解锁判定永远基于同一份数据
    public bool EnoughSkillPoints(int cost) => skillPoints >= cost;
    public void RemoveSkillPoints(int cost)
    {
        skillPoints -= cost;
        UpdateSkillPointsUI();
    }

    public void AddSkillPoints(int points)
    {
        skillPoints += points;
        UpdateSkillPointsUI();
    }

    private void Start()
    {
        // 连线刷新放 Start 而不是 Awake：
        // 需要等所有节点/连线组件的 Awake 执行完、引用就绪后，
        // 才能算出正确的连线位置与锁定颜色
        UpdateAllConnections();
        UpdateSkillPointsUI();
    }

    private void UpdateSkillPointsUI()
    {
        skillPointsText.text = skillPoints.ToString();
    }

    public void UnlockDefaultSkills()
    {
        allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);
        skillManager = FindAnyObjectByType<Player_SkillManager>();

        foreach (var node in allTreeNodes)
        {
            node.UnlockDefaultSkills();
        }
    }

    // ContextMenu 让组件右键菜单即可一键重置，主要用于编辑期调试；
    // 也作为公开方法保留，便于运行时调用
    [ContextMenu("重置技能树")]
    public void RefundAllSkills()
    {
        UI_TreeNode[] skillNodes = GetComponentsInChildren<UI_TreeNode>();

        foreach (var node in skillNodes)
        {
            node.Refund();
        }
    }

    [ContextMenu("更新技能树")]
    public void UpdateAllConnections()
    {
        // 空数组保护：Inspector 未拖入根节点时给出提示并返回，
        // 避免 foreach 空引用；单个节点为空则跳过，允许连线部分缺失
        if (parentNodes == null)
        {
            Debug.LogWarning("未分配根节点数组 parentNodes，无法更新技能树连线");
            return;
        }
        foreach (var node in parentNodes)
        {
            if (node == null) continue;
            node.UpdateAllConnections();
        }
    }
}
