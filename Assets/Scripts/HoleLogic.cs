using System.Collections;
using UnityEngine;

public enum HoleType
{
    Good,
    Bad
}

[RequireComponent(typeof(Collider2D))]
public class HoleLogic : MonoBehaviour
{
    //Hole Type Good by Default
    [Header("Hole Settings")]
    [SerializeField] private HoleType holeType = HoleType.Good;

    //Holes Visual Setting, good, bad , in transition
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color goodColor = Color.blue;
    [SerializeField] private Color badColor = Color.red;
    [SerializeField] private Color warningColor = Color.white;
    [SerializeField] private float flashSpeed = 0.15f;

    public HoleType HoleType => holeType;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Collider2D holeCollider = GetComponent<Collider2D>();
        holeCollider.isTrigger = true;

        UpdateVisual();
    }

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

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = holeType == HoleType.Good ? goodColor : badColor;
    }
}