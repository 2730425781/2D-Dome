using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour, ISaveable
{
    public static GameManager instance;

    /// <summary>
    /// 是否正在执行关卡切换。UI_FadeScreen 用它决定"是否要以全黑开场"：
    /// 只有切换流程才会在加载完成后调用 DoFadeIn 取消黑屏；
    /// 直接从某个关卡启动游戏（编辑器直接 Play，或构建以该关卡为起始场景）时没有任何切换流程，
    /// 此时若遮罩仍被置成全黑，就会永久黑屏。
    /// </summary>
    public static bool IsChangingScene { get; private set; }

    private Vector3 lastPlayerPosition;
    private bool dataLoaded;
    private string lastScenePlayed;

    /// <summary>
    /// 本项目关闭了"域重载"(Enter Play Mode Options)，静态字段会跨播放会话残留。
    /// 每轮播放开始前复位，避免上一轮卡在切换流程中，导致本轮一进游戏就是黑屏。
    /// SubsystemRegistration 早于所有场景的 Awake，时序上是安全的。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsChangingScene = false;
    }

    private void Awake()
    {
        // 用 instance != this 区分"已有另一个实例"与"自己"（原写法是两个相同的条件）。
        // 关域重载时 instance 可能残留上一轮已销毁的对象，Unity 重载过的 == 会把它判成 null，
        // 正好用于存活判断，不会误把新实例销毁掉。
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // public void SetLastPlayerPosition(Vector3 position)
    // {
    //     lastPlayerPosition = position;
    // }

    public void ContinuePlay()
    {
        ChangeScene(lastScenePlayed, RespawnType.None);
    }

    public void RestartScene()
    {
        SaveManager.instance.SaveGame();

        string sceneName = SceneManager.GetActiveScene().name;
        ChangeScene(sceneName, RespawnType.None);
    }

    public void ChangeScene(string sceneName, RespawnType respawnType)
    {
        SaveManager.instance.SaveGame();
        Time.timeScale = 1;
        // 先打标记再加载：新场景的 UI_FadeScreen.Awake 会读它，从而以全黑开场等待淡入
        IsChangingScene = true;
        StartCoroutine(ChangeSceneCo(sceneName, respawnType));
    }

    private IEnumerator ChangeSceneCo(string sceneName, RespawnType respawnType)
    {
        UI_FadeScreen fadeScreen = FindFadeScreenUI();

        // 判空：场景里没有淡入淡出遮罩时不应直接抛异常中断整个切换流程
        if (fadeScreen != null)
        {
            fadeScreen.DoFadeOut();
            yield return fadeScreen.fadeEffectCo;
        }

        SceneManager.LoadScene(sceneName);

        dataLoaded = false;

        yield return null;              //加载游戏后需要延迟一帧

        while (!dataLoaded)
        {
            yield return null;
        }

        fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null)
        {
            fadeScreen.DoFadeIn();
            // 等淡入真正结束再清标记：期间若又触发切换会重新置位，不会被这里误清
            yield return fadeScreen.fadeEffectCo;
        }

        IsChangingScene = false;

        Player player = Player.instance;

        if (player == null)
            yield break;

        Vector3 position = GetNewPlayerPosition(respawnType);

        if (position != Vector3.zero)
        {
            Player.instance.TeleportPlayer(position);
        }
    }

    private UI_FadeScreen FindFadeScreenUI()
    {
        if (UI.instance != null)
        {
            return UI.instance.fadeScreenUI;
        }
        else
        {
            return FindAnyObjectByType<UI_FadeScreen>();
        }
    }

    private Vector3 GetNewPlayerPosition(RespawnType type)
    {
        if (type == RespawnType.None)
        {
            var data = SaveManager.instance.GetGameData();
            var checkpoints = FindObjectsByType<Object_Checkpoint>();
            var unlockedPoints = checkpoints
                .Where(cp => data.unlockedCheckpoints.TryGetValue(cp.GetCheckpointID(), out bool unlocked) && unlocked)
                .Select(cp => cp.GetPosition())
                .ToList();

            var enterWaypoints = FindObjectsByType<Object_Waypoint>()
                .Where(wp => wp.GetWaypointType() == RespawnType.Enter)
                .Select(wp => wp.GetPosition())
                .ToList();

            var selectedPositions = unlockedPoints.Concat(enterWaypoints).ToList();

            if (selectedPositions.Count == 0)
            {
                return Vector3.zero;
            }

            return selectedPositions.OrderBy(position => Vector3.Distance(position, lastPlayerPosition)).First();
        }

        return GetWaypointPosition(type);
    }

    private Vector3 GetWaypointPosition(RespawnType type)
    {
        var waypoints = FindObjectsByType<Object_Waypoint>();

        foreach (var point in waypoints)
        {
            if (point.GetWaypointType() == type)
            {
                return point.GetPosition();
            }
        }

        return Vector3.zero;
    }

    public void LoadData(GameData data)
    {
        lastScenePlayed = data.lastScenePlayed;
        lastPlayerPosition = data.lastPlayerPosition;

        if (string.IsNullOrEmpty(lastScenePlayed))
        {
            lastScenePlayed = "Level_0";
        }

        dataLoaded = true;
    }

    public void SaveData(ref GameData data)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainMenu")
        {
            return;
        }

        data.lastPlayerPosition = Player.instance.transform.position;
        data.lastScenePlayed = currentScene;
        dataLoaded = false;
    }
}
