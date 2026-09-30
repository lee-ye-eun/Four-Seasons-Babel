using TMPro;
using UnityEngine;

public class BabelProgressDisplay : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;

    [Header("Progress Text")]
    [SerializeField] private TextMeshPro progressText;

    [Header("Season Sprites (0~50%)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite springSprite;
    [SerializeField] private Sprite summerSprite;
    [SerializeField] private Sprite autumnSprite;
    [SerializeField] private Sprite winterSprite;

    [Header("Season Sprites (50% 초과)")]
    [SerializeField] private Sprite springSpriteHalf;
    [SerializeField] private Sprite summerSpriteHalf;
    [SerializeField] private Sprite autumnSpriteHalf;
    [SerializeField] private Sprite winterSpriteHalf;

    private void Awake()
    {
        gameStateManager.OnBabelHpChanged += OnBabelHpChanged;
        gameStateManager.OnSeasonChanged += OnSeasonChanged;
    }

    private void Start()
    {
        Refresh(gameStateManager.CurrentBabelHp);
        RefreshSprite(gameStateManager.CurrentSeason, gameStateManager.CurrentBabelHp);
    }

    private void OnDestroy()
    {
        gameStateManager.OnBabelHpChanged -= OnBabelHpChanged;
        gameStateManager.OnSeasonChanged -= OnSeasonChanged;
    }

    private void OnBabelHpChanged(int currentHp)
    {
        Refresh(currentHp);
        RefreshSprite(gameStateManager.CurrentSeason, currentHp);
    }

    private void OnSeasonChanged(Season season) => RefreshSprite(season, gameStateManager.CurrentBabelHp);

    private void Refresh(int currentHp)
    {
        if (progressText == null) return;
        int startHp = gameStateManager.StartingBabelHp;
        int damaged = startHp - currentHp;
        float percent = startHp > 0 ? (float)damaged / startHp * 100f : 0f;
        progressText.text = $"{Mathf.RoundToInt(percent)}%";
    }

    private void RefreshSprite(Season season, int currentHp)
    {
        if (spriteRenderer == null) return;
        int startHp = gameStateManager.StartingBabelHp;
        float percent = startHp > 0 ? (float)(startHp - currentHp) / startHp * 100f : 0f;
        bool isHalf = percent > 50f;

        spriteRenderer.sprite = season switch
        {
            Season.Spring => isHalf ? springSpriteHalf  : springSprite,
            Season.Summer => isHalf ? summerSpriteHalf  : summerSprite,
            Season.Autumn => isHalf ? autumnSpriteHalf  : autumnSprite,
            Season.Winter => isHalf ? winterSpriteHalf  : winterSprite,
            _ => spriteRenderer.sprite
        };
    }
}
