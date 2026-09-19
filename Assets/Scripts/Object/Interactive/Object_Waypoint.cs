using UnityEngine;
using UnityEngine.SceneManagement;

public class Object_Waypoint : MonoBehaviour, IInteractable
{
    [SerializeField] private string transferToScene;
    [Space]
    [SerializeField] private RespawnType waypointType;
    [SerializeField] private RespawnType conntedWaypoint;
    [SerializeField] private Transform respwanPoint;

    private bool canBeTrigger = false;

    public RespawnType GetWaypointType() => waypointType;

    public Vector3 GetPosition()
    {
        return respwanPoint == null ? transform.position : respwanPoint.position;
    }

    private void OnValidate()
    {
        gameObject.name = "Object-Waypoint - " + waypointType.ToString() + " - " + transferToScene;

        if (waypointType == RespawnType.Enter)
        {
            conntedWaypoint = RespawnType.Exit;
        }

        if (waypointType == RespawnType.Exit)
        {
            conntedWaypoint = RespawnType.Enter;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        canBeTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        canBeTrigger = false;
    }

    public void Interact()
    {
        if (!canBeTrigger)
        {
            return;
        }

        SaveManager.instance.SaveGame();
        GameManager.instance.ChangeScene(transferToScene, conntedWaypoint);
    }
}
