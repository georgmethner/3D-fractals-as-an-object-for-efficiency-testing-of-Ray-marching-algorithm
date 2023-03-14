using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    public Color fractalColor1;
    public Color fractalColor2;

    public float Value;
    public Vector2 PixelOffset;

    public ComputeShader RayMarchingShader;

    private Camera _camera;
    private float _lastFieldOfView;
    private RenderTexture _target;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ScreenCapture.CaptureScreenshot(Time.time + ".png");
        }

        if (_camera.fieldOfView != _lastFieldOfView)
        {
            _lastFieldOfView = _camera.fieldOfView;
        }
    }

    private void SetShaderParameters()
    {
        RayMarchingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayMarchingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayMarchingShader.SetVector("fractalColor1", fractalColor1);
        RayMarchingShader.SetVector("fractalColor2", fractalColor2);
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        RayMarchingShader.SetFloat("_Value", Value);
        RayMarchingShader.SetVector("_PixelOffset", PixelOffset);

        // Set the target and dispatch the compute shader
        RayMarchingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayMarchingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(_target, destination);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
}