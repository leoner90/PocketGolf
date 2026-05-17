using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]


public class PlayerBall : MonoBehaviour
{
    //********** VARIABLES **********

    //Custom Components
    [SerializeField] private PlayerBallInput playerInput;

    //Player States
    public enum PlayerBallState{ Unoccupied, Busy }
    private PlayerBallState currentState = PlayerBallState.Unoccupied;

    //Defaul Ref
    private Rigidbody2D rb;

    [SerializeField] private SpriteRenderer ballRenderer;

    //Shot Parameteres (private - no need anywhere else)
    [Header("Shot Settings")]
    [SerializeField] private float shotPower = 8f;
    [SerializeField] private float maxDragDistance = 2.5f;
    [SerializeField] private float stopVelocity = 0.08f;
    [SerializeField] private float touchRadius = 0.7f;

    //aimiming
    [Header("References")]
    [SerializeField] private LineRenderer aimLine;
    private bool isAiming;
    private Vector2 dragStartWorld;
    private Vector2 dragCurrentWorld;
    public bool IsAiming => isAiming; // public access (getter)

    //sounds 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip goodHoleSound;
    [SerializeField] private AudioClip badHoleSound;
    [SerializeField] private AudioClip playerRestartSound;

    //start pos
    private Vector2 startPosition;

    //stop ball when velocity is low
    [SerializeField] private float snapStopVelocity = 0.15f;

    //rotation setings
    [Header("Visual Rotation")]
    [SerializeField] private Transform ballVisual;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float minRotationVelocity = 0.05f;

    //for ball enters mud  -> save/reset damping
    private float defaultLinearDamping;
    private float defaultAngularDamping;

    //VFX ref
    [Header("VFX")]
    [SerializeField] private GameObject goodHoleHitVfx;
    [SerializeField] private GameObject badHoleHitVfx;
    [SerializeField] private GameObject resetBallVfx;


    //********** AWAKE **********
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        //custom player input component
        if (playerInput == null)
            playerInput = GetComponent<PlayerBallInput>();

        //get audio compontn
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        //Save Start Position for reset
        startPosition = transform.position;

        //rotation find tranform
        if (ballVisual == null)
            ballVisual = transform;

        //for ball enters mud -> save/reset dampping
        defaultLinearDamping = rb.linearDamping;
        defaultAngularDamping = rb.angularDamping;
    }


    //********** UPDATE **********
    private void Update()
    {
        // stop if too slow
        SnapStopIfSlow(); 

        //If GameOver
        if (GameManager.GM != null && GameManager.GM.isGameOver)
        {
            CancelAim();
            StopBall();
            return;
        }

        //Ball Animation (rotation)
        RotateBallVisual();

        //Aiming / Shoot
        if (isAiming)
        {
            if (playerInput != null && playerInput.PointerHeld())
            {
                dragCurrentWorld = playerInput.GetPointerWorldPosition();
                UpdateAimLine();
            }

            if (playerInput != null && playerInput.PointerUp())
                Shoot();
      
            return;
        }

        if (!CanShoot())
            return;

        if (playerInput != null && playerInput.PointerDown())
        {
            Vector2 pointerWorld = playerInput.GetPointerWorldPosition();

            if (Vector2.Distance(pointerWorld, rb.position) <= touchRadius)
            {
                isAiming = true;
                dragStartWorld = rb.position;
                dragCurrentWorld = pointerWorld;

                if (aimLine != null)
                    aimLine.enabled = true;
            }
        }
    }


    //********** AIM LOGIC **********
    private void CancelAim()
    {
        isAiming = false;

        if (aimLine != null)
            aimLine.enabled = false;
    }

    private void UpdateAimLine()
    {
        if (aimLine == null)
            return;

        Vector2 dragVector = dragCurrentWorld - dragStartWorld;
        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        Vector2 shotDirection = -dragVector.normalized;
        float lineLength = dragVector.magnitude * 1.5f;

        Vector2 lineStart = rb.position;
        Vector2 lineEnd = lineStart + shotDirection * lineLength;

        aimLine.SetPosition(0, lineStart);
        aimLine.SetPosition(1, lineEnd);
    }


    //********** SHOOT LOGIC **********
    private bool CanShoot()
    {
        return rb.linearVelocity.magnitude <= stopVelocity && !isAiming && currentState == PlayerBallState.Unoccupied;
    }

    private void Shoot()
    {
        Vector2 dragVector = dragCurrentWorld - dragStartWorld;
        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        if (dragVector.magnitude < 0.1f)
        {
            CancelAim();
            return;
        }

        Vector2 shotDirection = -dragVector.normalized;
        float powerPercent = dragVector.magnitude / maxDragDistance;

        rb.AddForce(shotDirection * shotPower * powerPercent, ForceMode2D.Impulse);
        PlaySound(shootSound);
        CancelAim();
    }


    //********** IF OVERLAPED WITH HOLE **********
    private void OnTriggerEnter2D(Collider2D otherActor)
    {
        Hole hole = otherActor.GetComponent<Hole>();

        if (hole == null)
            return;

        if (GameManager.GM != null)
            GameManager.GM.OnHoleHit(hole);

        StopBall();

        if (GameManager.GM != null && GameManager.GM.isGameOver)
            return;

        //vfx
        GameObject selectedVfx = hole.HoleType == HoleType.Good ? goodHoleHitVfx : badHoleHitVfx;
        SpawnVfx(selectedVfx);

        //sound
        AudioClip currentHoleSound = hole.HoleType == HoleType.Good ? goodHoleSound : badHoleSound;
        PlaySound(currentHoleSound);

        ResetBallToStart();     
    }


    //********** STOP THE BALL **********
    private void StopBall()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void SnapStopIfSlow()
    {
        if (isAiming)
            return;

        if (rb.linearVelocity.magnitude > 0f && rb.linearVelocity.magnitude <= snapStopVelocity)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }


    //********** Player Reset **********
    public void ResetBallToStart()
    {
        currentState = PlayerBallState.Busy;
        //hide ball sprite and wait before reset the possition
        if (ballRenderer != null)
            ballRenderer.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = startPosition;

        StartCoroutine(ResetBallAfterDelay(1.0f));
    }

    private IEnumerator ResetBallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetBallVisual();
    }

    public void ResetBallVisual()
    {
        if (ballRenderer != null)
            ballRenderer.enabled = true;

        if(playerRestartSound != null)
            PlaySound(playerRestartSound);

        SpawnVfx(resetBallVfx);
        currentState = PlayerBallState.Unoccupied;
    }


    //********* "Animation" of the ball *********
    private void RotateBallVisual()
    {
        if (ballVisual == null)
            return;

        Vector2 velocity = rb.linearVelocity;

        if (velocity.magnitude < minRotationVelocity)
            return;

        float rotationDirection = -Mathf.Sign(velocity.x);

        if (Mathf.Abs(velocity.x) < 0.05f)
            rotationDirection = -Mathf.Sign(velocity.y);

        float rotationAmount = velocity.magnitude * rotationSpeed * Time.deltaTime * rotationDirection;

        ballVisual.Rotate(0f, 0f, rotationAmount);
    }


    //********* Obstacle Effect  (mud slow)  *********
    public void EnterMud(float speedMultiplier, float mudLinearDamping)
    {
        rb.linearVelocity *= speedMultiplier;
        rb.linearDamping = mudLinearDamping;
    }

    public void ExitMud()
    {
        rb.linearDamping = defaultLinearDamping;
        rb.angularDamping = defaultAngularDamping;
    }


    //********* VFX *********
    public void SpawnVfx(GameObject vfxPrefab)
    {
        if (vfxPrefab == null)
            return;
        Vector3 spawnPosition = transform.position;
        spawnPosition.z = -2f;
        GameObject spawnedVfx = Instantiate(vfxPrefab, spawnPosition, Quaternion.identity);

        float destroyDelay = 1.0f;
        if (destroyDelay > 0f)
            Destroy(spawnedVfx, destroyDelay);
    }


    //********* Sound *********
    private void PlaySound(AudioClip PlaySound)
    {
        if (audioSource == null || PlaySound == null)
            return;

        audioSource.PlayOneShot(PlaySound);
    }
}
