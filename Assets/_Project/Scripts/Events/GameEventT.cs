using System.Collections.Generic;
using UnityEngine;


public abstract class GameEventT<T> : ScriptableObject
{
    private readonly List<System.Action<T>> _listeners = new();

    public void Raise(T value)
    {
        for (int i = _listeners.Count - 1; i >= 0; i--)
            _listeners[i]?.Invoke(value);
    }

    public void AddListener(System.Action<T> listener) => _listeners.Add(listener);
    public void RemoveListener(System.Action<T> listener) => _listeners.Remove(listener);
}

// ── Concrete types (create assets from these) ────────────────────────────────

[CreateAssetMenu(menuName = "Events/Int Event")]
public class IntEvent : GameEventT<int> { }

[CreateAssetMenu(menuName = "Events/Float Event")]
public class FloatEvent : GameEventT<float> { }

[CreateAssetMenu(menuName = "Events/String Event")]
public class StringEvent : GameEventT<string> { }

[CreateAssetMenu(menuName = "Events/Bool Event")]
public class BoolEvent : GameEventT<bool> { }