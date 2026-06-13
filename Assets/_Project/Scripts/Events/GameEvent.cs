using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    private readonly List<GameEventListener> _listeners = new();

    public void Raise()
    {
        // iterate backwards — listeners may remove themselves during callback
        for (int i = _listeners.Count - 1; i >= 0; i--)
            _listeners[i].OnEventRaised();
    }

    public void RegisterListener(GameEventListener listener) =>
        _listeners.Add(listener);

    public void UnregisterListener(GameEventListener listener) =>
        _listeners.Remove(listener);
}