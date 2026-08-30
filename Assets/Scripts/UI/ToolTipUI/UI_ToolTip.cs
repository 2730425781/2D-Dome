using System;
using UnityEngine;

/// <summary>
/// ToolTip 基类：统一处理"跟随目标 + 屏幕边缘夹紧"的定位逻辑，
/// 子类（技能/物品/属性）只负责填充各自的内容。
/// 为什么用继承而不是一个组件切换内容：各 ToolTip 在场景中有独立布局与样式，
/// 把定位逻辑抽到基类，避免三个子类各写一遍相同的屏幕边界判断。
/// </summary>
public class UI_ToolTip : MonoBehaviour
{
    // protected：子类（技能/物品/属性提示框）覆写 UpdatePosition 定位时需要读取自身尺寸
    protected RectTransform rect;
    // ToolTip 相对目标的偏移：(300, 20) 表示横向放到目标侧边 300px 处、
    // 上下各留 20px 边距，保证不遮住目标本身
    [SerializeField] private Vector2 offset = new Vector2(300, 20);

    protected virtual void Awake()
    {
        rect = GetComponent<RectTransform>();
        // 开局移出屏幕，避免 ToolTip 在场景中预设的位置闪现
        rect.position = new Vector2(9999, 9999);
    }

    public virtual void ShowToolTip(bool show, RectTransform targetRect)
    {
        // Unity 假空校验：本项目关闭了域重载（Enter Play Mode Options → Reload Domain），
        // 播放模式切换时对象被销毁但缓存引用残留，rect 会指向已销毁的 RectTransform。
        // 先尝试重新获取；组件所在物体都没了（GetComponent 仍返回 null）就直接放弃，
        // 避免对已销毁对象赋值抛出 MissingReferenceException
        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
            if (rect == null) return;
        }

        if (!show)
        {
            // 隐藏时移出屏幕而不是 SetActive(false)：
            // 避免反复激活/禁用触发布局重建与文本重排，也让引用始终保持有效
            rect.position = new Vector2(9999, 9999);
            return;
        }

        // 目标槽位/节点可能已被销毁（如背包刷新重建了格子），
        // 定位到已销毁对象同样会抛异常，这里统一拦截
        if (targetRect == null) return;

        UpdatePosition(targetRect);
    }

    protected virtual void UpdatePosition(RectTransform targetRect)
    {
        float screenCenterX = Screen.width / 2;
        float screenTop = Screen.height;
        float screenBottom = 0;

        // 左右夹紧：目标在屏幕右半边时 ToolTip 放到其左侧，反之放右侧，
        // 否则 ToolTip 会超出屏幕右边缘而看不全
        Vector2 targetPosition = targetRect.position;
        targetPosition.x = targetPosition.x > screenCenterX ? targetPosition.x - offset.x : targetPosition.x + offset.x;

        // 纵向夹紧：用 ToolTip 自身高度（sizeDelta.y / 2）算上下边界，
        // 因为内容长短不一时高度会变化，不能写死固定值
        float yHalf = rect.sizeDelta.y / 2;
        float yTop = targetPosition.y + yHalf;
        float yBottom = targetPosition.y - yHalf;
        if (yTop > screenTop)
        {
            targetPosition.y = screenTop - yHalf - offset.y;
        }
        else if (yBottom < screenBottom)
        {
            targetPosition.y = screenBottom + yHalf + offset.y;
        }
        rect.position = targetPosition;
    }

    protected string GetColoredText(string color, string text)
    {
        // 用 TMP 富文本颜色标签给文字染色：
        // 同一段文本需要多种颜色（如"已满足/未满足"）时，
        // 用标签比拆成多个 Text 组件更简单
        return $"<color={color}>{text}</color>";
    }
}
