using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUITargetRegistry : MonoBehaviour
{
    [Serializable]
    private struct Entry
    {
        public TutorialTargetKey key;
        public RectTransform target;
    }

    [SerializeField] private List<Entry> entries;

    private Dictionary<TutorialTargetKey, RectTransform> map;

    private void Awake()
    {
        map = new Dictionary<TutorialTargetKey, RectTransform>();
        foreach (var e in entries)
            if (e.target != null) map[e.key] = e.target;
    }

    public RectTransform GetTarget(TutorialTargetKey key)
    {
        return map.TryGetValue(key, out var rt) ? rt : null;
    }
}
