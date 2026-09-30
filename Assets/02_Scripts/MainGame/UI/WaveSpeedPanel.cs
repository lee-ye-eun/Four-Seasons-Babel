using UnityEngine;
using UnityEngine.UI;

public class WaveSpeedPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button speed1xButton;
    [SerializeField] private Button speed2xButton;
    [SerializeField] private Button speed5xButton;

    [Header("Highlight")]
    [SerializeField] private Color activeColor   = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color inactiveColor = Color.white;

    private Image pauseImg, img1x, img2x, img5x;

    private void Awake()
    {
        pauseImg = pauseButton.GetComponent<Image>();
        img1x    = speed1xButton.GetComponent<Image>();
        img2x    = speed2xButton.GetComponent<Image>();
        img5x    = speed5xButton.GetComponent<Image>();

        pauseButton.onClick.AddListener(()    => SetSpeed(0f, pauseImg));
        speed1xButton.onClick.AddListener(()  => SetSpeed(1f, img1x));
        speed2xButton.onClick.AddListener(()  => SetSpeed(2f, img2x));
        speed5xButton.onClick.AddListener(()  => SetSpeed(5f, img5x));

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetSpeed(1f, img1x);
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private void SetSpeed(float speed, Image activeImg)
    {
        Time.timeScale = speed;
        pauseImg.color = pauseImg == activeImg ? activeColor : inactiveColor;
        img1x.color    = img1x    == activeImg ? activeColor : inactiveColor;
        img2x.color    = img2x    == activeImg ? activeColor : inactiveColor;
        img5x.color    = img5x    == activeImg ? activeColor : inactiveColor;
    }
}
