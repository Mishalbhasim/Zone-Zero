using System;
using System.Collections.Generic;
using UnityEngine;


public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
        Debug.Log($"[ServiceLocator] Registered: {typeof(T).Name}");
    }

    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;

        Debug.LogError($"[ServiceLocator] Not found: {typeof(T).Name}. " +
                       $"Did you register it in GameBootstrapper?");
        return null;
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var raw))
        {
            service = (T)raw;
            return true;
        }
        service = null;
        return false;
    }

    public static void Unregister<T>() where T : class =>
        _services.Remove(typeof(T));

    public static void Clear() => _services.Clear();
}