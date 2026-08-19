using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Stats : Entity_Stats
{
    private Inventory_Player inventory;
    private List<string> activeBuff = new List<string>();

    protected override void Awake()
    {
        inventory = GetComponent<Inventory_Player>();
    }

    public bool CanApplyBuff(string source)
    {
        return !activeBuff.Contains(source);
    }

    public void ApplyBuff(BuffEffectDate[] buffs, float duration, string source)
    {
        StartCoroutine(BuffCo(buffs, duration, source));
    }

    private IEnumerator BuffCo(BuffEffectDate[] buffs, float duration, string source)
    {
        activeBuff.Add(source);

        foreach (var buff in buffs)
        {
            GetStatValueByType(buff.type).AddModifier(buff.value, source);
        }

        yield return new WaitForSeconds(duration);

        foreach (var buff in buffs)
        {
            GetStatValueByType(buff.type).RemoveModifier(source);
        }

        inventory.TriggerDateUI();
        activeBuff.Remove(source);
    }
}
