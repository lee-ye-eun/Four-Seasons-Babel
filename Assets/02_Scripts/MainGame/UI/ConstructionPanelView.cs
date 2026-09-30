using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionPanelView : MonoBehaviour
{
    [SerializeField] private BuildingCatalog catalog;
    [SerializeField] private BuildingItemView itemPrefab;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject prevButton;
    [SerializeField] private GameObject nextButton;

    public event Action<BuildingSO> OnHoverStart;
    public event Action OnHoverEnd;
    public event Action OnDragBegan;
    public event Action OnDragFinished;

    private const int ItemsPerPage = 5;
    private readonly List<BuildingItemView> allItems = new();
    private int currentPage;

    private void Awake()
    {
        CreateAllItems();
    }

    private void OnEnable()
    {
        ShowPage(0);
    }

    private void CreateAllItems()
    {
        foreach (var so in catalog.buildings)
        {
            BuildingItemView item = Instantiate(itemPrefab, itemContainer);
            item.Setup(so);
            item.OnHoverStart += building => OnHoverStart?.Invoke(building);
            item.OnHoverEnd += () => OnHoverEnd?.Invoke();
            item.OnDragBegan += () => OnDragBegan?.Invoke();
            item.OnDragFinished += () => OnDragFinished?.Invoke();
            item.gameObject.SetActive(false);
            allItems.Add(item);
        }
    }

    private void ShowPage(int page)
    {
        currentPage = page;
        int start = currentPage * ItemsPerPage;

        for (int i = 0; i < allItems.Count; i++)
            allItems[i].gameObject.SetActive(i >= start && i < start + ItemsPerPage);

        bool hasPaging = allItems.Count > ItemsPerPage;
        if (prevButton != null) prevButton.SetActive(hasPaging && currentPage > 0);
        if (nextButton != null) nextButton.SetActive(hasPaging && start + ItemsPerPage < allItems.Count);
    }

    public void OnPrevPageClicked() => ShowPage(currentPage - 1);
    public void OnNextPageClicked() => ShowPage(currentPage + 1);
}
