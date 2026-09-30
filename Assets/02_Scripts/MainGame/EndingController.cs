using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EndingController : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private BuildingSO grandTempleSO;

    [Header("Ending Spawn")]
    [SerializeField] private HumanAgent humanPrefab;
    [SerializeField] private WaypointPath waypointPath;
    [SerializeField] private float spawnIntervalStart = 1.0f;  // 처음 스폰 간격
    [SerializeField] private float spawnIntervalEnd   = 0.08f; // 마지막 스폰 간격

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomTargetSize = 1.5f;
    [SerializeField] private float zoomDuration   = 2f;
    [SerializeField] private Ease  zoomEase       = Ease.InOutSine;

    [Header("Shake")]
    [SerializeField] private float shakeDelay    = 2f;
    [SerializeField] private float shakeDuration = 4f;
    [SerializeField] private float shakeStrength = 0.6f;
    [SerializeField] private int   shakeVibrato  = 20;

    [Header("UI")]
    [SerializeField] private GameObject gameClearPanel;
    [SerializeField] private GameObject inputBlocker;
    [SerializeField] private GameObject[] uiToHide;

    private void Start()
    {
        TutorialSignals.OnSignal += OnSignalReceived;
    }

    private void OnDestroy()
    {
        TutorialSignals.OnSignal -= OnSignalReceived;
    }

    private void OnSignalReceived(TutorialSignal signal, BuildingSO so)
    {
        if (so != grandTempleSO) return;
        if (signal != TutorialSignal.BuildingPlacedByDrag &&
            signal != TutorialSignal.BuildingPlacedByClick) return;

        TutorialSignals.OnSignal -= OnSignalReceived;
        StartEnding();
    }

    private void StartEnding()
    {
        if (inputBlocker != null) inputBlocker.SetActive(true);
        foreach (var ui in uiToHide)
            if (ui != null) ui.SetActive(false);

        if (mainCamera == null) mainCamera = Camera.main;

        BuildSpot templeSpot = FindTempleSpot();
        Vector3 camPos    = mainCamera.transform.position;
        Vector3 targetPos = templeSpot != null
            ? new Vector3(templeSpot.transform.position.x,
                          templeSpot.transform.position.y,
                          camPos.z)
            : camPos;

        float totalSpawnTime = zoomDuration + shakeDelay + shakeDuration;
        StartCoroutine(SpawnEndingHumans(totalSpawnTime));

        float step = shakeDuration / 3f;

        DOTween.Sequence()
            .Append(mainCamera.transform.DOMove(targetPos, zoomDuration).SetEase(zoomEase))
            .Join(mainCamera.DOOrthoSize(zoomTargetSize, zoomDuration).SetEase(zoomEase))
            .AppendInterval(shakeDelay)
            .Append(mainCamera.transform.DOShakePosition(step, shakeStrength * 0.3f,  shakeVibrato))
            .Append(mainCamera.transform.DOShakePosition(step, shakeStrength * 0.65f, shakeVibrato))
            .Append(mainCamera.transform.DOShakePosition(step, shakeStrength,         shakeVibrato))
            .AppendCallback(ShowGameClear);
    }

    private IEnumerator SpawnEndingHumans(float duration)
    {
        if (humanPrefab == null || waypointPath == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            var agent = Instantiate(humanPrefab, waypointPath.GetSpawnPoint(), Quaternion.identity);
            agent.Initialize(waypointPath);

            float t        = Mathf.Clamp01(elapsed / duration);
            float interval = Mathf.Lerp(spawnIntervalStart, spawnIntervalEnd, t * t); // 가속 곡선
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private void ShowGameClear()
    {
        StopAllCoroutines();
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private BuildSpot FindTempleSpot()
    {
        foreach (var spot in FindObjectsByType<BuildSpot>(FindObjectsSortMode.None))
        {
            if (spot.InstalledBuilding == grandTempleSO)
                return spot;
        }
        return null;
    }
}
