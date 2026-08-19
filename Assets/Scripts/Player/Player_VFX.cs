using System.Collections;
using UnityEngine;

public class Player_VFX : Entity_VFX
{
    [Header("残影设置")]
    [Range(0.01f, 0.2f)]
    [SerializeField] private float imageEchoInterval = 0.05f;
    [SerializeField] private GameObject imageEchoPrefab;
    private Coroutine imageEchoCo;

    public void CreateEffectOf(GameObject effect, Transform target)
    {
        Instantiate(effect, target.position, Quaternion.identity);
    }

    public void DoImageEchoEffect(float duretion)
    {
        if (imageEchoCo != null)
        {
            StopCoroutine(imageEchoCo);
        }

        imageEchoCo = StartCoroutine(ImageEchoEffectCo(duretion));
    }

    private IEnumerator ImageEchoEffectCo(float duartion)
    {
        float timeTracker = 0;

        while (timeTracker < duartion)
        {
            CreateImageEcho();

            yield return new WaitForSeconds(imageEchoInterval);
            timeTracker += imageEchoInterval;
        }
    }

    private void CreateImageEcho()
    {
        GameObject imageEcho = Instantiate(imageEchoPrefab, transform.position, transform.rotation);
        imageEcho.GetComponentInChildren<SpriteRenderer>().sprite = sr.sprite;      //把预制体中的精灵替换为Animator中的
    }
}
