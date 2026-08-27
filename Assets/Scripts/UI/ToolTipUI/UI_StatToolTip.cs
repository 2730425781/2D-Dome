using System;
using TMPro;
using UnityEngine;

public class UI_StatToolTip : UI_ToolTip
{
    private Player_Stats playerStats;
    private TextMeshProUGUI statToolTipText;
    protected override void Awake()
    {
        base.Awake();
        playerStats = FindAnyObjectByType<Player_Stats>();
        statToolTipText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowToolTip(bool show, RectTransform targetRect, StatType statType)
    {
        // 组件被销毁时（关闭域重载的播放模式切换），base 提前返回后
        // 这里仍会访问 statToolTipText，先拦掉避免对已销毁对象赋值
        if (this == null) return;

        base.ShowToolTip(show, targetRect);
        statToolTipText.text = GetStatTextByType(statType);
    }

    // 属性说明统一使用中文，描述与属性面板的数值逻辑保持一致
    public string GetStatTextByType(StatType type)
    {
        switch (type)
        {
            // 主要属性
            case StatType.Strength:
                return "每点力量提高 1 点物理伤害。" +
                       "\n 每点力量提高 0.5% 暴击伤害。";
            case StatType.Agility:
                return "每点敏捷提高 0.3% 暴击率。" +
                       "\n 每点敏捷提高 0.5% 闪避。";
            case StatType.Intelligence:
                return "每点智力提高 0.5% 元素抗性。" +
                        "\n 每点智力额外提供 1 点元素伤害。" +
                        "\n 如果所有元素伤害为 0，则该加成不会生效。";
            case StatType.Vitality:
                return "每点体力提高 5 点最大生命值。" +
                       "\n 每点体力提高 1 点护甲。";

            // 物理伤害
            case StatType.Damage:
                return "决定你攻击造成的物理伤害。";
            case StatType.CritChance:
                return "攻击造成暴击的几率。";
            case StatType.CritPower:
                return "提高暴击造成的伤害。";
            case StatType.ArmorReduction:
                return "攻击时无视目标护甲的比例。";
            case StatType.AttackSpeed:
                return "决定你的攻击速度。";

            // 防御
            case StatType.MaxHealth:
                return "决定你的总生命值。";
            case StatType.HealthRegen:
                return "每秒恢复的生命值。";
            case StatType.Armor:
                return "减少受到的物理伤害。"
                    + "\n 护甲减伤上限为 85%。"
                    + "当前减伤为：" + playerStats.GetArmorMitigation(0) * 100 + "%。";
            case StatType.Evasion:
                return "完全闪避攻击的几率。" + "\n 上限为 85%。";

            // 元素伤害
            case StatType.IceDamage:
                return "决定你攻击造成的冰冻伤害。";
            case StatType.FireDamage:
                return "决定你攻击造成的火焰伤害。";
            case StatType.LightningDamage:
                return "决定你攻击造成的闪电伤害。";
            case StatType.ElementalDamage:
                return
                    "元素伤害由三种元素伤害合并计算。" +
                    "\n 最高的元素会附加对应的元素状态效果，并造成全额伤害。" +
                    "\n 其余两种元素提供 50% 的伤害加成。";

            // 元素抗性
            case StatType.IceResistance:
                return "减少受到的冰冻伤害。";
            case StatType.FireResistance:
                return "减少受到的火焰伤害。";
            case StatType.LightningResistance:
                return "减少受到的闪电伤害。";

            default:
                return "该属性暂无说明。";
        }
    }
}