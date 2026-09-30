using System;
using UnityEngine;

public enum Season { Spring, Summer, Autumn, Winter }

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private int startingFaith = 100;
    [SerializeField] private int startingWave = 0;
    [SerializeField] private Season startingSeason = Season.Spring;
    [SerializeField] private int startingBabelHp = 10;

    public event Action<int> OnFaithChanged;
    public event Action<Season> OnSeasonChanged;
    public event Action<int> OnBabelHpChanged;
    public event Action<int> OnWaveChanged;
    public event Action OnGameOver;
    public event Action OnMaintenanceStarted;
    public event Action OnWaveRunStarted;

    public int CurrentFaith => currentFaith;
    public int CurrentWave => currentWave;
    public Season CurrentSeason => currentSeason;
    public int CurrentBabelHp => currentBabelHp;
    public int StartingBabelHp => startingBabelHp;

    private int currentFaith;
    private int currentWave;
    private Season currentSeason;
    [SerializeField] private int currentBabelHp;

    private void Awake()
    {
        Instance = this;
        currentFaith = startingFaith;
        currentWave = startingWave;
        currentSeason = startingSeason;
        currentBabelHp = startingBabelHp;
        Debug.Log($"[GameStateManager] Initialized. BabelHp={currentBabelHp}");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool TrySpend(int cost)
    {
        if (currentFaith < cost) return false;
        currentFaith -= cost;
        OnFaithChanged?.Invoke(currentFaith);
        return true;
    }

    public void AddFaith(int amount)
    {
        currentFaith += amount;
        OnFaithChanged?.Invoke(currentFaith);
    }

    public void SetSeason(Season season)
    {
        currentSeason = season;
        OnSeasonChanged?.Invoke(currentSeason);
    }

    public void StartMaintenance() => OnMaintenanceStarted?.Invoke();
    public void StartWaveRun()    => OnWaveRunStarted?.Invoke();

    public void AdvanceStage()
    {
        currentWave++;
        Season newSeason = (Season)(currentWave / 2 % 4);
        if (newSeason != currentSeason)
            SetSeason(newSeason);
        OnWaveChanged?.Invoke(currentWave);
    }

    public void TakeDamage(int amount)
    {
        currentBabelHp = Mathf.Max(0, currentBabelHp - amount);
        Debug.Log($"[GameStateManager] TakeDamage({amount}) → BabelHp={currentBabelHp}");
        OnBabelHpChanged?.Invoke(currentBabelHp);
        if (currentBabelHp <= 0)
        {
            Debug.Log("[GameStateManager] Game Over!");
            OnGameOver?.Invoke();
        }
    }
}