using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnWeightResultPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button anywhereBlocker;

    private Action onConfirmed;

    private void Awake()
    {
        anywhereBlocker.onClick.AddListener(OnConfirm);
        gameObject.SetActive(false);
    }

    public void Show(float weight, int count, Action onConfirm)
    {
        onConfirmed = onConfirm;
        resultText.text = $"X{weight:F2}\n{count}명의 인간이 바벨탑을 향합니다….";
        gameObject.SetActive(true);
    }

    private void OnConfirm()
    {
        gameObject.SetActive(false);
        onConfirmed?.Invoke();
        onConfirmed = null;
    }
}
