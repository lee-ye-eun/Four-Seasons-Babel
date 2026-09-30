using TMPro;
using UnityEngine;

public class MaintenanceButtonsUI : MonoBehaviour
{
    [SerializeField] private ConstructionModeController controller;
    [SerializeField] private GameObject constructionPanel;
    [SerializeField] private SimpleSpawner spawner;
    [SerializeField] private GameObject maintenancePanel;
    [SerializeField] private GameObject destroyText;

    [Header("Spawn Range")]
    [SerializeField] private TextMeshProUGUI spawnRangeText;

    [Header("Weight Result Panel")]
    [SerializeField] private SpawnWeightResultPanel weightResultPanel;

    private void OnEnable()
    {
        if (destroyText != null) destroyText.SetActive(false);
        constructionPanel.SetActive(false);
        RefreshRangeText();
    }

    private void RefreshRangeText()
    {
        if (spawnRangeText == null) return;
        spawner.GetSpawnCountRange(out int min, out int max);
        spawnRangeText.text = $"{min} ~ {max}명";
    }

    public void OnRunButtonClicked()
    {
        TutorialSignals.Emit(TutorialSignal.RunButtonClicked);
        controller.ExitBuildMode();
        constructionPanel.SetActive(false);
        maintenancePanel.SetActive(false);

        var (weight, count) = spawner.PrepareStage();
        weightResultPanel.Show(weight, count, () => spawner.StartStage());
    }

    public void OnBuildButtonClicked()
    {
        if (destroyText != null) destroyText.SetActive(false);

        if (constructionPanel.activeSelf)
        {
            controller.ExitBuildMode();
            constructionPanel.SetActive(false);
        }
        else
        {
            controller.EnterBuildMode();
            constructionPanel.SetActive(true);
        }
    }

    public void OnStoreButtonClicked()
    {
        // TODO: 보관 패널 활성화
        Debug.Log("[UI] Store mode — not yet implemented.");
        controller.EnterStoringMode();
    }

    public void OnDestroyButtonClicked()
    {
        constructionPanel.SetActive(false);
        controller.ToggleDestroyMode();
        if (destroyText != null)
            destroyText.SetActive(controller.CurrentMode == ConstructionMode.Destroying);
    }
}
