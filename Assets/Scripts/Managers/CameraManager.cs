using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    GameObject _mainCamGo;
    GameObject _uiCamGo;
    GameObject _mainPostProcessVolumeGo;
    GameObject _uiPostProcessVolumeGo;
    Camera _mainCam;
    Camera _uiCam;
    UniversalAdditionalCameraData _mainCamUacd;
    UniversalAdditionalCameraData _uiCamUacd;
    Volume _mainPostProcessVolume;
    Volume _uiPostProcessVolume;

    public GameObject MainCamGo {
        get => _mainCamGo;
        set => _mainCamGo = value;
    }

    public GameObject UiCamGo {
        get => _uiCamGo;
        set => _uiCamGo = value;
    }

    void Awake()
    {
        SetupCam();
    }

    void SetupCam() {
        _mainCamGo = new GameObject("mainCam", typeof(Camera), typeof(UniversalAdditionalCameraData));
        _mainCamGo.tag = "MainCamera";
        _mainCam = _mainCamGo.GetComponent<Camera>();
        _mainCam.cullingMask = LayerMask.GetMask("Default");
        _mainCamUacd = _mainCam.GetComponent<UniversalAdditionalCameraData>();
        _mainCamUacd.renderPostProcessing = true;
        _mainCamUacd.renderShadows = true;
        _mainCamUacd.dithering = true;
        _mainCamUacd.renderType = CameraRenderType.Base;

        _mainPostProcessVolumeGo = new GameObject("mainPostProcessing", typeof(Volume));
        _mainPostProcessVolumeGo.transform.SetParent(_mainCamGo.transform);
        _mainPostProcessVolume = _mainPostProcessVolumeGo.GetComponent<Volume>();
        _mainPostProcessVolume.sharedProfile = Resources.Load<VolumeProfile>("PostProcessingMain");


        _uiCamGo = new GameObject("uiCam", typeof(Camera), typeof(UniversalAdditionalCameraData));
        _uiCam = _uiCamGo.GetComponent<Camera>();
        _uiCamUacd = _uiCam.GetComponent<UniversalAdditionalCameraData>();
        _uiCamUacd.renderPostProcessing = true;
        _uiCamUacd.renderShadows = true;
        _uiCamUacd.dithering = true;
        _uiCam.cullingMask = 1 << 5;
        _uiCamUacd.renderType = CameraRenderType.Overlay;
        _uiCamUacd.volumeLayerMask = 1 << 5;

        _uiPostProcessVolumeGo = new GameObject("uiPostProcessing", typeof(Volume));
        _uiPostProcessVolumeGo.transform.SetParent(_uiCamGo.transform);
        _uiPostProcessVolumeGo.layer = 5;
        _uiPostProcessVolume = _uiPostProcessVolumeGo.GetComponent<Volume>();
        _uiPostProcessVolume.sharedProfile = Resources.Load<VolumeProfile>("PostProcessingUi");

        _mainCamUacd.cameraStack.Add(_uiCam);
    }

}
