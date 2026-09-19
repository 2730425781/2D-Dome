using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Options : MonoBehaviour
{


    public void GoMainMenuBTN() => GameManager.instance.ChangeScene("MainMenu", RespawnType.None);
}
