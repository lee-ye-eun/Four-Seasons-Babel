using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonClockDisplay : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;

    [Header("Season Icon")]
    [SerializeField] private Image seasonIcon;
    [SerializeField] private Sprite springSprite;
    [SerializeField] private Sprite summerSprite;
    [SerializeField] private Sprite autumnSprite;
    [SerializeField] private Sprite winterSprite;

    [Header("Season Text")]
    [SerializeField] private TextMeshProUGUI seasonText;

    private void Awake()
    {
        gameStateManager.OnSeasonChanged += OnSeasonChanged;
        gameStateManager.OnWaveChanged   += OnWaveChanged;
    }

    private void Start()
    {
        RefreshIcon(gameStateManager.CurrentSeason);
        RefreshText(gameStateManager.CurrentWave, gameStateManager.CurrentSeason);
    }

    private void OnDestroy()
    {
        gameStateManager.OnSeasonChanged -= OnSeasonChanged;
        gameStateManager.OnWaveChanged   -= OnWaveChanged;
    }

    private void OnSeasonChanged(Season season)
    {
        RefreshIcon(season);
        RefreshText(gameStateManager.CurrentWave, season);
    }

    private void OnWaveChanged(int wave) => RefreshText(wave, gameStateManager.CurrentSeason);

    private void RefreshIcon(Season season)
    {
        if (seasonIcon == null) return;
        seasonIcon.sprite = season switch
        {
            Season.Spring => springSprite,
            Season.Summer => summerSprite,
            Season.Autumn => autumnSprite,
            Season.Winter => winterSprite,
            _ => seasonIcon.sprite
        };
        seasonIcon.preserveAspect = true;
    }

    private void RefreshText(int wave, Season season)
    {
        if (seasonText == null) return;
        int year      = wave / 8 + 1;
        int seasonNum = wave % 2 + 1;
        string seasonName = season switch
        {
            Season.Spring => "봄",
            Season.Summer => "여름",
            Season.Autumn => "가을",
            Season.Winter => "겨울",
            _ => ""
        };
        seasonText.text = $"{year}년차 {seasonNum}번째 {seasonName}";
    }
}
