using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BuildingItemView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;

    public event Action<BuildingSO> OnHoverStart;
    public event Action OnHoverEnd;
    public event Action OnDragBegan;
    public event Action OnDragFinished;

    private BuildingSO buildingSO;
    private GameObject ghostIcon;
    private Canvas rootCanvas;

    public void Setup(BuildingSO so)
    {
        buildingSO = so;
        if (so.icon != null)
        {
            iconImage.sprite = so.icon;
            iconImage.preserveAspect = true;
        }
        nameText.text = so.buildingName;
        costText.text = so.cost.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData) => OnHoverStart?.Invoke(buildingSO);
    public void OnPointerExit(PointerEventData eventData) => OnHoverEnd?.Invoke();

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnDragBegan?.Invoke();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        ghostIcon = new GameObject("GhostIcon");
        ghostIcon.transform.SetParent(rootCanvas.transform, false);

        var img = ghostIcon.AddComponent<Image>();
        img.sprite = buildingSO.icon;
        img.raycastTarget = false;
        img.preserveAspect = true;

        var rt = ghostIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60f, 60f);
        rt.anchorMin = rt.anchorMax = Vector2.one * 0.5f;
        rt.pivot = Vector2.one * 0.5f;

        MoveGhostToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveGhostToPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);
            ghostIcon = null;
        }

        OnDragFinished?.Invoke();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null)
        {
            BuildSpot spot = hit.GetComponent<BuildSpot>();
            if (spot != null && ConstructionModeController.Instance != null)
                ConstructionModeController.Instance.TryPlace(spot, buildingSO);
        }
    }

    private void MoveGhostToPointer(PointerEventData eventData)
    {
        if (ghostIcon == null || rootCanvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        ghostIcon.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }
}
