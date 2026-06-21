using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MainMenuAnimator : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private RectTransform _titleGroup;

    [Header("Buttons")]
    [SerializeField] private RectTransform _playButton;
    [SerializeField] private RectTransform _friendsButton;
    [SerializeField] private RectTransform _settingsButton;

    [Header("Animation Settings")]
    [SerializeField] private float _titleFadeTime = 0.8f;
    [SerializeField] private float _buttonFadeDelay = 0.3f;
    [SerializeField] private float _buttonFadeTime = 0.4f;

    private CanvasGroup _titleCanvasGroup;
    private CanvasGroup _playCanvasGroup;
    private CanvasGroup _friendsCanvasGroup;
    private CanvasGroup _settingsCanvasGroup;

    void Start()
    {
        SetupCanvasGroups();
        StartCoroutine(PlayEntranceSequence());
    }

    private void SetupCanvasGroups()
    {
        _titleCanvasGroup = GetOrAdd<CanvasGroup>(_titleGroup?.gameObject);
        _playCanvasGroup = GetOrAdd<CanvasGroup>(_playButton?.gameObject);
        _friendsCanvasGroup = GetOrAdd<CanvasGroup>(_friendsButton?.gameObject);
        _settingsCanvasGroup = GetOrAdd<CanvasGroup>(_settingsButton?.gameObject);

        // start invisible
        if (_titleCanvasGroup) _titleCanvasGroup.alpha = 0;
        if (_playCanvasGroup) _playCanvasGroup.alpha = 0;
        if (_friendsCanvasGroup) _friendsCanvasGroup.alpha = 0;
        if (_settingsCanvasGroup) _settingsCanvasGroup.alpha = 0;

        // start offset (slide in from left)
        if (_titleGroup) _titleGroup.anchoredPosition += new Vector2(-50, 0);
    }

    private IEnumerator PlayEntranceSequence()
    {
        yield return new WaitForSeconds(0.2f);

        // fade + slide title in
        yield return StartCoroutine(FadeAndSlide(_titleCanvasGroup, _titleGroup, _titleFadeTime));

        yield return new WaitForSeconds(0.1f);

        // fade buttons in staggered
        yield return StartCoroutine(FadeIn(_playCanvasGroup, _buttonFadeTime));
        yield return new WaitForSeconds(_buttonFadeDelay);
        yield return StartCoroutine(FadeIn(_friendsCanvasGroup, _buttonFadeTime));
        yield return new WaitForSeconds(_buttonFadeDelay);
        yield return StartCoroutine(FadeIn(_settingsCanvasGroup, _buttonFadeTime));

        // start play button pulse after entrance
        if (_playButton != null)
            StartCoroutine(PulseButton(_playButton));
    }

    private IEnumerator FadeAndSlide(CanvasGroup cg, RectTransform rt, float duration)
    {
        if (cg == null) yield break;
        float elapsed = 0;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(50, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smooth = t * t * (3f - 2f * t); // smoothstep
            cg.alpha = smooth;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, smooth);
            yield return null;
        }
        cg.alpha = 1;
        rt.anchoredPosition = endPos;
    }

    private IEnumerator FadeIn(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = elapsed / duration;
            yield return null;
        }
        cg.alpha = 1;
    }

    private IEnumerator PulseButton(RectTransform rt)
    {
        Vector3 original = rt.localScale;
        while (true)
        {
            // scale up
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                rt.localScale = Vector3.Lerp(original, original * 1.04f, t);
                yield return null;
            }
            // scale down
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                rt.localScale = Vector3.Lerp(original * 1.04f, original, t);
                yield return null;
            }
        }
    }

    private T GetOrAdd<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }
}