using UnityEngine;

public enum ObstacleType
{
    Mud,
    Water
}

[RequireComponent(typeof(Collider2D))]
public class ObstacleZone : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private ObstacleType obstacleType = ObstacleType.Mud;

    [Header("Mud Settings")]
    [SerializeField] private float mudSpeedMultiplier = 0.35f;
    [SerializeField] private float mudLinearDamping = 4f;

    //vfx
    [Header("VFX")]
    [SerializeField] private GameObject waterHitVfx;
    [SerializeField] private GameObject mudHitVfx;

    //audio 
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip waterSound;
    [SerializeField] private AudioClip mudSound;

    private void Awake()
    {
        Collider2D obstacleCollider = GetComponent<Collider2D>();
        obstacleCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerBall playerBall = other.GetComponent<PlayerBall>();

        if (playerBall == null)
            return;

        if (obstacleType == ObstacleType.Mud)
        {
            playerBall.EnterMud(mudSpeedMultiplier, mudLinearDamping);
            playerBall.SpawnVfx(mudHitVfx);
            PlaySound(mudSound);
        }
        else if (obstacleType == ObstacleType.Water)
        {
            if (GameManager.GM != null)
                GameManager.GM.OnHoleHit(HoleType.Bad);

            playerBall.SpawnVfx(waterHitVfx);
            PlaySound(waterSound);
            playerBall.ResetBallToStart();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerBall playerBall = other.GetComponent<PlayerBall>();

        if (playerBall == null)
            return;

        if (obstacleType == ObstacleType.Mud)
        {
            playerBall.ExitMud();
        }
    }

    private void PlaySound(AudioClip PlaySound)
    {
        if (audioSource == null || PlaySound == null)
            return;

        audioSource.PlayOneShot(PlaySound);
    }
}