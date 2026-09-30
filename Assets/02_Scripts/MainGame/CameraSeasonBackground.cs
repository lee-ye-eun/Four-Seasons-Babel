using UnityEngine;

public class CameraSeasonBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Header("Season Colors")]
    [SerializeField] private Color springColor = new Color(0.67f, 0.89f, 0.74f);
    [SerializeField] private Color summerColor = new Color(0.37f, 0.72f, 0.90f);
    [SerializeField] private Color autumnColor = new Color(0.87f, 0.60f, 0.35f);
    [SerializeField] private Color winterColor = new Color(0.75f, 0.85f, 0.95f);

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Start()
    {
        GameStateManager.Instance.OnSeasonChanged += ApplyColor;
        ApplyColor(GameStateManager.Instance.CurrentSeason);
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnSeasonChanged -= ApplyColor;
    }

    private void ApplyColor(Season season)
    {
        if (targetCamera == null) return;
        targetCamera.backgroundColor = season switch
        {
            Season.Spring => springColor,
            Season.Summer => summerColor,
            Season.Autumn => autumnColor,
            Season.Winter => winterColor,
            _             => targetCamera.backgroundColor,
        };
    }
}
