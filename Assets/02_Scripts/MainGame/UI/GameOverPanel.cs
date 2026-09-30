using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;

    private void Awake()
    {
        gameStateManager.OnGameOver += Show;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        gameStateManager.OnGameOver -= Show;
    }

    private void Show()
    {
        gameObject.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.DuckBGM();
        Time.timeScale = 0f;
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnTitleButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }
}
