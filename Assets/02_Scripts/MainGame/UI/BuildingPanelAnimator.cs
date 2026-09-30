using DG.Tweening;
using UnityEngine;

public class BuildingPanelAnimator : MonoBehaviour
{
    [SerializeField] private ConstructionPanelView panelView;
    [SerializeField] private float slideDistance = 300f;
    [SerializeField] private float animDuration = 0.25f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private RectTransform rectTransform;
    private float shownY;
    private float hiddenY;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        shownY = rectTransform.anchoredPosition.y;
        hiddenY = shownY - slideDistance;

        panelView.OnDragBegan += OnDragBegan;
        panelView.OnDragFinished += OnDragFinished;
    }

    private void OnDestroy()
    {
        if (panelView != null)
        {
            panelView.OnDragBegan -= OnDragBegan;
            panelView.OnDragFinished -= OnDragFinished;
        }
    }

    private void OnDragBegan() => Slide(hiddenY);
    private void OnDragFinished() => Slide(shownY);

    private void Slide(float targetY)
    {
        rectTransform.DOKill();
        rectTransform.DOAnchorPosY(targetY, animDuration).SetEase(ease);
    }
}
