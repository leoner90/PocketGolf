using UnityEngine;
using TMPro;



public class GameManager : MonoBehaviour
{
    //ref
    public static GameManager GM { get; private set; }

    [Header("References")]
    [SerializeField] private HoleManager holeManager;
    [SerializeField] private PlayerBall playerBall;

    //Timers and Hit Hole Effects
    [Header("Timer Settings")]
    [SerializeField] private float startTime = 30f;
    [SerializeField] private float goodHoleBonus = 5f;
    [SerializeField] private float badHolePenalty = 10f;

    //UI
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject mainMenuPanel;

    //Game Stats
    private float currentTime;
    private float currentScore;
    public bool isGameOver;

    //create Singleton on Awake
    private void Awake()
    {
        if (GM != null && GM != this)
        {
            Destroy(gameObject);
            return;
        }

        GM = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        currentTime = startTime;
        currentScore = 0;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (isGameOver)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isGameOver = true;
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }

        UpdateTimerUI();
    }

    public void OnHoleHit(HoleType holeType)
    {
        if (isGameOver)
            return;

        if (holeType == HoleType.Good)
        {
            currentTime += goodHoleBonus;
            currentScore += 1;
        }
        else
        {
            currentTime -= badHolePenalty;
            currentTime = Mathf.Max(currentTime, 0f);
        }

        if (holeManager != null)
            holeManager.MoveHolesAfterShot();

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null || scoreText == null)
            return;

        timerText.text = Mathf.CeilToInt(currentTime).ToString();
        scoreText.text = Mathf.CeilToInt(currentScore).ToString();
    }


    public void StartNewGame()
    {
        currentTime = startTime;
        currentScore = 0;
        isGameOver = false;

        //basicly need it only when game starts first time so a bit stupid set it false every time
        if(mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (playerBall != null)
            playerBall.ResetBallToStart();

        if (holeManager != null)
            holeManager.MoveHolesAfterShot();

        UpdateTimerUI();
    }
}