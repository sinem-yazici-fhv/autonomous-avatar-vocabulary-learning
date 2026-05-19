using UnityEngine;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Ziehe hier das gesamte Start Menu Canvas / Panel rein")]
    public GameObject startMenuUI;
    
    [Tooltip("Der Spielen-Button im Menu")]
    public Button playButton;

    [Header("Audio")]
    [Tooltip("AudioSource für die Schulglocke (Schoolbell)")]
    public AudioSource schoolbellAudio;

    void Start()
    {
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;

        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
    }

    public void StartGame()
    {
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(false);
        }

        if (schoolbellAudio != null)
        {
            schoolbellAudio.Play();
        }

        Time.timeScale = 1f;
        
    }
}
