using System;
using UnityEngine;

public class UI_ToolTip : MonoBehaviour
{
    private RectTransform rect;
    [SerializeField] private Vector2 offset = new Vector2(300, 20);

    protected virtual void Awake()
    {
        rect = GetComponent<RectTransform>();
        // 开局移出屏幕，避免 ToolTip 在场景中预设的位置闪现
        rect.position = new Vector2(9999, 9999);
    }

    public virtual void ShowToolTip(bool show, RectTransform targetRect)
    {
        if (!show)
        {
            rect.position = new Vector2(9999, 9999);
            return;
        }
        UpdatePosition(targetRect);
    }

    private void UpdatePosition(RectTransform targetRect)
    {
        float screenCenterX = Screen.width / 2;
        float screenTop = Screen.height;
        float screenBottom = 0;

        Vector2 targetPosition = targetRect.position;
        targetPosition.x = targetPosition.x > screenCenterX ? targetPosition.x - offset.x : targetPosition.x + offset.x;

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
        return $"<color={color}>{text}</color>";
    }
}
