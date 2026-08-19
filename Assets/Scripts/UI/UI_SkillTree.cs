using UnityEngine;

public class UI_SkillTree : MonoBehaviour
{
    [SerializeField] private int skillPoints;
    [SerializeField] private UI_TreeConnectionHandler[] parentNodes;
    public Player_SkillManager skillManager { get; private set; }

    public bool EnoughSkillPoints(int cost) => skillPoints >= cost;
    public void RemoveSkillPoints(int cost) => skillPoints -= cost;
    public void AddSkillPoints(int points) => skillPoints += points;

    private void Awake()
    {
        skillManager = FindAnyObjectByType<Player_SkillManager>();
    }

    private void Start()
    {
        UpdateAllConnections();
    }

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
