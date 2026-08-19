using System.Collections;
using UnityEngine;

public class VFX_AutoController : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("自毁设置")]
    [SerializeField] private bool autoDestroy = true;
    [SerializeField] private float destroyDelay = 1;
    [Header("淡出设置")]
    [SerializeField] private bool canFade;
    [SerializeField] private float fadeSpeed = 1;
    [Header("随机设置")]
    [SerializeField] private bool randomOffset = true;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private float minRotation = 0;
    [SerializeField] private float maxRotation = 360;
    [Space]
    [SerializeField] private float xMinOffset = -0.3f;
    [SerializeField] private float xMaxOffset = 0.3f;
    [Space]
    [SerializeField] private float yMinOffset = -0.3f;
    [SerializeField] private float yMaxOffset = 0.3f;


    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (canFade)
        {
            StartCoroutine(FadeCo());
        }
        
        ApplyRandomOffset();
        ApplyRandomRotation();
        if (autoDestroy)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private IEnumerator FadeCo()
    {
        Color targetColor = Color.white;

        while (targetColor.a > 0)
        {
            targetColor.a -= fadeSpeed * Time.deltaTime;
            sr.color = targetColor;
            yield return null;
        }
    }

    private void ApplyRandomOffset()
    {
        if (!randomOffset)
        {
            return;
        }
        float xOffset = Random.Range(xMinOffset, xMaxOffset);
        float yOffset = Random.Range(yMinOffset, yMaxOffset);

        transform.position = transform.position + new Vector3(xOffset, yOffset);
    }

    private void ApplyRandomRotation()
    {
        if (!randomRotation)
        {
            return;
        }
        float zRotation = Random.Range(minRotation, maxRotation);
        transform.Rotate(0, 0, zRotation);
    }
}
/// 自动控制 VFX 对象生命周期：
/// 1. 生成时随机偏移和旋转，让每次特效看起来略有不同（而非死板的一致）。
/// 2. autoDestroy 销毁机制替代手动管理：VFX 是"发射后不管"的一次性对象，
///    不自动销毁会导致场景中堆积大量无效粒子/贴图对象。
/// 3. 为什么用 Start() 而不是 Awake()：随机偏移依赖 Transform 初始化完毕，
///    Awake 时位置可能还未最终确定（比如 Instantiate 赋值位置发生在 Start 之前）。
/// </summary>
