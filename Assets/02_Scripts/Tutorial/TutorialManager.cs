using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼 진행 전체를 관리한다.
/// 게임 시스템에 직접 접근하지 않고 TutorialSignals 이벤트만 구독한다.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TutorialSequenceSO sequence;
    [SerializeField] private TutorialUITargetRegistry registry;

    [Header("Spotlight Overlays (dark semi-transparent)")]
    [SerializeField] private RectTransform overlayTop;
    [SerializeField] private RectTransform overlayBottom;
    [SerializeField] private RectTransform overlayLeft;
    [SerializeField] private RectTransform overlayRight;

    [Header("Highlight Frame")]
    [SerializeField] private RectTransform highlightFrame;

    [Header("Description Panel")]
    [SerializeField] private RectTransform descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Click-Anywhere Blocker")]
    [SerializeField] private Image blockerImage;
    [SerializeField] private Button blockerButton;

    [Header("Spotlight Animation")]
    [SerializeField] private float spotlightDuration = 0.4f;
    [SerializeField] private Ease spotlightEase = Ease.OutCubic;

    [Header("Tutorial Setup")]
    [SerializeField] private int tutorialStartFaith = 210;
    [SerializeField] private GameObject tutorialCanvasRoot;

    private const string TutorialCompletedKey = "TutorialCompleted";
    private static bool shownThisSession;

    private int currentStepIndex;
    private Canvas rootCanvas;
    private RectTransform canvasRT;
    private Image highlightFrameImage;
    private Image overlayTopImg, overlayBottomImg, overlayLeftImg, overlayRightImg;

    // ---- 초기화 ----

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;
        if (rootCanvas != null) canvasRT = rootCanvas.GetComponent<RectTransform>();
        if (highlightFrame  != null) highlightFrameImage = highlightFrame.GetComponent<Image>();
        if (overlayTop      != null) overlayTopImg       = overlayTop.GetComponent<Image>();
        if (overlayBottom   != null) overlayBottomImg    = overlayBottom.GetComponent<Image>();
        if (overlayLeft     != null) overlayLeftImg      = overlayLeft.GetComponent<Image>();
        if (overlayRight    != null) overlayRightImg     = overlayRight.GetComponent<Image>();
    }

    private void Start()
    {
        if (shownThisSession)
        {
            DestroyTutorialCanvas();
            return;
        }

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.AddFaith(tutorialStartFaith);

        blockerButton.onClick.AddListener(OnBlockerClicked);
        ShowStep(0);
    }

    private void DestroyTutorialCanvas()
    {
        // tutorialCanvasRoot 미할당 시 이 오브젝트의 계층 최상위(Canvas)를 삭제
        GameObject target = tutorialCanvasRoot != null
            ? tutorialCanvasRoot
            : transform.root.gameObject;
        Destroy(target);
    }

    private void OnDestroy()
    {
        TutorialSignals.OnSignal -= OnSignalReceived;
        TutorialSignals.ClearRequiredPlacement();
        TutorialSignals.ClearRequiredDestruction();
    }

    [ContextMenu("Reset Tutorial (Clear PlayerPrefs)")]
    private void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TutorialCompletedKey);
        PlayerPrefs.Save();
        Debug.Log("[TutorialManager] TutorialCompleted 키를 삭제했습니다. 다음 실행부터 튜토리얼이 재생됩니다.");
    }

    // ---- 스텝 표시 ----

    private void ShowStep(int index)
    {
        currentStepIndex = index;
        var step = sequence.steps[index];

        descriptionPanel.gameObject.SetActive(!step.hideDescription);
        descriptionText.text = step.description;

        var targetRT = step.highlightTarget != TutorialTargetKey.None
            ? registry.GetTarget(step.highlightTarget)
            : null;

        if (!step.hideDescription && targetRT != null)
        {
            highlightFrame.gameObject.SetActive(true);
            SetSpotlight(targetRT);
            PositionDescription(targetRT);
        }
        else
        {
            highlightFrame.gameObject.SetActive(false);
            HideOverlays();
            if (!step.hideDescription) CenterDescription();
        }

        if (step.advanceType == TutorialAdvanceType.ClickAnywhere)
        {
            // blockerImage가 전체 화면을 커버 — 오버레이는 시각만 담당, 클릭 흡수 금지
            blockerImage.raycastTarget = true;
            SetOverlayRaycast(false);
            TutorialSignals.OnSignal -= OnSignalReceived;
        }
        else
        {
            // 오버레이가 강조 외 영역 클릭 차단
            blockerImage.raycastTarget = false;
            SetOverlayRaycast(true);
            TutorialSignals.OnSignal -= OnSignalReceived;
            TutorialSignals.OnSignal += OnSignalReceived;
        }

        // 특정 건물만 배치/파괴 허용 (requiredBuilding이 있는 스텝에서만)
        bool hasReq = step.advanceType == TutorialAdvanceType.WaitForSignal
                      && step.requiredBuilding != null;
        TutorialSignals.SetRequiredPlacement(
            hasReq && IsPlacementSignal(step.requiredSignal, step.hasAlternateSignal, step.alternateRequiredSignal)
                ? step.requiredBuilding : null);
        TutorialSignals.SetRequiredDestruction(
            hasReq && IsDestroySignal(step.requiredSignal, step.hasAlternateSignal, step.alternateRequiredSignal)
                ? step.requiredBuilding : null);
    }

    private static bool IsPlacementSignal(TutorialSignal signal, bool hasAlt, TutorialSignal alt)
    {
        static bool IsPl(TutorialSignal s) =>
            s == TutorialSignal.BuildingPlacedByDrag || s == TutorialSignal.BuildingPlacedByClick;
        return IsPl(signal) || (hasAlt && IsPl(alt));
    }

    private static bool IsDestroySignal(TutorialSignal signal, bool hasAlt, TutorialSignal alt)
    {
        static bool IsDe(TutorialSignal s) => s == TutorialSignal.BuildingDestroyed;
        return IsDe(signal) || (hasAlt && IsDe(alt));
    }

    // ---- 진행 ----

    private void OnBlockerClicked()
    {
        if (currentStepIndex >= sequence.steps.Count) return;
        if (sequence.steps[currentStepIndex].advanceType != TutorialAdvanceType.ClickAnywhere) return;
        AdvanceStep();
    }

    private void OnSignalReceived(TutorialSignal signal, BuildingSO so)
    {
        if (currentStepIndex >= sequence.steps.Count) return;
        var step = sequence.steps[currentStepIndex];
        if (step.advanceType != TutorialAdvanceType.WaitForSignal) return;

        bool signalMatch = signal == step.requiredSignal
            || (step.hasAlternateSignal && signal == step.alternateRequiredSignal);

        bool buildingMatch = step.requiredBuilding == null || so == step.requiredBuilding;

        if (signalMatch && buildingMatch)
            AdvanceStep();
    }

    private void AdvanceStep()
    {
        TutorialSignals.OnSignal -= OnSignalReceived;
        TutorialSignals.ClearRequiredPlacement();
        TutorialSignals.ClearRequiredDestruction();

        currentStepIndex++;
        if (currentStepIndex >= sequence.steps.Count)
        {
            shownThisSession = true;
            PlayerPrefs.SetInt(TutorialCompletedKey, 1);
            PlayerPrefs.Save();
            DestroyTutorialCanvas();
            return;
        }

        ShowStep(currentStepIndex);
    }

    // ---- 스포트라이트 레이아웃 ----

    private void SetSpotlight(RectTransform targetRT)
    {
        if (canvasRT == null) return;

        var corners = new Vector3[4];
        targetRT.GetWorldCorners(corners);

        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, corners[0], cam, out var bl);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, corners[2], cam, out var tr);

        var cr = canvasRT.rect;
        float x0 = bl.x, y0 = bl.y, x1 = tr.x, y1 = tr.y;

        overlayTop.gameObject.SetActive(true);
        overlayBottom.gameObject.SetActive(true);
        overlayLeft.gameObject.SetActive(true);
        overlayRight.gameObject.SetActive(true);

        overlayTop.DOKill();
        overlayBottom.DOKill();
        overlayLeft.DOKill();
        overlayRight.DOKill();
        highlightFrame.DOKill();
        if (highlightFrameImage != null) highlightFrameImage.DOKill();

        // 모든 오버레이에 stretch 앵커 적용
        foreach (var rt in new[] { overlayTop, overlayBottom, overlayLeft, overlayRight })
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        // ── Top: 위쪽 끝(제로 높이)에서 y1까지 아래로 펼침 ──
        overlayTop.offsetMin = new Vector2(0f, cr.yMax - cr.yMin);   // bottom = T (높이 0)
        overlayTop.offsetMax = new Vector2(0f, 0f);
        DOTween.To(() => overlayTop.offsetMin,
                   v => overlayTop.offsetMin = v,
                   new Vector2(0f, y1 - cr.yMin),
                   spotlightDuration).SetEase(spotlightEase);

        // ── Bottom: 아래쪽 끝(제로 높이)에서 y0까지 위로 펼침 ──
        overlayBottom.offsetMin = new Vector2(0f, 0f);
        overlayBottom.offsetMax = new Vector2(0f, cr.yMin - cr.yMax); // top = B (높이 0)
        DOTween.To(() => overlayBottom.offsetMax,
                   v => overlayBottom.offsetMax = v,
                   new Vector2(0f, y0 - cr.yMax),
                   spotlightDuration).SetEase(spotlightEase);

        // ── Left: 왼쪽 끝(제로 너비)에서 x0까지 오른쪽으로 펼침 ──
        overlayLeft.offsetMin = new Vector2(0f, y0 - cr.yMin);
        overlayLeft.offsetMax = new Vector2(cr.xMin - cr.xMax, y1 - cr.yMax); // right = L (너비 0)
        DOTween.To(() => overlayLeft.offsetMax,
                   v => overlayLeft.offsetMax = v,
                   new Vector2(x0 - cr.xMax, y1 - cr.yMax),
                   spotlightDuration).SetEase(spotlightEase);

        // ── Right: 오른쪽 끝(제로 너비)에서 x1까지 왼쪽으로 펼침 ──
        overlayRight.offsetMin = new Vector2(cr.xMax - cr.xMin, y0 - cr.yMin); // left = R (너비 0)
        overlayRight.offsetMax = new Vector2(0f, y1 - cr.yMax);
        DOTween.To(() => overlayRight.offsetMin,
                   v => overlayRight.offsetMin = v,
                   new Vector2(x1 - cr.xMin, y0 - cr.yMin),
                   spotlightDuration).SetEase(spotlightEase);

        // ── HighlightFrame: 위치 즉시 설정 후 깜빡임 ──
        SetRect(highlightFrame, x0, y0, x1, y1);
        if (highlightFrameImage != null)
        {
            var c = highlightFrameImage.color;
            highlightFrameImage.color = new Color(c.r, c.g, c.b, 0f);
            highlightFrameImage.DOFade(0.1f, spotlightDuration).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void HideOverlays()
    {
        overlayTop.DOKill();
        overlayBottom.DOKill();
        overlayLeft.DOKill();
        overlayRight.DOKill();
        highlightFrame.DOKill();
        if (highlightFrameImage != null) highlightFrameImage.DOKill();
        overlayTop.gameObject.SetActive(false);
        overlayBottom.gameObject.SetActive(false);
        overlayLeft.gameObject.SetActive(false);
        overlayRight.gameObject.SetActive(false);
    }

    private void SetOverlayRaycast(bool value)
    {
        if (overlayTopImg    != null) overlayTopImg.raycastTarget    = value;
        if (overlayBottomImg != null) overlayBottomImg.raycastTarget = value;
        if (overlayLeftImg   != null) overlayLeftImg.raycastTarget   = value;
        if (overlayRightImg  != null) overlayRightImg.raycastTarget  = value;
    }

    // stretch anchor (0,0)→(1,1) + offsetMin/Max 로 캔버스 로컬 좌표 직접 설정
    // ★ rt 가 Canvas 의 직계 자식이어야 한다 (canvasRT.rect 기준 계산)
    private void SetRect(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        var cr = canvasRT.rect;           // 예: {xMin:-960, yMin:-540, xMax:960, yMax:540}
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(x0 - cr.xMin, y0 - cr.yMin);
        rt.offsetMax = new Vector2(x1 - cr.xMax, y1 - cr.yMax);
    }

    // ---- 설명 패널 위치 ----

    private void PositionDescription(RectTransform _) => CenterDescription();

    private void CenterDescription()
    {
        descriptionPanel.anchorMin = new Vector2(0.2f, 0.35f);
        descriptionPanel.anchorMax = new Vector2(0.8f, 0.65f);
        descriptionPanel.offsetMin = Vector2.zero;
        descriptionPanel.offsetMax = Vector2.zero;
    }
}
