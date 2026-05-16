using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoScale : MonoBehaviour
{
    [SerializeField] private float targetWidth = 5.6f;
    [SerializeField] private float targetHeight = 10f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        FitCamera();
    }

    private void FitCamera()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        float sizeByHeight = targetHeight / 2f;
        float sizeByWidth = targetWidth / screenAspect / 2f;

        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }
}