using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private RectTransform rect;
    private UI_SkillTree skillTree;
    private UI_TreeConnectionHandler connectionHandler;

    [Header("解锁详情")]
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
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        skillTree = GetComponentInParent<UI_SkillTree>();
        connectionHandler = GetComponent<UI_TreeConnectionHandler>();

        if (skillIcon != null && skillData != null)
        {
            skillIcon.sprite = skillData.icon;
        }
        UpdateIconColor(GetColorByHex(lockedColorHex));
    }

    private void Start()
    {
        if (skillData != null && skillData.unlockedByDefault)
        {
            Unlock();
        }
    }

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

    private void Unlock()
    {
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
        skill.SetSkillUpgrade(skillData.upgradeDate);
    }

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

    private void LockConflictNodes()
    {
        foreach (var node in conflictNodes)
        {
            node.isLocked = true;
            node.LockChildNodes();
        }
    }

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

        lastColor = skillIcon.color;
        skillIcon.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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

        if (skillTree == null || !skillTree.gameObject.activeInHierarchy) return;
        ui.skillToolTip.ShowToolTip(true, rect, this);

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
