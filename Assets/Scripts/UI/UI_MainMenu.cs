using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
    private void Start()
    {
        transform.root.GetComponentInChildren<UI_FadeScreen>().DoFadeIn();
        AudioManager.instance.StartBGM("mainMenuBgm");
    }

    public void PlayBTN()
    {
        AudioManager.instance.PlayGlobalSFX("ButtonClickSFX");
        GameManager.instance.ContinuePlay();
    }

    public void QuitGameBTN()
    {
        AudioManager.instance.PlayGlobalSFX("ButtonClickSFX");
        Application.Quit();
    }
}
