using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class LinkOpener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Camera eventCamera; // Overlay 模式可留空；Screen Space - Camera / World Space 请指定对应相机
    private TMP_Text tmp;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tmp == null) return;

        // 用事件位置 + 正确的相机
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmp, eventData.position, eventCamera);

        // 未点中链接时返回 -1（不是 1）
        if (linkIndex == -1) return;

        var textInfo = tmp.textInfo;
        if (linkIndex < 0 || linkIndex >= textInfo.linkCount) return; // 再保险

        var linkInfo = textInfo.linkInfo[linkIndex];
        string linkId = linkInfo.GetLinkID();
        if (string.IsNullOrEmpty(linkId)) return;

        // 仅切两段：id:name，避免越界
        var parts = linkId.Split(new[] { ':' }, 2);
        if (parts.Length < 2)
        {
            Debug.LogWarning($"Bad link id: '{linkId}'. Expected format 'id:name'");
            return;
        }

        if (!int.TryParse(parts[0], out int targetId))
        {
            Debug.LogWarning($"Invalid id in link: '{parts[0]}'");
            return;
        }
        string targetName = parts[1];

        var menu = UIManager.Instance.Show<UIPopCharMenu>();
        if (menu == null)
        {
            Debug.LogWarning("UIPopCharMenu is null.");
            return;
        }

        menu.targetId = targetId;
        menu.targetName = targetName;
    }

    public void OnPointerDown(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
}
