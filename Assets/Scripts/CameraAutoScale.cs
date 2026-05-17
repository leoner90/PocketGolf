using UnityEngine;

[RequireComponent(typeof(Camera))]


public class CameraAutoScale : MonoBehaviour
{
   //********** VARIABLES**********
   private float mobileTargetWidth = 5.1f;
   private float mobileTargetHeight = 9.3f;
    [SerializeField] private float desktopTargetWidth = 5f;
    [SerializeField] private float desktopTargetHeight = 11f;

    private Camera cam;


    //********** Awake **********
    private void Awake()
    {
        cam = GetComponent<Camera>();
        FitCamera();
    }

    //********** Try to fit mobile screen ....... **********
    private void FitCamera()
    {
        float targetWidth;
        float targetHeight;

        if (Application.isMobilePlatform)
        {
            targetWidth = mobileTargetWidth;
            targetHeight = mobileTargetHeight;
        }
        else
        {
            targetWidth = desktopTargetWidth;
            targetHeight = desktopTargetHeight;
        }

        float screenAspect = (float)Screen.width / Screen.height;

        float sizeByHeight = targetHeight / 2f;
        float sizeByWidth = targetWidth / screenAspect / 2f;

        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }
}
 

 