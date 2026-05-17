using UnityEngine;

//Types of Obstacles
public enum ObstacleType { Mud, Water }

public class ObstacleZone : MonoBehaviour
{
    //********** VARIABLES **********

    [Header("Obstacle Settings")]
    [SerializeField] private ObstacleType obstacleType = ObstacleType.Mud;

    [Header("Mud Settings")]
    [SerializeField] private float mudSpeedMultiplier = 0.35f;
    [SerializeField] private float mudLinearDamping = 4f;

    [Header("VFX")]
    [SerializeField] private GameObject hitVfx;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;


    //********** Awake **********
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }


    //********** ON Water or Mud Hit Trigger **********
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerBall playerBall = other.GetComponent<PlayerBall>();

        if (playerBall == null)
            return;
        //vfx & sound
        playerBall.SpawnVfx(hitVfx);
        PlaySound(hitSound);

        //if Mud
        if (obstacleType == ObstacleType.Mud)
        {
            playerBall.EnterMud(mudSpeedMultiplier, mudLinearDamping);
        }
        //If water
        else if (obstacleType == ObstacleType.Water)
        {

            if (GameManager.GM != null)
            {
                GameManager.GM.OnHoleHit(null); // treat water same way as red hole

                if (GameManager.GM.isGameOver)
                    return;
            }

            playerBall.ResetBallToStart();
        }
    }


    //********** Restore Player Speed when leave Mud **********
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerBall playerBall = other.GetComponent<PlayerBall>();

        if (playerBall == null)
            return;

        if (obstacleType == ObstacleType.Mud)
            playerBall.ExitMud();
    }


    //********** Sound Player**********
    private void PlaySound(AudioClip sound)
    {
        if (audioSource == null || sound == null)
            return;

        audioSource.PlayOneShot(sound);
    }
}