using TMPro;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    //********** VARIABLES**********

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

    //audio 
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lowTimeWarningSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioSource gameAmbientSoundRef;

    //low time overalay and settings
    [Header("Low Time Warning")]
    [SerializeField] private GameObject lowTimeOverlay;
    [SerializeField] private float lowTimeWarningThreshold = 5f;

    private bool isLowTimeSoundPlaying;

    //Game Stats
    private float currentTime;
    private float currentScore;
    public bool isGameOver;


 
    //********** Create GM Singleton On Awake **********
    private void Awake()
    {
        if (GM != null && GM != this)
        {
            Destroy(gameObject);
            return;
        }

        GM = this;
        DontDestroyOnLoad(gameObject);

        //audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }


    //********** Start Reset and Update UI **********
    private void Start()
    {
        currentTime = startTime;
        currentScore = 0;
        UpdateTimerUI();
    }


    //**********  Update timer, handle time left Warning , in case if time <= 0 GameOver() **********
    private void Update()
    {
        if (isGameOver)
            return;

        currentTime -= Time.deltaTime;
        HandleLowTimeWarningSound();

        if (currentTime <= 0f)
            GameOverHandler();
 
        UpdateTimerUI();
    }


    //**********  When Hole or Water Hit Update Timer and Score stats and Move Holes In new Random Places + check game over **********
    public void OnHoleHit(Hole hole)
    {
        if (isGameOver)
            return;

        if (hole != null && hole.HoleType == HoleType.Good)
        {
            currentTime += goodHoleBonus;
            currentScore++; // probably need some extra var instead of ++
        }
        else
        {
            currentTime -= badHolePenalty;
            currentTime = Mathf.Max(currentTime, 0f);
        }
        UpdateTimerUI();

        if (currentTime <= 0f)
            GameOverHandler();

        if (holeManager != null)
            holeManager.MoveHolesAfterShot(hole);

    }


    //**********  UI update **********
    private void UpdateTimerUI()
    {
        if (timerText == null || scoreText == null)
            return;

        timerText.text = Mathf.CeilToInt(currentTime).ToString();
        scoreText.text = Mathf.CeilToInt(currentScore).ToString();
    }


    //**********  New Game Reset **********
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
            holeManager.MoveHolesAfterShot(null);

        UpdateTimerUI();

        //restore ambient volume, if game failed
        gameAmbientSoundRef.volume = 1.0f;
    }



    //**********  Low Time Handler, sound and overlay **********
    private void HandleLowTimeWarningSound()
    {
        if (audioSource == null || lowTimeWarningSound == null)
            return;

        if (currentTime <= lowTimeWarningThreshold && currentTime > 0f)
        {
            if (!isLowTimeSoundPlaying)
            {
                audioSource.clip = lowTimeWarningSound;
                audioSource.loop = true;
                audioSource.Play();

                isLowTimeSoundPlaying = true;

                //show red overlay
                if (lowTimeOverlay != null)
                    lowTimeOverlay.SetActive(true);
            }
        }
        else
        {
            if (audioSource != null && isLowTimeSoundPlaying)
                audioSource.Stop();

            isLowTimeSoundPlaying = false;

            //show red overlay
            if (lowTimeOverlay != null)
                lowTimeOverlay.SetActive(false);
        }
    }


    //**********  Sound Player **********
    private void PlaySound(AudioClip PlaySound)
    {
        if (audioSource == null || PlaySound == null)
            return;

        audioSource.PlayOneShot(PlaySound);
    }


    //********** Game Over **********
    private void GameOverHandler()
    {
        HandleLowTimeWarningSound(); // cancel ticking sound

        //ReduceAmbientSound , play Game Over sound
        gameAmbientSoundRef.volume = 0.1f;
        currentTime = 0f;
        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        PlaySound(gameOverSound);
        UpdateTimerUI();
    }
}