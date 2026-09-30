using UnityEngine;
using UnityEngine.UI;

// Button 컴포넌트가 있는 오브젝트에 추가하면 클릭 시 자동으로 효과음 재생
[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(Play);
    }

    private void Play() => AudioManager.Instance?.PlaySFX(clip);
}
