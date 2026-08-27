using System;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 一条技能连线：用"旋转点 + 长度"的方式绘制两个节点之间的连线。
/// 为什么用旋转 + 改长度而不是把图片直接拉伸到两点之间：
/// 技能树按固定 8 方向摆放，用 pivot 旋转和 sizeDelta 长度完全由数值驱动，
/// 避免计算任意两点间连线的复杂变换。
/// </summary>
public class UI_TreeConnection : MonoBehaviour
{
    [SerializeField] private RectTransform rotationPoint;
    [SerializeField] private RectTransform connectionLength;
    [SerializeField] private RectTransform childConnectionPoint;

    public void DirectConnection(NodeDirectionType direction, float length, float offset)
    {
        // 方向为 None 时长度归零：该位置在树中没有连线，
        // 用长度 0 隐藏而不是 SetActive(false)，避免反复启停
        bool shouldBeActive = direction != NodeDirectionType.None;
        float finalLength = shouldBeActive ? length : 0;
        float angle = GetDirectionAngle(direction);

        rotationPoint.localRotation = Quaternion.Euler(0, 0, angle + offset);
        connectionLength.sizeDelta = new Vector2(finalLength, connectionLength.sizeDelta.y);
    }

    public Image GetConnectionImage() => connectionLength.GetComponent<Image>();

    public Vector2 GetConnectionPoint(RectTransform rect)
    {
        // 把子节点连接点转换到父节点本地坐标：
        // 子节点最终要对齐到连线端点，必须统一到同一个坐标系再取位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            rect.parent as RectTransform,
            childConnectionPoint.position,
            null,
            out var localPosition
        );

        return localPosition;
    }

    // 8 个方向的固定角度：技能树按网格方向摆放节点，
    // 用离散角度而非任意角，保证连线永远笔直对齐网格
    private float GetDirectionAngle(NodeDirectionType type)
    {
        switch (type)
        {
            case NodeDirectionType.DownLeft: return -135f;
            case NodeDirectionType.Down: return -90f;
            case NodeDirectionType.DownRight: return -45f;
            case NodeDirectionType.Right: return 0f;
            case NodeDirectionType.UpRight: return 45f;
            case NodeDirectionType.Up: return 90f;
            case NodeDirectionType.UpLeft: return 135f;
            case NodeDirectionType.Left: return 180f;
            default: return 0;
        }
    }
}

/// <summary>
/// 连线方向：8 个罗盘方向 + None（该位置无连线）。
/// 枚举值顺序只影响 Inspector 下拉的排列，角度映射按值在
/// GetDirectionAngle 中一一对应。
/// </summary>
public enum NodeDirectionType
{
    None,
    UpLeft,
    Up,
    UpRight,
    Left,
    Right,
    DownLeft,
    Down,
    DownRight,
}