using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBallInput : MonoBehaviour
{
    private Camera mainCamera;

    //********** Controllers new input system (no actions input needed, as it's simple game -> direct usage) **********

    //********** AWAKE **********
    private void Awake()
    {
        mainCamera = Camera.main;
    }


    //********** Converts Mouse/Finger Screen position into world position **********
    public Vector2 GetPointerWorldPosition()
    {
        Vector2 screenPosition = GetPointerScreenPosition();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        return new Vector2(worldPosition.x, worldPosition.y);
    }


    //********** Mouse Or Finger Screen Pos Return **********
    private Vector2 GetPointerScreenPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Vector2.zero;
    }


    //********** Mouse Or Finger is Pressed **********
    public bool PointerDown()
    {
        bool mouseDown = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touchDown = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        return mouseDown || touchDown;
    }


    //********** Mouse Or Finger is Held **********
    public bool PointerHeld()
    {
        bool mouseHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool touchHeld = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;

        return mouseHeld || touchHeld;
    }


    //********** Mouse Or Finger Up **********
    public bool PointerUp()
    {
        bool mouseUp = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        bool touchUp = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;

        return mouseUp || touchUp;
    }
}

