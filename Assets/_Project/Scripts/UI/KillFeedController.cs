using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillFeedController : MonoBehaviour
{
    [SerializeField] private GameObject _killFeedItemPrefab;
    [SerializeField] private Transform _killFeedPanel;
    [SerializeField] private int _maxItems = 5;
    [SerializeField] private float _fadeDuration = 5f;

    private Queue<GameObject> _items = new Queue<GameObject>();

    void OnEnable()
    {
        EventBus.OnPlayerEliminated += OnElimination;
    }

    void OnDisable()
    {
        EventBus.OnPlayerEliminated -= OnElimination;
    }

    private void OnElimination(string eliminatedId, int placement)
    {
        // check if it's local player
        string eliminatedName = eliminatedId == GameManager.Instance?.LocalPlayerId
            ? GameManager.Instance?.LocalPlayerName
            : $"Player_{placement}";

        AddKillFeedItem($"✕ {eliminatedName}");
    }
    public void AddKillFeedItem(string text)
    {
        // limit max items
        if (_items.Count >= _maxItems)
        {
            var old = _items.Dequeue();
            Destroy(old);
        }

        var item = Instantiate(_killFeedItemPrefab, _killFeedPanel);
        var tmp = item.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;

        _items.Enqueue(item);
        StartCoroutine(FadeAndRemove(item, tmp));
    }

    private IEnumerator FadeAndRemove(GameObject item, TextMeshProUGUI tmp)
    {
        yield return new WaitForSeconds(_fadeDuration - 1f);

        float timer = 0f;
        Color startColor = tmp.color;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - timer);
            yield return null;
        }

        // remove from queue safely
        if (_items.Contains(item))
            _items = new Queue<GameObject>(_items);

        Destroy(item);
    }
}