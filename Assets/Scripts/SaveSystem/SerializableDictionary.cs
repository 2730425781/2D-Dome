using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<Tkey, TValue> : Dictionary<Tkey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<Tkey> keys = new List<Tkey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
        {
            Debug.Log("键与值的数量不相等");
        }

        for (int i = 0; i < keys.Count; i++)
        {
            if (!ContainsKey(keys[i]))
                Add(keys[i], values[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<Tkey, TValue> pairs in this)
        {
            keys.Add(pairs.Key);
            values.Add(pairs.Value);
        }
    }
}
