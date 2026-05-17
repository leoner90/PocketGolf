using System.Collections;
using UnityEngine;

//hole Types
public enum HoleType { Good, Bad }

//ref
[RequireComponent(typeof(Collider2D))]

public class Hole : MonoBehaviour
{
    //********** VARIABLES **********

    //Hole Type - Good by Default
    [Header("Hole Settings")]
    [SerializeField] private HoleType holeType = HoleType.Good;

    //Holes Visual Setting, good, bad, in-transit
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color goodColor = Color.blue;
    [SerializeField] private Color badColor = Color.red;
    [SerializeField] private Color warningColor = Color.white;
    [SerializeField] private float flashSpeed = 0.15f;

    public HoleType HoleType => holeType; // const getter

    //Save to which spawn index this hole belong too, so on restart we will not spawn in same spawn index at all.
    public int CurrentSpawnIndex { get; private set; } = -1;


    //********** AWAKE **********
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Collider2D holeCollider = GetComponent<Collider2D>();
        holeCollider.isTrigger = true;

        UpdateVisual();
    }


    //********** Hole Type Set/Toggle **********
    public void SetHoleType(HoleType newType)
    {
        holeType = newType;
        UpdateVisual();
    }

    public void ToggleHoleType()
    {
        holeType = holeType == HoleType.Good ? HoleType.Bad : HoleType.Good;
        UpdateVisual();
    }


    //********** Flash for 2 sec **********
    public IEnumerator FlashWarning(float duration)
    {
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;
        float timer = 0f;

        while (timer < duration)
        {
            spriteRenderer.color = warningColor;
            yield return new WaitForSeconds(flashSpeed);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashSpeed);

            timer += flashSpeed * 2f;
        }

        spriteRenderer.color = originalColor;
    }


    //********** Update color based on Hole type **********
    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = holeType == HoleType.Good ? goodColor : badColor;
    }


    //********** Save index this hole belong too - to delete used hole **********
    public void SetCurrentSpawnIndex(int spawnIndex)
    {
        CurrentSpawnIndex = spawnIndex;
    }
}