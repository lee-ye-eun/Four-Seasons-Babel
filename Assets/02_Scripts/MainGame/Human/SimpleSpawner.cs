using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [SerializeField] private HumanAgent humanPrefab;
    [SerializeField] private WaypointPath waypointPath;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int baseSpawnCount = 5;
    [SerializeField] private float spawnMultiplier = 1.2f;

    [Header("Spawn Weight")]
    [SerializeField] private float spawnWeightMin = 0.9f;
    [SerializeField] private float spawnWeightMax = 1.5f;

    [Header("Exit Skip")]
    [SerializeField] private float skipSpeedMultiplier = 10f;

    [Header("Speed Control")]
    [SerializeField] private WaveSpeedPanel waveSpeedPanel;

    public event Action OnAllHumansDespawned;

    public int LastSpawnCount => maxSpawnCount;

    private float timer;
    private int spawnedCount;
    private int activeHumanCount;
    private int maxSpawnCount;
    private bool isRunning;

    private readonly List<HumanAgent> activeAgents = new();

    // Run 버튼 클릭 시 호출 — 가중치와 인원을 확정하고 반환한다.
    // 실제 웨이브 시작은 StartStage()가 별도로 담당한다.
    public (float weight, int count) PrepareStage()
    {
        int wave = GameStateManager.Instance != null ? GameStateManager.Instance.CurrentWave : 0;
        int baseCount = Mathf.RoundToInt(baseSpawnCount * Mathf.Pow(spawnMultiplier, wave));
        float weight = UnityEngine.Random.Range(spawnWeightMin, spawnWeightMax);
        maxSpawnCount = Mathf.RoundToInt(baseCount * weight);
        return (weight, maxSpawnCount);
    }

    // PrepareStage() 호출 후 사용자 확인 시 호출 — 웨이브를 실제로 시작한다.
    public void StartStage()
    {
        spawnedCount = 0;
        activeHumanCount = 0;
        activeAgents.Clear();
        timer = 0f;
        isRunning = true;
        if (waveSpeedPanel != null) waveSpeedPanel.Show();
        GameStateManager.Instance.StartWaveRun();
        Debug.Log($"[SimpleSpawner] Stage started. SpawnCount={maxSpawnCount}");
    }

    // Run 버튼 위 범위 텍스트 표시용
    public void GetSpawnCountRange(out int min, out int max)
    {
        int wave = GameStateManager.Instance != null ? GameStateManager.Instance.CurrentWave : 0;
        int baseCount = Mathf.RoundToInt(baseSpawnCount * Mathf.Pow(spawnMultiplier, wave));
        min = Mathf.RoundToInt(baseCount * spawnWeightMin);
        max = Mathf.RoundToInt(baseCount * spawnWeightMax);
    }

    public void NotifyBuildingOccupancyChanged() => TryFireStageEnd();

    private void Update()
    {
        if (!isRunning) return;
        if (spawnedCount >= maxSpawnCount) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnHuman();
        }
    }

    private void SpawnHuman()
    {
        Vector3 spawnPosition = waypointPath.GetSpawnPoint();
        HumanAgent agent = Instantiate(humanPrefab, spawnPosition, Quaternion.identity);
        agent.name = $"Human_{spawnedCount}";
        agent.Initialize(waypointPath);
        agent.OnReachedDestination += OnHumanDespawned;
        agent.OnEnteredBuilding += OnAgentEnteredBuilding;

        activeAgents.Add(agent);
        spawnedCount++;
        activeHumanCount++;
    }

    private void OnHumanDespawned()
    {
        activeHumanCount--;
        activeAgents.RemoveAll(a => a == null);
        Debug.Log($"[SimpleSpawner] Human despawned. spawned={spawnedCount}/{maxSpawnCount}, active={activeHumanCount}");
        TrySkipBuildingUsers();
        TryFireStageEnd();
    }

    private void OnAgentEnteredBuilding()
    {
        activeAgents.RemoveAll(a => a == null);
        TrySkipBuildingUsers();
    }

    // 남은 인간이 전원 건물 안이거나 완료(Despawned)일 때 모든 건물 타이머 배속
    private void TrySkipBuildingUsers()
    {
        if (spawnedCount < maxSpawnCount) return;

        foreach (var agent in activeAgents)
        {
            if (!agent.IsInsideBuilding && agent.CurrentState != AgentState.Despawned) return;
        }

        foreach (var spot in FindObjectsByType<BuildSpot>(FindObjectsSortMode.None))
            spot.SetUsageSpeedMultiplier(skipSpeedMultiplier);
    }

    private void TryFireStageEnd()
    {
        if (!isRunning)                   { Debug.Log("[StageEnd] BLOCKED: isRunning=false"); return; }
        if (spawnedCount < maxSpawnCount) { Debug.Log($"[StageEnd] BLOCKED: spawned={spawnedCount} < max={maxSpawnCount}"); return; }
        if (activeHumanCount > 0)         { Debug.Log($"[StageEnd] BLOCKED: activeHuman={activeHumanCount}"); return; }

        foreach (var spot in FindObjectsByType<BuildSpot>(FindObjectsSortMode.None))
        {
            if (spot.CurrentOccupants > 0)
            {
                Debug.Log($"[StageEnd] BLOCKED: {spot.name} occupants={spot.CurrentOccupants}");
                return;
            }
        }

        isRunning = false;
        if (waveSpeedPanel != null) waveSpeedPanel.Hide();

        int babelHp = GameStateManager.Instance != null ? GameStateManager.Instance.CurrentBabelHp : -1;
        if (babelHp <= 0) { Debug.Log($"[StageEnd] BLOCKED: BabelHp={babelHp}"); return; }

        Debug.Log("[StageEnd] All conditions passed → OnAllHumansDespawned fired");
        Debug.Log($"[StageEnd] Subscriber count: {OnAllHumansDespawned?.GetInvocationList().Length ?? 0}");
        OnAllHumansDespawned?.Invoke();
    }
}
