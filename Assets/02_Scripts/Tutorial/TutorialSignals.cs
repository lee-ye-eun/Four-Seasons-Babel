using System;

public enum TutorialSignal
{
    ConstructionButtonClicked,
    BuildingPlacedByDrag,
    BuildingPlacedByClick,
    RunButtonClicked,
    StageEndShown,
    NextStageButtonClicked,
    DestroyButtonClicked,
    BuildingDestroyed,
    DestroyModeExited
}

public static class TutorialSignals
{
    public static event Action<TutorialSignal, BuildingSO> OnSignal;

    public static void Emit(TutorialSignal signal, BuildingSO so = null)
    {
        OnSignal?.Invoke(signal, so);
    }

    // ---- 건물 배치 제한 ----

    public static BuildingSO RequiredBuildingForPlacement { get; private set; }

    public static void SetRequiredPlacement(BuildingSO so) =>
        RequiredBuildingForPlacement = so;

    public static void ClearRequiredPlacement() =>
        RequiredBuildingForPlacement = null;

    public static bool CanPlace(BuildingSO so)
    {
        if (RequiredBuildingForPlacement == null) return true;
        return so == RequiredBuildingForPlacement;
    }

    // ---- 건물 파괴 제한 ----

    public static BuildingSO RequiredBuildingForDestruction { get; private set; }

    public static void SetRequiredDestruction(BuildingSO so) =>
        RequiredBuildingForDestruction = so;

    public static void ClearRequiredDestruction() =>
        RequiredBuildingForDestruction = null;

    public static bool CanDestroy(BuildingSO so)
    {
        if (RequiredBuildingForDestruction == null) return true;
        return so == RequiredBuildingForDestruction;
    }
}
