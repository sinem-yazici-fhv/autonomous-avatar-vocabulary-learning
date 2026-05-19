using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("Speech Timing")]
    public float task1CorrectMessageDuration = 3.5f;
    public float minimumTalkingDuration = 3f;
    public float talkingSecondsPerCharacter = 0.06f;

    public TMP_Text speechText;
    public ObjectClick objectClick;
    public Button tryAgainButton;
    public Button nextTaskButton;
    public Task2Manager task2Manager;
    public Animator animator;
    public TMP_Text progressText;

    NPCOverlay npcOverlay;
    SpeechBubble introSpeechBubble;
    RectTransform roomSpeechBackgroundRect;
    RectTransform roomSpeechBubbleRect;
    ObjectLabel jacketLockerLabel;
    
    private bool playerIsNear = false;
    private bool taskStarted = false;
    private int firstTryCorrect = 0;
    private int multipleTries = 0;
    private bool wrongThisRound = false;

    private enum TaskState { Pending, Correct, Wrong }
    private TaskState[] taskStates;

    private (string tag, string example, string hint)[] tasks = {
        ("Chair", "I sit on a chair.", "Look near the desks in the classroom."),
        ("Backpack", "I carry my backpack to school.", "Look for a school bag in the classroom."),
        ("Notepad", "I write in my notebook.", "Look on a desk for something used for writing."),
        ("Ruler", "I measure with a ruler.", "Look on a desk for a long school object."),
        ("Eraser", "I erase mistakes with an eraser.", "Look on a desk for something to correct mistakes."),
        ("Pen", "I write with a pen.", "Look on a desk for something used to write."),
        ("FireExtinguisher", "The fire extinguisher is on the wall.", "Look on the wall for a red object."),
        ("Laptop", "I use a laptop in class.", "Look on the teacher's desk."),
        ("Blackboard", "The teacher writes on the blackboard.", "Look at the front of the classroom."),
        ("Bookshelf", "There are many books on the bookshelf.", "Look for a shelf with books."),
         ("Jacket", "I hang my jacket in the locker.", "Click Help if you need to see the correct locker.")
    };

    private int currentTaskIndex = 0;

    public int TotalTasks => tasks.Length;
    public int CurrentFirstTryCorrect => firstTryCorrect;
    public bool IsTask1HintAvailable => taskStarted && currentTaskIndex < tasks.Length;

    private string[] praiseMessages = {
    "Excellent!",
    "Well done!",
    "Amazing!",
    "Great job!",
    "Brilliant!"
    };


    void Start()
    {
        npcOverlay = FindObjectOfType<NPCOverlay>();
        introSpeechBubble = FindObjectOfType<SpeechBubble>();
        roomSpeechBackgroundRect = speechText != null ? speechText.transform.parent as RectTransform : null;
        roomSpeechBubbleRect = roomSpeechBackgroundRect != null ? roomSpeechBackgroundRect.parent as RectTransform : null;
        objectClick = FindObjectOfType<ObjectClick>();
        objectClick.onCorrect += OnCorrect;
        objectClick.onWrong += OnWrong;

        tryAgainButton.gameObject.SetActive(false);
        nextTaskButton.gameObject.SetActive(false);
        tryAgainButton.onClick.AddListener(TryAgain);
        nextTaskButton.onClick.AddListener(NextTask);
        
        taskStates = new TaskState[tasks.Length];
        if (progressText != null) progressText.text = "";
        if (npcOverlay != null) npcOverlay.SetHelpButtonVisible(false);
        CacheJacketLockerLabel();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = false;
    }

    void Update()
    {
        if (playerIsNear && !taskStarted && Input.GetKeyDown(KeyCode.E))
        {
            taskStarted = true;
            if (introSpeechBubble != null)
                introSpeechBubble.StopIntro();
            ObjectLabel[] labels = FindObjectsOfType<ObjectLabel>();
            foreach (var label in labels)
                label.SetExploreMode(false);
            StartNextTask();
        }
    }

    void UpdateProgressBar()
    {
        if (progressText == null) return;
        
        string barStr = "";
        for (int i = 0; i < tasks.Length; i++)
        {
            if (taskStates[i] == TaskState.Correct)
                barStr += "<color=#4CAF50>█</color>";
            else if (taskStates[i] == TaskState.Wrong)
                barStr += "<color=#F44336>█</color>";
            else
                barStr += "<color=#B0B0B0>█</color>";
        }
        
        int current = Mathf.Min(currentTaskIndex + 1, tasks.Length);
        string finalProgressStr = $"{barStr}   {current}/{tasks.Length}";
        progressText.text = finalProgressStr;
        ResizeRoomSpeechBubble();
        
        if (npcOverlay != null)
            npcOverlay.SetOverlayProgress(finalProgressStr);
    }

    void StartNextTask()
    {
        UpdateProgressBar();
        HideJacketLockerLabel();
        
        if (currentTaskIndex >= tasks.Length)
        {
            ShowResult();
            return;
        }

        wrongThisRound = false;
        if (npcOverlay != null) npcOverlay.SetHelpButtonVisible(true);
        var task = tasks[currentTaskIndex];
        if (task.tag == "Jacket")
        {
            ShowJacketLockerLabel();
            SetSpeech("Find the correct object: Jacket\n\nOpen a locker and click on the jacket!");
        }
        else
        {
            SetSpeech($"Find the correct object: {task.tag}\n\nClick on it!");
        }
        objectClick.StartTask(task.tag);
    }

    void OnCorrect()
    {
        if (!wrongThisRound)
        {
            firstTryCorrect++;
            taskStates[currentTaskIndex] = TaskState.Correct;
        }
        else
        {
            multipleTries++;
        }
        
        UpdateProgressBar();

        var task = tasks[currentTaskIndex];
        currentTaskIndex++;
        HideJacketLockerLabel();

        if (animator != null)
        {
            animator.SetBool("isTalking", false);
            CancelInvoke(nameof(StopTalking));
            animator.SetTrigger("doNod");
        }

        string randomPraise = praiseMessages[Random.Range(0, praiseMessages.Length)];

        speechText.text = $"{randomPraise}\nExample: {task.example}";

        if (npcOverlay != null)
            npcOverlay.SetOverlayMessage($"{randomPraise}\nExample: {task.example}");
        ResizeRoomSpeechBubble();

        Invoke(nameof(StartNextTask), task1CorrectMessageDuration);
    }

    void OnWrong()
    {
        if (!wrongThisRound)
        {
            taskStates[currentTaskIndex] = TaskState.Wrong;
            UpdateProgressBar();
        }
        wrongThisRound = true;
        var task = tasks[currentTaskIndex];

        if (animator != null)
        {
            animator.SetBool("isTalking", false);
            CancelInvoke(nameof(StopTalking));
            animator.SetTrigger("doShake");
        }

        speechText.text = $"Try again!\nFind: {task.tag}";
        if (npcOverlay != null)
            npcOverlay.SetOverlayMessage($"Try again!\nFind: {task.tag}");
        ResizeRoomSpeechBubble();
    }

    public void GiveHint()
    {
        if (currentTaskIndex < tasks.Length)
        {
            var task = tasks[currentTaskIndex];
            if (task.tag == "Jacket")
            {
                ShowJacketLockerHelp();
                ShowJacketLockerLabel();
                SetSpeech("Hint: Open this locker!");
                return;
            }
            SetSpeech($"Hint: {task.hint}");
        }
    }

    void ShowResult()
    {

    if (animator != null)
    {
        animator.SetBool("isTalking", false);
        CancelInvoke(nameof(StopTalking));
        animator.SetTrigger("doClap");
    }
        if (progressText != null) progressText.text = "";
        if (npcOverlay != null) npcOverlay.SetOverlayProgress("");
        if (npcOverlay != null) npcOverlay.SetHelpButtonVisible(false);
        HideJacketLockerLabel();
        
        int total = tasks.Length;
        int score = Mathf.RoundToInt((firstTryCorrect / (float)total) * 100);

        int bestCorrect = PlayerPrefs.GetInt("Task1BestCorrect", 0);
        if (firstTryCorrect > bestCorrect)
        {
            PlayerPrefs.SetInt("Task1BestCorrect", firstTryCorrect);
        }
        
        int bestScore = PlayerPrefs.GetInt("Task1BestScore", 0);
        if (score > bestScore)
        {
            PlayerPrefs.SetInt("Task1BestScore", score);
        }
        PlayerPrefs.Save();

        SetSpeech($"Great job! You finished!\n" +
                  $"First try correct: {firstTryCorrect}/{total}\n" +
                  $"Needed multiple tries: {multipleTries}\n" +
                  $"Your Score: {score}%\n" +
                  $"Try again or go to next task?");

        tryAgainButton.gameObject.SetActive(true);
        nextTaskButton.gameObject.SetActive(true);
    }

    void TryAgain()
    {
        currentTaskIndex = 0;
        firstTryCorrect = 0;
        multipleTries = 0;
        wrongThisRound = false;
        taskStarted = false;
        taskStates = new TaskState[tasks.Length];
        if (progressText != null) progressText.text = "";
        if (npcOverlay != null) npcOverlay.SetOverlayProgress("");
        if (npcOverlay != null) npcOverlay.SetHelpButtonVisible(false);
        HideJacketLockerLabel();
        tryAgainButton.gameObject.SetActive(false);
        nextTaskButton.gameObject.SetActive(false);
        SetSpeech("Let's try again! Press E to start!");
    }

    void NextTask()
    {
        tryAgainButton.gameObject.SetActive(false);
        nextTaskButton.gameObject.SetActive(false);
        
        if (task2Manager != null)
            task2Manager.StartTask2();
        else
            SetSpeech("Great! Next task coming soon!");
    }

    public string GetCurrentHint()
    {
        if (currentTaskIndex < tasks.Length)
        {
            if (tasks[currentTaskIndex].tag == "Jacket")
                return "Open this locker!";
            return tasks[currentTaskIndex].hint;
        }
        return "Keep looking!";
    }

    public void SetSpeech(string message, bool withTalking = true)
    {
        speechText.text = message;
        ResizeRoomSpeechBubble();

        if (npcOverlay != null)
            npcOverlay.SetOverlayMessage(message);

        if (withTalking && animator != null)
        {
            animator.SetBool("isTalking", true);
            CancelInvoke(nameof(StopTalking));
            float talkDuration = Mathf.Max(minimumTalkingDuration, message.Length * talkingSecondsPerCharacter);
            Invoke(nameof(StopTalking), talkDuration);
        }
    }

    void StopTalking()
    {
        if (animator != null)
            animator.SetBool("isTalking", false);
    }

    void ResizeRoomSpeechBubble()
    {
        if (speechText == null || roomSpeechBackgroundRect == null) return;

        float maxWidth = 420f;
        float minWidth = 250f;
        float minHeight = 110f;

        speechText.fontSize = 24f;
        
        bool hasProgress = progressText != null && !string.IsNullOrEmpty(progressText.text);
        float topMargin = hasProgress ? 30f : 0f;

        speechText.margin = new Vector4(0f, topMargin, 0f, 18f);

        Vector2 preferred = speechText.GetPreferredValues(speechText.text, maxWidth - 42f, 0f);

        float width = Mathf.Clamp(preferred.x + 42f, minWidth, maxWidth);
        float height = Mathf.Max(minHeight, preferred.y + 34f + topMargin);

        roomSpeechBackgroundRect.sizeDelta = new Vector2(width, height);

        if (roomSpeechBubbleRect != null)
            roomSpeechBubbleRect.sizeDelta = new Vector2(width, height);
            
        if (hasProgress)
        {
            RectTransform ptRect = progressText.rectTransform;
            ptRect.anchorMin = new Vector2(0.5f, 1f);
            ptRect.anchorMax = new Vector2(0.5f, 1f);
            ptRect.pivot = new Vector2(0.5f, 1f);
            ptRect.sizeDelta = new Vector2(width - 20f, 30f);
            ptRect.anchoredPosition = new Vector2(0, -10f); 
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.fontSize = 20f;
        }
    }

    public void RefreshSpeechBubbleLayout()
    {
        ResizeRoomSpeechBubble();
    }

    void CacheJacketLockerLabel()
    {
        LockerClick[] lockers = FindObjectsOfType<LockerClick>(true);
        foreach (var locker in lockers)
        {
            if (!locker.HasJacket) continue;

            jacketLockerLabel = locker.GetComponent<ObjectLabel>();
            if (jacketLockerLabel == null)
                jacketLockerLabel = locker.GetComponentInChildren<ObjectLabel>(true);
            if (jacketLockerLabel == null)
                jacketLockerLabel = locker.GetComponentInParent<ObjectLabel>();
            break;
        }
    }

    void ShowJacketLockerLabel()
    {
        if (jacketLockerLabel != null)
            jacketLockerLabel.ShowLabel();
    }

    void HideJacketLockerLabel()
    {
        if (jacketLockerLabel != null)
            jacketLockerLabel.HideLabel();
    }

    void ShowJacketLockerHelp()
    {
        if (jacketLockerLabel != null)
            jacketLockerLabel.SetLabelText("Open the highlighted locker to find the jacket.");
    }
}
