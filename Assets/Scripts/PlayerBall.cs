using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerBall : MonoBehaviour
{
    //defaul Ref
    private Rigidbody2D rb;
    private Camera mainCamera;
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
    public bool IsAiming => isAiming;

    //sounds 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip aimSound;
    [SerializeField] private AudioClip goodHoleSound;
    [SerializeField] private AudioClip badHoleSound;

    //start pos
    private Vector2 startPosition;

    //stop ball when velocity is low
    [SerializeField] private float snapStopVelocity = 0.15f;


    //rotation setings
    [Header("Visual Rotation")]
    [SerializeField] private Transform ballVisual;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float minRotationVelocity = 0.05f;


    //for ball enters mud  -> save/reset dampping
    private float defaultLinearDamping;
    private float defaultAngularDamping;

    //VFX ref
    [Header("VFX")]
    [SerializeField] private GameObject goodHoleHitVfx;
    [SerializeField] private GameObject badHoleHitVfx;
    [SerializeField] private GameObject resetBallVfx;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }

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


    private void Update()
    {
        SnapStopIfSlow(); // stop if too slow

        if (GameManager.GM != null && GameManager.GM.isGameOver)
        {
            CancelAim();
            StopBall();
            return;
        }
        RotateBallVisual();

        if (isAiming)
        {
            if (PointerHeld())
            {
                dragCurrentWorld = GetPointerWorldPosition();
                UpdateAimLine();
            }

            if (PointerUp())
            {
                Shoot();
            }

            return;
        }

        if (!CanShoot())
            return;

        if (PointerDown())
        {
            Vector2 pointerWorld = GetPointerWorldPosition();

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

    private bool CanShoot()
    {
        return rb.linearVelocity.magnitude <= stopVelocity && !isAiming;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        HoleLogic hole = other.GetComponent<HoleLogic>();

        if (hole == null)
            return;

        if (GameManager.GM != null)
            GameManager.GM.OnHoleHit(hole.HoleType);

        StopBall();

        //vfx
        GameObject selectedVfx = hole.HoleType == HoleType.Good ? goodHoleHitVfx : badHoleHitVfx;
        SpawnVfx(selectedVfx);

        //sound
        AudioClip selectedSound = hole.HoleType == HoleType.Good ? goodHoleSound : badHoleSound;
        PlaySound(selectedSound);

        //hide ball sprite and wait before reset the possition
        if (ballRenderer != null)
            ballRenderer.enabled = false;
        ResetBallToStart();
      

    }

    private IEnumerator ResetBallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetBallVisual();
    }


    private void StopBall()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }


    //********* Controllers *********
    private Vector2 GetPointerWorldPosition()
    {
        Vector3 screenPosition;

        if (Input.touchCount > 0)
            screenPosition = Input.GetTouch(0).position;
        else
            screenPosition = Input.mousePosition;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private bool PointerDown()
    {
        return Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    }

    private bool PointerHeld()
    {
        return Input.GetMouseButton(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary);
    }

    private bool PointerUp()
    {
        return Input.GetMouseButtonUp(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
    }

    private void PlaySound(AudioClip PlaySound)
    {
        if (audioSource == null || PlaySound == null)
            return;

        audioSource.PlayOneShot(PlaySound);
    }


    public void ResetBallToStart()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = startPosition;

        StartCoroutine(ResetBallAfterDelay(1.0f));
    }

    public void ResetBallVisual()
    {
        if (ballRenderer != null)
            ballRenderer.enabled = true;
        SpawnVfx(resetBallVfx);
    }

    //Stop ball Velocity if to slow
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
}
