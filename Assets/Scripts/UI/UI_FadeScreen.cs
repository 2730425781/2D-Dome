using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全屏淡入淡出遮罩。
///
/// 关键点：开场该不该全黑，取决于"有没有正在进行的关卡切换"。
/// - 有关卡切换（MainMenu → 关卡）：以全黑开场，避免新场景在淡入前闪现；随后由
///   GameManager.ChangeSceneCo 调用 DoFadeIn 把黑幕淡开。
/// - 直接从某个关卡启动游戏（编辑器直接 Play，或构建以该关卡为起始场景）：没有任何
///   切换流程，也就没人会调用 DoFadeIn。这种情况下如果还置成全黑，就会永久黑屏。
/// </summary>
public class UI_FadeScreen : MonoBehaviour
{
    private Image fadeImage;
    public Coroutine fadeEffectCo { get; private set; }

    private void Awake()
    {
        fadeImage = GetComponent<Image>();

        // 只有切换流程才允许全黑开场，其余情况一律透明
        fadeImage.color = new Color(0, 0, 0, GameManager.IsChangingScene ? 1f : 0f);
    }

    private void Start()
    {
        // 兜底：非切换流程下再确认一次遮罩是透明的。
        // Awake 的执行顺序不保证（GameManager 的静态复位与它的读取可能颠倒），
        // 这里再兜一层，确保"直接从关卡启动"永远不会黑屏。
        if (!GameManager.IsChangingScene && fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }

    /// <summary>淡入：从全黑变透明（黑幕消失，画面显现）。</summary>
    public void DoFadeIn(float duration = 1)
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        FadeEffect(0f, duration);
    }

    /// <summary>淡出：从透明变全黑（画面被黑幕盖住），用于切场景前。</summary>
    public void DoFadeOut(float duration = 1)
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        FadeEffect(1f, duration);
    }

    private void FadeEffect(float targetAlpha, float duration)
    {
        if (fadeEffectCo != null)
        {
            StopCoroutine(fadeEffectCo);
        }

        fadeEffectCo = StartCoroutine(FadeEffectCo(targetAlpha, duration));
    }

    private IEnumerator FadeEffectCo(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            // 用 unscaledDeltaTime 而不是 deltaTime：
            // 淡入淡出属于界面过渡，不该受 timeScale 影响。
            // timeScale 为 0 时（例如暂停中切场景）deltaTime 为 0，循环会永远走不完，
            // 遮罩就会卡在全黑再也退不掉
            time += Time.unscaledDeltaTime;

            var color = fadeImage.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            fadeImage.color = color;

            yield return null;
        }

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
    }
}
