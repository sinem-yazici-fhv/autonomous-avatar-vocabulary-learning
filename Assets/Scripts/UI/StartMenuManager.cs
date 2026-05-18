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
        // 1. Menü aktivieren, wenn das Spiel startet
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(true);
        }

        // 2. Das Spiel (die Zeit) pausieren, damit man nicht rumlaufen kann
        Time.timeScale = 0f;

        // 3. Dem Button die Funktion zuweisen
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
    }

    public void StartGame()
    {
        // 1. Menü ausschalten
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(false);
        }

        // 2. Schulglocke abspielen, falls vorhanden
        if (schoolbellAudio != null)
        {
            schoolbellAudio.Play();
        }

        // 3. Das Spiel weiterlaufen lassen
        Time.timeScale = 1f;
        
    }
}
