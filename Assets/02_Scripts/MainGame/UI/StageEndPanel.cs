using DG.Tweening;
using UnityEngine;

public class StageEndPanel : MonoBehaviour
{
    [SerializeField] private SimpleSpawner spawner;
    [SerializeField] private GameObject maintenancePanel;
    [SerializeField] private GameObject buildingPanel;

    [Header("Clock")]
    [SerializeField] private Transform clock;
    [SerializeField] private float clockAnimDuration = 0.5f;
    [SerializeField] private Ease clockEase = Ease.OutCubic;
    [SerializeField] private AudioClip clockSFX;

    [Header("Clock Hand")]
    [SerializeField] private Transform clockHand;
    [SerializeField] private float handAnimDuration = 0.6f;
    [SerializeField] private Ease handEase = Ease.OutBack;
    [SerializeField] private float handStartAngle = -25f;
    [SerializeField] private AudioClip handSFX;

    [Header("Clock Audio")]
    [SerializeField] private AudioSource clockAudioSource;

    private float handAngle;

    private void Awake()
    {
        spawner.OnAllHumansDespawned += Show;
        gameObject.SetActive(false);
        handAngle = handStartAngle;

        if (clockAudioSource == null)
            clockAudioSource = gameObject.AddComponent<AudioSource>();
        clockAudioSource.playOnAwake = false;
    }

    private void OnDestroy()
    {
        spawner.OnAllHumansDespawned -= Show;
    }

    private void Show()
    {
        gameObject.SetActive(true);
        GameStateManager.Instance.AddFaith(spawner.LastSpawnCount);
        TutorialSignals.Emit(TutorialSignal.StageEndShown);
        if (AudioManager.Instance != null) AudioManager.Instance.DuckBGM();
        AnimateClock();
    }

    public void OnConfirmButtonClicked()
    {
        TutorialSignals.Emit(TutorialSignal.NextStageButtonClicked);
        GameStateManager.Instance.AdvanceStage();
        GameStateManager.Instance.StartMaintenance();
        if (ConstructionModeController.Instance != null)
            ConstructionModeController.Instance.ExitBuildMode();
        if (AudioManager.Instance != null) AudioManager.Instance.RestoreBGM();
        gameObject.SetActive(false);
        buildingPanel.SetActive(false);
        maintenancePanel.SetActive(true);
    }

    private void AnimateClock()
    {
        if (clock == null) { AnimateClockHand(); return; }

        clock.DOKill();
        clock.localEulerAngles = new Vector3(0f, 0f, -110f);
        clock.DOLocalRotate(Vector3.zero, clockAnimDuration)
             .SetEase(clockEase)
             .OnStart(() => PlayClockSFX(clockSFX))
             .OnComplete(() => { StopClockSFX(); AnimateClockHand(); });
    }

    private void AnimateClockHand()
    {
        if (clockHand == null) return;

        handAngle -= 45f;
        clockHand.DOKill();
        clockHand.DOLocalRotate(new Vector3(0f, 0f, handAngle), handAnimDuration)
                 .SetEase(handEase)
                 .OnStart(() => PlayClockSFX(handSFX))
                 .OnComplete(StopClockSFX);
    }

    private void PlayClockSFX(AudioClip clip)
    {
        if (clockAudioSource == null || clip == null) return;
        clockAudioSource.clip = clip;
        clockAudioSource.Play();
    }

    private void StopClockSFX()
    {
        if (clockAudioSource != null) clockAudioSource.Stop();
    }
}
