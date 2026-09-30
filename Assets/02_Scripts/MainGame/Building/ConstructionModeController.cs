using UnityEngine;

public enum ConstructionMode { None, Storing, Building, Destroying }

public class ConstructionModeController : MonoBehaviour
{
    public static ConstructionModeController Instance { get; private set; }

    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private AudioClip placeSFX;

    public ConstructionMode CurrentMode { get; private set; } = ConstructionMode.None;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    public void EnterBuildMode()
    {
        CurrentMode = ConstructionMode.Building;
        TutorialSignals.Emit(TutorialSignal.ConstructionButtonClicked);
        Debug.Log("[Construction] Build mode activated.");
    }

    public void ExitBuildMode()
    {
        CurrentMode = ConstructionMode.None;
    }

    public void EnterStoringMode()
    {
        // TODO: 보관 모드 동작 구현
        CurrentMode = ConstructionMode.Storing;
        Debug.Log("[Construction] Storing mode — not yet implemented.");
    }

    public void ToggleDestroyMode()
    {
        if (CurrentMode == ConstructionMode.Destroying)
        {
            CurrentMode = ConstructionMode.None;
            TutorialSignals.Emit(TutorialSignal.DestroyModeExited);
            Debug.Log("[Construction] Destroy mode deactivated.");
        }
        else
        {
            CurrentMode = ConstructionMode.Destroying;
            TutorialSignals.Emit(TutorialSignal.DestroyButtonClicked);
            Debug.Log("[Construction] Destroy mode activated.");
        }
    }

    public bool TryPlace(BuildSpot spot, BuildingSO so, bool fromDrag = true)
    {
        if (spot == null || so == null) return false;

        if (!TutorialSignals.CanPlace(so))
        {
            var req = TutorialSignals.RequiredBuildingForPlacement;
            Debug.Log($"[Tutorial] {so.buildingName}은(는) 지금 배치할 수 없습니다. " +
                      $"필요 건물: {(req != null ? req.buildingName : "없음")}");
            return false;
        }

        if (spot.HasBuilding)
        {
            Debug.Log("[Construction] Spot already occupied.");
            return false;
        }

        if (!gameStateManager.TrySpend(so.cost))
        {
            Debug.Log($"[Construction] Not enough faith. Need {so.cost}, have {gameStateManager.CurrentFaith}.");
            return false;
        }

        spot.TryInstall(so);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(placeSFX);
        TutorialSignals.Emit(fromDrag ? TutorialSignal.BuildingPlacedByDrag : TutorialSignal.BuildingPlacedByClick, so);
        Debug.Log($"[Construction] Placed {so.buildingName} (-{so.cost} faith). Remaining: {gameStateManager.CurrentFaith}.");
        return true;
    }
}
