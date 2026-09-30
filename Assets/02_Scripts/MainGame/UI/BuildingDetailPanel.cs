using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingDetailPanel : MonoBehaviour
{
    [SerializeField] private ConstructionPanelView panelView;

    [Header("Info")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI useTimeText;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Season Bars")]
    [SerializeField] private RectTransform springBar;
    [SerializeField] private RectTransform summerBar;
    [SerializeField] private RectTransform autumnBar;
    [SerializeField] private RectTransform winterBar;
    [SerializeField] private float maxBarHeight = 200f;
    [SerializeField] private int maxCapacity = 200;

    [Header("Season Bar Labels")]
    [SerializeField] private TextMeshProUGUI springCapacityText;
    [SerializeField] private TextMeshProUGUI summerCapacityText;
    [SerializeField] private TextMeshProUGUI autumnCapacityText;
    [SerializeField] private TextMeshProUGUI winterCapacityText;

    private void Awake()
    {
        panelView.OnHoverStart += Show;
        panelView.OnHoverEnd += Hide;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        panelView.OnHoverStart -= Show;
        panelView.OnHoverEnd -= Hide;
    }

    private void Show(BuildingSO so)
    {
        gameObject.SetActive(true);
        iconImage.sprite = so.icon;
        iconImage.preserveAspect = true;
        nameText.text = so.buildingName;
        descriptionText.text = so.description;
        useTimeText.text = $"{so.useTime}s";
        costText.text = $"{so.cost}";
        RefreshBars(so);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void RefreshBars(BuildingSO so)
    {
        SetBar(springBar, springCapacityText, so.capacitySpring);
        SetBar(summerBar, summerCapacityText, so.capacitySummer);
        SetBar(autumnBar, autumnCapacityText, so.capacityAutumn);
        SetBar(winterBar, winterCapacityText, so.capacityWinter);
    }

    private void SetBar(RectTransform bar, TextMeshProUGUI label, int capacity)
    {
        if (bar != null)
        {
            bar.pivot = new Vector2(0.5f, 0f);
            Vector2 size = bar.sizeDelta;
            size.y = maxCapacity > 0 ? (capacity / (float)maxCapacity) * maxBarHeight : 0f;
            bar.sizeDelta = size;
        }

        if (label != null)
            label.text = capacity.ToString();
    }
}
