using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 技能树节点：一个技能图标，带解锁/锁定状态与前/后置关系。
/// 为什么解锁规则放在节点自己身上：每个节点的前置/冲突关系都是独有的，
/// 节点持有自己的数组并在点击时判定，不需要集中式规则表。
/// 为什么实现指针事件接口而不是由 UI_SkillTree 统一分发：
/// 节点既是个体交互目标（悬停/点击）又持有自己的数据，自包含处理最直接。
/// </summary>
public class UI_TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private RectTransform rect;
    private UI_SkillTree skillTree;
    private UI_TreeConnectionHandler connectionHandler;

    [Header("解锁详情")]
    // neededNodes：必须全部解锁才能点本技能；
    // conflictNodes：互斥分支，解锁其中任一节点会锁定本技能整条子树（二选一）
    [Tooltip("前置节点")]
    public UI_TreeNode[] neededNodes;
    [Tooltip("冲突节点")]
    public UI_TreeNode[] conflictNodes;
    [Tooltip("技能是否锁定")]
    public bool isLocked;
    [Tooltip("技能是否解锁")]
    public bool isUnLocked;

    [Header("技能详情")]
    public SkillDataSO skillData;
    [SerializeField] private string skillName;
    [SerializeField] private Image skillIcon;
    [SerializeField] private int skillCost;
    // 锁定状态的灰色 #9F9797：中性灰与解锁的纯白对比明显，一眼区分状态
    private string lockedColorHex = "#9F9797";
    private Color lastColor;

    private void OnValidate()
    {
        if (skillData == null) return;

        skillName = skillData.displayName;
        skillIcon.sprite = skillData.icon;
        skillCost = skillData.cost;
        gameObject.name = "UI_TreeNode - " + skillData.displayName;
    }

    private void Awake()
    {
        // Awake 只做引用缓存与初始着色：默认按"锁定灰"显示，
        // 是否真正解锁交给 Start判断
        if (skillIcon != null && skillData != null)
        {
            skillIcon.sprite = skillData.icon;
        }
    }

    private void Start()
    {
        // 按"当前实际状态"着色，而不是一律置灰：
        // UI.Start（场景加载时）已经通过 UnlockDefaultSkills 解锁了默认技能（isUnLocked=true），
        // 若这里强制置灰，已解锁的节点会显示成锁定灰；随后 Unlock 因 isUnLocked 提前 return，
        // 颜色永远不会被重新上成白色——节点看起来就像"没解锁、也没更新"。
        // 这里先根据状态上色，再允许 UnlockDefaultSkills 对仍未解锁的默认节点补一次解锁。
        if (isUnLocked)
        {
            UpdateIconColor(Color.white);
        }
        else
        {
            UpdateIconColor(GetColorByHex(lockedColorHex));
        }

        UnlockDefaultSkills();
    }

    public void UnlockDefaultSkills()
    {
        GetNeededComponents();
        // Unlock 会调用技能树与连线组件，必须等它们的 Awake 先跑完
        if (skillData != null && skillData.unlockedByDefault)
        {
            Unlock();

        }
    }

    private void GetNeededComponents()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        skillTree = GetComponentInParent<UI_SkillTree>(true);
        connectionHandler = GetComponent<UI_TreeConnectionHandler>();
    }

    // 重置只把"已解锁"退回成锁定并返还技能点
    //（下方注释：不会退回技能升级）——升级数据一旦写入真实技能就保留，
    // 重置是技能树的回退手段，不是技能本身的洗点
    public void Refund()            //不会退回技能升级
    {
        if (isUnLocked && !skillData.unlockedByDefault)
        {
            isUnLocked = false;
            isLocked = false;
            UpdateIconColor(GetColorByHex(lockedColorHex));

            skillTree.AddSkillPoints(skillData.cost);
            connectionHandler.ConnectionImageUnlocked(false);
        }
    }

    // 解锁完整流程：改状态/图标 → 扣技能点 → 更新连线颜色 → 写入真实技能。
    // 后续步骤都带空引用保护：Inspector 漏配组件时警告而不是崩溃
    private void Unlock()
    {
        if (isUnLocked)
        {
            Debug.Log("技能已解锁");
            return;
        }

        isUnLocked = true;
        UpdateIconColor(Color.white);
        LockConflictNodes();

        if (skillTree == null)
        {
            Debug.LogWarning("未找到 UI_SkillTree 组件，无法扣除技能点");
        }
        else
        {
            skillTree.RemoveSkillPoints(skillData.cost);
        }

        if (connectionHandler == null)
        {
            Debug.LogWarning("未找到 UI_TreeConnectionHandler 组件，无法更新连线颜色");
        }
        else
        {
            connectionHandler.ConnectionImageUnlocked(true);
        }

        if (skillData == null || skillData.upgradeDate == null)
        {
            Debug.LogWarning("技能数据或升级数据为空，无法升级技能");
            return;
        }

        if (skillTree == null || skillTree.skillManager == null)
        {
            Debug.LogWarning("未找到技能管理器，无法升级技能");
            return;
        }

        Skill_Base skill = skillTree.skillManager.GetSkillByType(skillData.skillType);
        if (skill == null)
        {
            Debug.LogWarning("技能类型 " + skillData.skillType + " 尚未在技能管理器中实现");
            return;
        }

        skill.SetSkillUpgrade(skillData);
    }

    // 判定顺序即优先级：锁定 > 已解锁 > 技能点 > 前置 > 冲突。
    // "已解锁"也判为不可再点，防止重复点击扣两次技能点
    private bool CanBeUnlocked()
    {
        if (isLocked)
        {
            Debug.Log("技能已锁定");
            return false;
        }

        if (isUnLocked)
        {
            Debug.Log("技能已解锁");
            return false;
        }

        if (!skillTree.EnoughSkillPoints(skillData.cost))
        {
            Debug.Log("技能点不足");
            return false;
        }

        foreach (var node in neededNodes)
        {
            if (!node.isUnLocked)
            {
                Debug.Log("前置节点未解锁");
                return false;
            }
        }

        foreach (var node in conflictNodes)
        {
            if (node.isUnLocked)
            {
                Debug.Log("冲突节点已解锁");
                return false;
            }
        }
        return true;
    }

    // 互斥实现：解锁本技能时把冲突节点及其整条子树递归锁定，
    // 保证"二选一"分支里另一侧从根到叶全部不可用
    private void LockConflictNodes()
    {
        foreach (var node in conflictNodes)
        {
            node.isLocked = true;
            node.LockChildNodes();
        }
    }

    // 锁定沿连线向下递归传播：父节点被锁定时其所有后代也不能解锁
    private void LockChildNodes()
    {
        isLocked = true;
        foreach (var node in connectionHandler.GetChildNodes())
        {
            node.LockChildNodes();
        }
    }

    private void UpdateIconColor(Color color)
    {
        if (skillIcon == null) return;

        // 换色前缓存旧色：悬停高亮结束后要恢复成原来的颜色
        lastColor = skillIcon.color;
        skillIcon.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 点击分支：满足条件就解锁；已被锁定则触发闪烁提示；
        // 其余情况（技能点不足/前置未解锁）只打日志，原因由 ToolTip 展示
        if (CanBeUnlocked())
        {
            Unlock();
            Debug.Log("技能解锁");
        }
        else if (isLocked)
        {
            Debug.Log("技能无法解锁");
            ui.skillToolTip.LockedSkillEffect();
        }
        else
        {
            Debug.Log("技能无法解锁");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ui == null || ui.skillToolTip == null)
        {
            Debug.LogWarning("未找到 UI 或 SkillToolTip 组件，无法显示技能提示");
            return;
        }

        if (skillData == null)
        {
            Debug.LogWarning("技能数据 skillData 未在 Inspector 中赋值");
            return;
        }

        // 技能树面板未激活时不响应悬停：
        // ToolTip 属于面板层，面板关闭时节点仍在场景中但不应弹提示
        if (skillTree == null || !skillTree.gameObject.activeInHierarchy) return;
        ui.skillToolTip.ShowToolTip(true, rect, this);

        // 只有"可解锁但还没点"的节点才做悬停高亮：
        // 已解锁/已锁定的颜色有明确语义，不能被悬停色覆盖
        if (isUnLocked || isLocked) return;

        ToggleNodeHighLight(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui == null || ui.skillToolTip == null) return;
        ui.skillToolTip.ShowToolTip(false, rect);

        if (isUnLocked || isLocked) return;

        ToggleNodeHighLight(false);
    }

    private void ToggleNodeHighLight(bool highLight)
    {
        Color highLightColor = Color.white * 0.9f;
        highLightColor.a = 1;
        Color colorToApply = highLight ? highLightColor : lastColor;

        UpdateIconColor(colorToApply);
    }

    private Color GetColorByHex(string hexNumber)
    {
        ColorUtility.TryParseHtmlString(hexNumber, out Color color);

        return color;
    }

    private void OnDisable()
    {
        // 面板关闭时恢复图标颜色：
        // 否则悬停高亮色会残留，下次打开面板看到错误的状态颜色
        if (isLocked)
        {
            UpdateIconColor(GetColorByHex(lockedColorHex));
        }

        if (isUnLocked)
        {
            UpdateIconColor(Color.white);
        }
    }
}
