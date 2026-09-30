using UnityEngine;
using TMPro;

public class FaithDisplay : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private TextMeshProUGUI faithText;

    private void Awake()
    {
        gameStateManager.OnFaithChanged += UpdateText;
    }

    private void Start()
    {
        UpdateText(gameStateManager.CurrentFaith);
    }

    private void OnDestroy()
    {
        gameStateManager.OnFaithChanged -= UpdateText;
    }

    private void UpdateText(int faith)
    {
        faithText.text = faith.ToString();
    }
}
