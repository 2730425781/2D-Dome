using System;
using System.Collections;
using UnityEngine;

public class Object_Buff : MonoBehaviour
{
    private Player_Stats statsToModify;

    [Header("Buff设置")]
    [SerializeField] private BuffEffectDate[] buffs;
    [SerializeField] private string buffName;
    //[SerializeField] private float buffValue = 5;
    [SerializeField] private float buffDuration = 4;
    [Header("Buff显示设置")]
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float floatRange = 0.1f;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = startPosition + new Vector3(0, yOffset);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        statsToModify = collider.GetComponent<Player_Stats>();

        if (statsToModify.CanApplyBuff(buffName))
        {
            statsToModify.ApplyBuff(buffs, buffDuration, buffName);
            Destroy(gameObject);
        }
    }
}
