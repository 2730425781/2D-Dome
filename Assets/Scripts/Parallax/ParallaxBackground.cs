using UnityEngine;

/// <summary>
/// 视差滚动背景控制器。
/// 在 FixedUpdate 中跟踪摄像机水平移动，驱动所有视差层移动和循环。
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastCameraPositionX;
    private float cameraHalfWidth;            // 摄像机水平半宽（用于判断是否出屏）

    [SerializeField] private ParallaxLayer[] backgroundLayer;

    private void Awake()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        InitializeLayers();
    }

    private void FixedUpdate()
    {
        // 计算摄像机这一帧的水平位移量
        float currentCameraPositionX = mainCamera.transform.position.x;
        float distanceToMove = currentCameraPositionX - lastCameraPositionX;
        lastCameraPositionX = currentCameraPositionX;

        float cameraLeftEdge = currentCameraPositionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPositionX + cameraHalfWidth;

        // 逐层应用视差移动和屏幕外循环
        foreach (ParallaxLayer layer in backgroundLayer)
        {
            layer.Move(distanceToMove);
            layer.LoopBackGround(cameraLeftEdge, cameraRightEdge);
        }
    }

    /// <summary>
    /// 初始化所有视差层（读取每层的图片宽度）。
    /// </summary>
    private void InitializeLayers()
    {
        foreach (ParallaxLayer layer in backgroundLayer)
        {
            layer.ImageWidth();
        }
    }
}