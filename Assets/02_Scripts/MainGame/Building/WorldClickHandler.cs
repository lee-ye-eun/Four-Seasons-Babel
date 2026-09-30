using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldClickHandler : MonoBehaviour
{
    private static readonly ContactFilter2D NonTriggerFilter = CreateNonTriggerFilter();

    private static ContactFilter2D CreateNonTriggerFilter()
    {
        var f = ContactFilter2D.noFilter;
        f.useTriggers = false;
        return f;
    }

    [SerializeField] private GameObject constructionPanel;

    private readonly List<Collider2D> hitBuffer = new();

    private void Update()
    {
        if (ConstructionModeController.Instance == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        ConstructionMode mode = ConstructionModeController.Instance.CurrentMode;

        if (mode == ConstructionMode.Building)
            HandleBuildClick();
        else if (mode == ConstructionMode.Destroying)
            HandleDestroyClick();
    }

    private void HandleBuildClick()
    {
        // 건설 스폿이 아닌 빈 공간 클릭 → 건설 모드 종료
        if (OverlapNonTrigger() == null)
        {
            ConstructionModeController.Instance.ExitBuildMode();
            if (constructionPanel != null) constructionPanel.SetActive(false);
        }
    }

    private void HandleDestroyClick()
    {
        BuildSpot spot = OverlapBuildSpot(ContactFilter2D.noFilter);
        if (spot != null && spot.HasBuilding)
        {
            spot.RemoveBuilding();
            Debug.Log("[WorldClick] Building removed from spot.");
        }
    }

    private BuildSpot OverlapNonTrigger()
    {
        return OverlapBuildSpot(NonTriggerFilter);
    }

    private BuildSpot OverlapBuildSpot(ContactFilter2D filter)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        hitBuffer.Clear();
        Physics2D.OverlapPoint(worldPos, filter, hitBuffer);

        foreach (var col in hitBuffer)
        {
            BuildSpot spot = col.GetComponentInParent<BuildSpot>();
            if (spot != null) return spot;
        }
        return null;
    }
}
