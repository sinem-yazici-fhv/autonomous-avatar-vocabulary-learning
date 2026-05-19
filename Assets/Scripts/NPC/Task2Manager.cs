using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Task2Sentence
{
    public string sentencePattern;
    public string correctAnswer;
    public string[] wrongAnswers;
}

public class Task2Manager : MonoBehaviour
{
    [Header("UI & References")]
    public TMP_Text blackboardText;
    public Button[] answerButtons;
    public NPCInteraction npcInteraction;
    public Animator npcAnimator;
    
    [Header("Transforms for Rotation")]
    public Transform npcTransform;
    public Transform blackboardTransform;

    [Header("Speech Timing")]
    public float firstSentenceIntroDelay = 4.5f;
    public float nextSentenceDelay = 3.5f;
    public float blackboardTypewriterSpeed = 0.05f;

    [Header("Task 2 Data")]
    public string blackboardHeader = "<b>Fill the gap</b>\n\n";
    public Task2Sentence[] sentences = new Task2Sentence[] {
        new Task2Sentence { sentencePattern = "I sit on a _____.", correctAnswer = "chair", wrongAnswers = new string[] { "window", "desk" } },
        new Task2Sentence { sentencePattern = "I carry my _____ to school.", correctAnswer = "backpack", wrongAnswers = new string[] { "jacket", "ruler" } },
        new Task2Sentence { sentencePattern = "I write in my _____.", correctAnswer = "notebook", wrongAnswers = new string[] { "eraser", "pen" } },
        new Task2Sentence { sentencePattern = "I measure with a _____.", correctAnswer = "ruler", wrongAnswers = new string[] { "pen", "eraser" } },
        new Task2Sentence { sentencePattern = "I erase mistakes with an _____.", correctAnswer = "eraser", wrongAnswers = new string[] { "pen", "ruler" } },
        new Task2Sentence { sentencePattern = "I write with a _____.", correctAnswer = "pen", wrongAnswers = new string[] { "eraser", "ruler" } },
        new Task2Sentence { sentencePattern = "I hang my _____ in the locker.", correctAnswer = "jacket", wrongAnswers = new string[] { "backpack", "eraser" } },
        new Task2Sentence { sentencePattern = "The teacher writes on the _____.", correctAnswer = "blackboard", wrongAnswers = new string[] { "window", "door" } }
    };

    private int currentIndex = 0;
    private int correctFirstTry = 0;
    private int multipleTries = 0;
    private bool wrongThisRound = false;
    private Transform playerTransform;

    private enum TaskState { Pending, Correct, Wrong }
    private TaskState[] taskStates;
    private NPCOverlay npcOverlay;

    private string[] praiseMessages = {
    "Excellent!",
    "Well done!",
    "Amazing!",
    "Great job!",
    "Brilliant!"
    };

    void Start()
    {
        HideButtons();
        if (blackboardText != null) blackboardText.text = "";
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        
        npcOverlay = FindObjectOfType<NPCOverlay>();
    }

    public void StartTask2()
    {
        npcInteraction.SetSpeech("Great! Let's do the next task: Sentence Task. Come to the classroom!");
        if (npcOverlay != null) npcOverlay.SetHelpButtonVisible(false);
        currentIndex = 0;
        correctFirstTry = 0;
        multipleTries = 0;
        taskStates = new TaskState[sentences.Length];
        
        StartCoroutine(NextSentenceRoutine());
    }

    private void UpdateProgressBar()
    {
        string barStr = "";
        for (int i = 0; i < sentences.Length; i++)
        {
            if (taskStates[i] == TaskState.Correct)
                barStr += "<color=#4CAF50>█</color>"; 
            else if (taskStates[i] == TaskState.Wrong)
                barStr += "<color=#F44336>█</color>"; 
            else
                barStr += "<color=#B0B0B0>█</color>";
        }
        
        int current = Mathf.Min(currentIndex + 1, sentences.Length);
        string finalProgressStr = $"{barStr}   {current}/{sentences.Length}";
        
        if (npcInteraction != null && npcInteraction.progressText != null)
        {
            npcInteraction.progressText.text = finalProgressStr;
            npcInteraction.RefreshSpeechBubbleLayout();
        }
        
        if (npcOverlay != null)
        {
            npcOverlay.SetOverlayProgress(finalProgressStr);
        }
    }

    private IEnumerator NextSentenceRoutine()
    {
        wrongThisRound = false;
        UpdateProgressBar();

        if (currentIndex == 0) yield return new WaitForSeconds(firstSentenceIntroDelay);
        else yield return new WaitForSeconds(nextSentenceDelay);

        if (currentIndex >= sentences.Length)
        {
            ShowFinalResult();
            yield break;
        }

        var task = sentences[currentIndex];
        
        if (npcTransform != null && blackboardTransform != null)
        {
            Vector3 targetPos = new Vector3(blackboardTransform.position.x, npcTransform.position.y, blackboardTransform.position.z);
            npcTransform.LookAt(targetPos);
        }

        if (blackboardText != null)
        {
            blackboardText.text = blackboardHeader;
            foreach (char c in task.sentencePattern)
            {
                blackboardText.text += c;
                yield return new WaitForSeconds(blackboardTypewriterSpeed);
            }
        }

        if (npcTransform != null && playerTransform != null)
        {
            Vector3 targetPos = new Vector3(playerTransform.position.x, npcTransform.position.y, playerTransform.position.z);
            npcTransform.LookAt(targetPos);
        }

        SetupButtons(task);
    }

    private void SetupButtons(Task2Sentence task)
    {
        if (answerButtons == null || answerButtons.Length < 3) return;

        List<string> options = new List<string>(task.wrongAnswers);
        options.Add(task.correctAnswer);
        
        for (int i = 0; i < options.Count; i++) {
            string temp = options[i];
            int randomIndex = Random.Range(i, options.Count);
            options[i] = options[randomIndex];
            options[randomIndex] = temp;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < options.Count)
            {
                answerButtons[i].gameObject.SetActive(true);
                TMP_Text btnText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                if (btnText != null) btnText.text = options[i];
                
                string selectedAnswer = options[i];
                answerButtons[i].onClick.RemoveAllListeners();
                
                string curCorrect = task.correctAnswer;
                string curSentence = task.sentencePattern;
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(selectedAnswer, curCorrect, curSentence));
            }
        }
    }

    private void OnAnswerSelected(string answer, string correctAnswer, string sentencePattern)
    {
        if (answer == correctAnswer)
        {
            if (!wrongThisRound)
            {
                correctFirstTry++;
                taskStates[currentIndex] = TaskState.Correct;
            }
            else
            {
                multipleTries++;
            }
            
            UpdateProgressBar();

            HideButtons();

            if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", false);
            npcAnimator.SetTrigger("doNod");
        }
   
            string completedSentence = sentencePattern.Replace("_____", answer);
            string randomPraise = praiseMessages[Random.Range(0, praiseMessages.Length)];

            npcInteraction.SetSpeech($"{randomPraise} {completedSentence}");
            
            if (blackboardText != null)
                blackboardText.text = blackboardHeader + completedSentence;

            currentIndex++;
            StartCoroutine(NextSentenceRoutine());
        }
        else
        {
            if (!wrongThisRound)
            {
                taskStates[currentIndex] = TaskState.Wrong;
                UpdateProgressBar();
            }
            wrongThisRound = true;

             if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", false);
            npcAnimator.SetTrigger("doShake");
        }
            npcInteraction.SetSpeech("Try again!");
        }
    }

    private void HideButtons()
    {
        if (answerButtons == null) return;
        foreach (Button b in answerButtons) {
            if (b != null) b.gameObject.SetActive(false);
        }
    }

    private void ShowFinalResult()
    {
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", false);
            npcAnimator.SetTrigger("doClap");
        }
        if (npcInteraction != null && npcInteraction.progressText != null) npcInteraction.progressText.text = "";
        if (npcOverlay != null) npcOverlay.SetOverlayProgress("");
        if (npcOverlay != null) npcOverlay.SetHelpButtonVisible(false);
        
        int total = sentences.Length;
        int score = Mathf.RoundToInt((correctFirstTry / (float)total) * 100);

        int bestCorrect2 = PlayerPrefs.GetInt("Task2BestCorrect", 0);
        if (correctFirstTry > bestCorrect2)
        {
            PlayerPrefs.SetInt("Task2BestCorrect", correctFirstTry);
            bestCorrect2 = correctFirstTry;
        }

        int bestScore2 = PlayerPrefs.GetInt("Task2BestScore", 0);
        if (score > bestScore2)
        {
            PlayerPrefs.SetInt("Task2BestScore", score);
            bestScore2 = score;
        }
        PlayerPrefs.Save();

        string message = $"Task 2 finished!\n" +
                         $"First try correct: {correctFirstTry}/{total}\n" +
                         $"Needed multiple tries: {multipleTries}\n" +
                         $"Your Score: {score}%\n" +
                         $"Try again or finish?";

        npcInteraction.SetSpeech(message);
        if (blackboardText != null)
            blackboardText.text = "Well done!";
        
        if (npcTransform != null && playerTransform != null)
        {
            Vector3 targetPos = new Vector3(playerTransform.position.x, npcTransform.position.y, playerTransform.position.z);
            npcTransform.LookAt(targetPos);
        }

        Button tryAgain = npcInteraction.tryAgainButton;
        Button next = npcInteraction.nextTaskButton;
        
        tryAgain.gameObject.SetActive(true);
        next.gameObject.SetActive(true);

        TMP_Text nextText = next.GetComponentInChildren<TMP_Text>();
        if (nextText != null) nextText.text = "Finish Tasks";

        tryAgain.onClick.RemoveAllListeners();
        next.onClick.RemoveAllListeners();

        tryAgain.onClick.AddListener(TryAgainTask2);
        next.onClick.AddListener(ShowOverallSummary);
    }

    private void TryAgainTask2()
    {
        npcInteraction.tryAgainButton.gameObject.SetActive(false);
        npcInteraction.nextTaskButton.gameObject.SetActive(false);
        StartTask2();
    }

    private void ShowOverallSummary()
    {
       GameObject.Find("SchoolBell").GetComponent<AudioSource>().Play();

        npcInteraction.tryAgainButton.gameObject.SetActive(false);
        npcInteraction.nextTaskButton.gameObject.SetActive(false); 
        if (npcOverlay != null) npcOverlay.SetHelpButtonVisible(false);

        int total = sentences.Length;
        int currentScore1 = Mathf.RoundToInt((npcInteraction.CurrentFirstTryCorrect / (float)npcInteraction.TotalTasks) * 100);
        int currentScore2 = Mathf.RoundToInt((correctFirstTry / (float)total) * 100);
        int currentTotalScore = Mathf.RoundToInt((currentScore1 + currentScore2) / 2f);
        
        int totalPracticed = npcInteraction.TotalTasks + total;
        int firstTryTotal = npcInteraction.CurrentFirstTryCorrect + correctFirstTry;

        string motivation = currentTotalScore >= 90 ? "Outstanding work!" :
                    currentTotalScore >= 75 ? "Excellent job!" :
                    currentTotalScore >= 60 ? "Good effort!" :
                    "Keep practicing!";

string totalColor = currentTotalScore >= 75 ? "#4CAF50" :
                    currentTotalScore >= 50 ? "#FFC107" :
                    "#F44336";

string task1Color = currentScore1 >= 75 ? "#4CAF50" :
                    currentScore1 >= 50 ? "#FFC107" :
                    "#F44336";

string task2Color = currentScore2 >= 75 ? "#4CAF50" :
                    currentScore2 >= 50 ? "#FFC107" :
                    "#F44336";

string message =
    $"<b>{motivation}</b>\n" +
    $"You completed both tasks!\n" +

    $"Task 1: <color={task1Color}><b>{currentScore1}%</b></color>\n" +
    $"Task 2: <color={task2Color}><b>{currentScore2}%</b></color>\n" +
    $"Total Score: <color={totalColor}><b>{currentTotalScore}%</b></color>\n" +

    $"Words Practiced: <b>{totalPracticed}</b>\n" +
    $"First Try Correct: <color=#4CAF50><b>{firstTryTotal}</b></color>\n" +

    $"See you next lesson!";
        npcInteraction.SetSpeech(message);
    }
}
