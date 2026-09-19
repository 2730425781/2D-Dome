using UnityEngine;

public class Object_Ports : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector2 dafaultPosition;
    //[SerializeField] private string townSceneName = "Level_0";

    [SerializeField] private bool canBetriggerd;
    [SerializeField] private Transform respawnPoint;

    private void UseTeleport()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBetriggerd)
        {
            return;
        }

        UseTeleport();
    }

    public void SetTrigger(bool trigger) => canBetriggerd = trigger;
    public Vector3 GEtPosition() => respawnPoint != null ? respawnPoint.position : transform.position;

    public void Interact()
    {
        canBetriggerd = true;
    }
}
