using UnityEngine;

/// <summary>
/// 单个视差层的数据类（Serializable 以在 Inspector 中嵌套配置）。
/// 包含背景图片的 Transform、视差倍率，以及图片尺寸和循环逻辑。
/// </summary>
[System.Serializable]
public class ParallaxLayer
{
    [SerializeField] private Transform background;          // 背景图片的 Transform
    [SerializeField] private float parallaxMultipier;       // 视差倍率（0=静止, 1=随摄像机同步移动）
    [SerializeField] private float imageWidthOffset = 10;   // 循环检测的容差偏移量

    private float imageFullWidth;   // 图片完整宽度
    private float imageHalfWidth;   // 图片一半宽度

    /// <summary>
    /// 读取 SpriteRenderer 的宽并计算半宽。
    /// </summary>
    public void ImageWidth()
    {
        imageFullWidth = background.GetComponent<SpriteRenderer>().bounds.size.x;
        imageHalfWidth = imageFullWidth / 2;
    }

    /// <summary>
    /// 根据摄像机位移和视差倍率移动背景。
    /// </summary>
    public void Move(float distanceToMove)
    {
        background.position += Vector3.right * (distanceToMove * parallaxMultipier);
    }

    /// <summary>
    /// 当图片完全移出屏幕一侧时，将其拼接到另一侧，实现无缝循环。
    /// </summary>
    public void LoopBackGround(float cameraLeftEdge, float cameraRightEdge)
    {
        float imageRightEdge = background.position.x + imageHalfWidth - imageWidthOffset;
        float imageLeftEdge  = background.position.x - imageHalfWidth + imageWidthOffset;

        // 图片右边界完全出左屏 → 拼到右侧
        if (imageRightEdge < cameraLeftEdge)
        {
            background.position += Vector3.right * imageFullWidth;
        }
        // 图片左边界完全出右屏 → 拼到左侧
        if (imageLeftEdge > cameraRightEdge)
        {
            background.position += Vector3.right * -imageFullWidth;
        }
    }
}