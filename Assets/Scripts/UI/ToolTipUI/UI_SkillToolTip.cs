using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 技能 ToolTip：继承 UI_ToolTip 复用"跟随目标 + 屏幕边缘夹紧"的定位逻辑，
/// 只负责填充技能内容与解锁需求。
/// 为什么按内容拆成独立子类（技能/物品/属性各一个 ToolTip 组件）而不是
/// 一个组件切换内容：各 ToolTip 在场景中有独立的布局与样式，
/// 拆开可以单独排版而不互相影响。
/// </summary>
public class UI_SkillToolTip : UI_ToolTip
{
    private UI ui;
    private UI_SkillTree skillTree;

    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillDescription;
    [SerializeField] private TextMeshProUGUI skillCooldown;
    [SerializeField] private TextMeshProUGUI skillRequirements;

    [Space]
    // 颜色约定：金黄 = 条件已满足，红 = 条件未满足，
    // 与节点图标的锁定灰色呼应，让玩家一眼看出缺哪个前置条件
    [SerializeField] private string importantInfoHex;
    [SerializeField] private string metConditionHex = "#CFBE2C";
    [SerializeField] private string notMetConditionHex = "#FF455D";
    [SerializeField] private string lockedSkillText = "this skill is now locked.";

    private Coroutine textEffectCo;

    protected override void Awake()
    {
        base.Awake();
        // 顺着父级 UI 取技能树引用：ToolTip 是 UI 的子物体，
        // 用父子关系定位组件，避免在 Inspector 里逐个手动拖引用
        ui = GetComponentInParent<UI>();
        skillTree = ui.GetComponentInChildren<UI_SkillTree>(true);
    }

    public override void ShowToolTip(bool show, RectTransform targetRect)
    {
        // 组件被销毁时（关闭域重载的播放模式切换），base 提前返回后
        // 这里仍会继续执行，先拦掉避免访问已销毁的子物体
        if (this == null) return;

        base.ShowToolTip(show, targetRect);
    }

    public void ShowToolTip(bool show, RectTransform targetRect, UI_TreeNode node)
    {
        // 同上：base 只负责定位，内容填充还在本方法里，销毁后必须整体放弃
        if (this == null) return;

        base.ShowToolTip(show, targetRect);
        if (!show) return;

        // 节点或技能数据为空时直接返回：
        // 没有数据可展示，强行访问 skillData 会空引用崩溃
        if (node == null || node.skillData == null) return;

        if (skillName != null) skillName.text = node.skillData.displayName;
        if (skillDescription != null) skillDescription.text = node.skillData.description;
        // 逐字段判空：这些文本组件在 Inspector 中可能单独漏配，
        // 判空避免某一个未赋值时整个 ToolTip 崩掉
        if (skillDescription != null) skillCooldown.text = "冷却时间：" + node.skillData.upgradeDate.cooldown + "秒";
        else Debug.LogWarning("skillDescription TextMeshPro 未在 Inspector 中赋值");

        string skillLockedText = GetColoredText(importantInfoHex, lockedSkillText);
        string requirements = node.isLocked ? skillLockedText : GetRequirements(node.skillData.cost, node.neededNodes, node.conflictNodes);

        if (skillRequirements != null) skillRequirements.text = requirements;
    }

    // 逐行生成需求说明：用 StringBuilder 而不是字符串 += 拼接，
    // 避免多次拼接产生临时字符串；每行的颜色即时反映该条件是否满足
    private string GetRequirements(int skillCost, UI_TreeNode[] neededNodes, UI_TreeNode[] conflictNodes)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("需求技能:");
        string costColor = skillTree.EnoughSkillPoints(skillCost) ? metConditionHex : notMetConditionHex;

        string costText = $"- {skillCost} 技能点";
        string finalCostText = GetColoredText(costColor, costText);
        sb.AppendLine(finalCostText);

        foreach (var node in neededNodes)
        {
            string nodeColor = node.isUnLocked ? metConditionHex : notMetConditionHex;
            string nodeText = $"- {node.skillData.displayName}";
            string finalNodeText = GetColoredText(nodeColor, nodeText);

            sb.AppendLine(finalNodeText);
        }

        if (conflictNodes.Length <= 0)
        {
            return sb.ToString();
        }

        sb.AppendLine();
        sb.AppendLine(GetColoredText(importantInfoHex, "已锁定技能: "));

        foreach (var node in conflictNodes)
        {
            string nodeText = $"- {node.skillData.displayName}";
            string finalNodeText = GetColoredText(importantInfoHex, nodeText);
            sb.AppendLine(finalNodeText);
        }

        return sb.ToString();
    }

    public void LockedSkillEffect()
    {
        // 先停旧协程再开新的：
        // 玩家快速连点锁定时不会叠加多个闪烁协程，闪烁节奏始终一致
        if (textEffectCo != null)
        {
            StopCoroutine(textEffectCo);
        }

        textEffectCo = StartCoroutine(TextBlinkEffectCo(skillRequirements, 0.15f, 3));
    }

    // 0.15s 间隔、闪烁 3 次：节奏短促刚好抓住注意力又不干扰操作；
    // 在"未满足"红与强调色之间交替，强化"该路线被锁定"的语义
    private IEnumerator TextBlinkEffectCo(TextMeshProUGUI text, float blinkInterval, int blinkCount)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            text.text = GetColoredText(notMetConditionHex, lockedSkillText);
            yield return new WaitForSeconds(blinkInterval);

            text.text = GetColoredText(importantInfoHex, lockedSkillText);
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
