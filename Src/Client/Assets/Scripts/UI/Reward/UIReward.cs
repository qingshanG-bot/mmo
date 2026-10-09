using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIReward : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public Text countText;
    public CanvasGroup canvasGroup;

    public RectTransform rect;

    public float slideDistance = 80f;   // 滑入位移
    public float slideTime = 0.2f;      // 滑入时间
    public float holdTime = 1.2f;       // 停留时间
    public float fadeTime = 0.25f;      // 淡出时间

    Vector2 _startAnchoredPos;

    public void Setup(Sprite spIcon, string itemName, int count)
    {
        if (icon) icon.overrideSprite = spIcon;
        if (nameText) nameText.text = itemName ?? "";
        if (countText) countText.text = count > 1 ? $"×{count}" : "";
    }

    /// <summary> 在父物体下以动画方式展示；调用完会自动回收到对象池（或销毁） </summary>
    public void PlayAndAutoRecycle(System.Action<UIReward> onComplete)
    {
        StopAllCoroutines();
        StartCoroutine(Co_Play(onComplete));
    }

    IEnumerator Co_Play(System.Action<UIReward> onComplete)
    {
        // ⭐ 等一帧，让 VerticalLayoutGroup 把根节点排好位置，再去拿起始坐标
        yield return null;

        if (canvasGroup) canvasGroup.alpha = 0f;

        _startAnchoredPos = rect.anchoredPosition;

        // 从左侧滑入：只改 X，让 Y 保持 LayoutGroup 给的值
        Vector2 from = _startAnchoredPos + new Vector2(-slideDistance, 0f);
        Vector2 to = _startAnchoredPos;
        rect.anchoredPosition = from;

        float t = 0f;
        while (t < slideTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / slideTime);
            rect.anchoredPosition = Vector2.Lerp(from, to, k);
            if (canvasGroup) canvasGroup.alpha = k;
            yield return null;
        }
        rect.anchoredPosition = to;
        if (canvasGroup) canvasGroup.alpha = 1f;

        // 中间停留
        yield return new WaitForSecondsRealtime(holdTime);

        // 往上轻轻飘一点再消失
        Vector2 outFrom = to;
        Vector2 outTo = to + new Vector2(0f, 20f);
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            rect.anchoredPosition = Vector2.Lerp(outFrom, outTo, k);
            if (canvasGroup) canvasGroup.alpha = 1f - k;
            yield return null;
        }

        if (canvasGroup) canvasGroup.alpha = 0f;
        onComplete?.Invoke(this);
    }
}
