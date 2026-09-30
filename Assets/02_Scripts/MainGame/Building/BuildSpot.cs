using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildSpot : MonoBehaviour
{
    [SerializeField] private BuildingSO installedBuilding;
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private float visualSize = 1f;
    [SerializeField] private TextMeshPro occupancyText;
    [SerializeField] private GameObject interactionAreaVisual;


    private readonly List<HumanAgent> waitingQueue = new();
    private readonly List<HumanAgent> usageQueue = new();
    private int currentOccupants;

    private float usageTimer;
    private bool isProcessingUsage;
    private float usageSpeedMultiplier = 1f;

    public bool HasBuilding => installedBuilding != null;
    public BuildingSO InstalledBuilding => installedBuilding;
    public int CurrentOccupants => currentOccupants;

    private Vector3 visualAnchor;   // 에디터에서 설정한 visualRenderer의 설계 위치

    private void Awake()
    {
        if (occupancyText != null) occupancyText.gameObject.SetActive(false);
        if (visualRenderer != null)
            visualAnchor = visualRenderer.transform.localPosition;
    }

    private void Start()
    {
        var gsm = GameStateManager.Instance;
        gsm.OnWaveChanged        += OnWaveChanged;
        gsm.OnMaintenanceStarted += ShowInteractionArea;
        gsm.OnWaveRunStarted     += HideInteractionArea;
        gsm.OnGameOver           += HideInteractionArea;
        // 게임 시작 직후는 정비 타임이므로 즉시 표시
        if (interactionAreaVisual != null) interactionAreaVisual.SetActive(true);
        if (HasBuilding) RefreshSprite();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance == null) return;
        var gsm = GameStateManager.Instance;
        gsm.OnWaveChanged        -= OnWaveChanged;
        gsm.OnMaintenanceStarted -= ShowInteractionArea;
        gsm.OnWaveRunStarted     -= HideInteractionArea;
        gsm.OnGameOver           -= HideInteractionArea;
    }

    // ---- 건설 / 철거 ----

    public bool TryInstall(BuildingSO so)
    {
        if (HasBuilding) return false;
        installedBuilding = so;
        RefreshSprite();
        RefreshOccupancyText();
        return true;
    }

    public void RemoveBuilding()
    {
        if (!HasBuilding) return;
        if (!TutorialSignals.CanDestroy(installedBuilding))
        {
            var req = TutorialSignals.RequiredBuildingForDestruction;
            Debug.Log($"[Tutorial] {installedBuilding.buildingName}은(는) 지금 파괴할 수 없습니다. " +
                      $"필요 건물: {(req != null ? req.buildingName : "없음")}");
            return;
        }

        var removed = installedBuilding;
        waitingQueue.Clear();
        usageQueue.Clear();
        isProcessingUsage = false;
        currentOccupants = 0;
        installedBuilding = null;
        if (visualRenderer != null)
            visualRenderer.sprite = null;
        RefreshOccupancyText();
        if (removed != null)
            TutorialSignals.Emit(TutorialSignal.BuildingDestroyed, removed);
        // TODO: 이미 입장해 이용 중인 인간 처리 — 이번 범위 밖
    }

    // ---- 이용 큐 ----

    private void Update()
    {
        if (!isProcessingUsage) return;

        usageTimer -= Time.deltaTime * usageSpeedMultiplier;
        if (usageTimer <= 0f)
            ProcessNextInUsageQueue();
    }

    public void SetUsageSpeedMultiplier(float multiplier) => usageSpeedMultiplier = multiplier;

    public void EnqueueForUsage(HumanAgent agent)
    {
        usageQueue.Add(agent);
        if (!isProcessingUsage)
            StartUsageTimer();
    }

    private void StartUsageTimer()
    {
        usageTimer = installedBuilding.useTime;
        isProcessingUsage = true;
    }

    private void ProcessNextInUsageQueue()
    {
        if (usageQueue.Count == 0) { isProcessingUsage = false; return; }

        HumanAgent agent = usageQueue[0];
        usageQueue.RemoveAt(0);

        currentOccupants--;
        RefreshOccupancyText();

        if (installedBuilding != null && installedBuilding.faithProduction != 0)
            GameStateManager.Instance.AddFaith(installedBuilding.faithProduction);

        TryAdmitNext();

        var spawner = FindFirstObjectByType<SimpleSpawner>();
        if (spawner != null) spawner.NotifyBuildingOccupancyChanged();

        if (agent != null && agent.CurrentState != AgentState.Despawned)
            agent.CompleteUsage();

        if (usageQueue.Count > 0)
            StartUsageTimer();
        else
            isProcessingUsage = false;
    }

    // ---- 트리거 ----

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasBuilding) return;
        if (!other.TryGetComponent(out HumanAgent agent)) return;
        if (agent.CurrentState == AgentState.UsingFacility) return;
        if (waitingQueue.Contains(agent)) return;

        waitingQueue.Add(agent);
        TryAdmitNext();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out HumanAgent agent)) return;
        // 이미 입장 확정(UsingFacility)된 인간은 제거하지 않음
        if (agent.CurrentState != AgentState.UsingFacility)
            waitingQueue.Remove(agent);
    }

    // ---- 입장 / 퇴장 ----

    private void TryAdmitNext()
    {
        if (!HasBuilding) return;
        int capacity = installedBuilding.GetCapacity(GameStateManager.Instance.CurrentSeason);

        while (currentOccupants < capacity && waitingQueue.Count > 0)
        {
            HumanAgent next = waitingQueue[0];
            waitingQueue.RemoveAt(0);

            // 파괴 예정이거나 이미 다른 건물 이용 중인 인간은 건너뜀
            if (next == null || next.CurrentState == AgentState.Despawned || next.CurrentState == AgentState.UsingFacility)
            {
                Debug.Log($"[BuildSpot] {name} → skipped agent in queue (state={(next != null ? next.CurrentState.ToString() : "null")})");
                continue;
            }

            currentOccupants++;
            next.EnterFacility(this);
            RefreshOccupancyText();
        }
    }

    // ---- 스테이지 전환 ----

    private void ShowInteractionArea()
    {
        if (interactionAreaVisual != null) interactionAreaVisual.SetActive(true);
    }

    private void HideInteractionArea()
    {
        if (interactionAreaVisual != null) interactionAreaVisual.SetActive(false);
    }

    private void OnWaveChanged(int wave)
    {
        _ = wave;
        usageSpeedMultiplier = 1f;
        if (!HasBuilding) return;
        RefreshSprite();
        RefreshOccupancyText();
        // TODO: 정원이 0이 된 계절로 전환 시 대기 중인 인간 타임아웃 처리 — 이번 범위 밖
    }

    // ---- 비주얼 ----

    private void RefreshSprite()
    {
        if (visualRenderer == null || installedBuilding == null) return;
        visualRenderer.sprite = GetSeasonSprite(GameStateManager.Instance.CurrentSeason);
        NormalizeVisualSize();
    }

    private void NormalizeVisualSize()
    {
        if (visualRenderer.sprite == null) return;

        Sprite s = visualRenderer.sprite;
        float w = s.rect.width / s.pixelsPerUnit;
        float h = s.rect.height / s.pixelsPerUnit;
        float maxDim = Mathf.Max(w, h);
        if (maxDim <= 0f) return;

        float worldScale = visualSize / maxDim;

        // 부모 스케일이 불균등할 경우에도 월드 스페이스에서 균등하게 보이도록 보정
        Transform t = visualRenderer.transform;
        Vector3 parentLossy = t.parent ? t.parent.lossyScale : Vector3.one;
        float lx = parentLossy.x > 0f ? worldScale / parentLossy.x : worldScale;
        float ly = parentLossy.y > 0f ? worldScale / parentLossy.y : worldScale;
        t.localScale = new Vector3(lx, ly, 1f);

        // 이미지 하단 중앙이 visualAnchor(에디터 설계 위치)에 오도록 보정
        float pivotX = s.pivot.x / s.pixelsPerUnit;
        float pivotY = s.pivot.y / s.pixelsPerUnit;
        t.localPosition = new Vector3(
            visualAnchor.x + (pivotX - w * 0.5f) * lx,
            visualAnchor.y + pivotY * ly,
            visualAnchor.z
        );
    }

    private void RefreshOccupancyText()
    {
        if (occupancyText == null) return;
        if (!HasBuilding)
        {
            occupancyText.gameObject.SetActive(false);
            return;
        }
        int capacity = installedBuilding.GetCapacity(GameStateManager.Instance.CurrentSeason);
        occupancyText.text = $"{currentOccupants}/{capacity}";
        occupancyText.gameObject.SetActive(true);
    }

    private Sprite GetSeasonSprite(Season season) => season switch
    {
        Season.Spring => installedBuilding.springVisualSprite,
        Season.Summer => installedBuilding.summerVisualSprite,
        Season.Autumn => installedBuilding.autumnVisualSprite,
        Season.Winter => installedBuilding.winterVisualSprite,
        _ => null
    };
}
