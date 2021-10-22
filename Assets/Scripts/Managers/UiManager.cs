using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public CameraManager cameraManager;

    GameObject _uiGroup;
    GameObject _mainMenu;
    GameObject _bottomText;
    GameObject _editUnit;
    GameObject _editTerrainPreviewGroup;
    GameObject _editTerrainPreviewBg;
    GameObject _editTerrainPreview;
    GameObject _uiLight;
    public GameObject MainMenu {
        get => _mainMenu;
        set => _mainMenu = value;
    }

    public GameObject BottomText {
        get => _bottomText;
        set => _bottomText = value;
    }

    public GameObject EditUnit {
        get => _editUnit;
        set => _editUnit = value;
    }

    public GameObject EditTerrainPreviewGroup {
        get => _editTerrainPreviewGroup;
        set => _editTerrainPreviewGroup = value;
    }

    public void SetMainMenu(string text) {
        float size = 0.015f;
        Mesh mesh = new Mesh();
        if (!string.IsNullOrEmpty(text)) { mesh = MeshGen.Create(MeshGen.MakeGroupAutoAlphaNum(text, new Vector3(0, 0, 0), size, size, size), false, true); }
        _mainMenu.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetBottomText(string text) {
        float size = 0.015f;
        float offset = ((text.Length * size) / 2) *-1;
        Mesh mesh = new Mesh();
        if (!string.IsNullOrEmpty(text)) { mesh = MeshGen.Create(MeshGen.MakeGroupAutoAlphaNum(text, new Vector3(offset, 0, 0), size, size, size), false, true); }
        _bottomText.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetEditUnit(string text) {
        float size = 0.015f;
        Mesh mesh = new Mesh();
        if (!string.IsNullOrEmpty(text)) { mesh = MeshGen.Create(MeshGen.MakeGroupAutoAlphaNum(text, new Vector3(0, 0, 0), size, size, size), false, true); }
        _editUnit.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetMeshEditTerrainPreview(Mesh mesh) {
        _editTerrainPreview.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetMatEditTerrainPreview(Material mat) {
        _editTerrainPreview.GetComponent<Renderer>().material = mat;
    }

    private void Awake() {
        SetupUi();
    }

    void SetupUi() {
        _uiGroup = new GameObject("ui");
        _uiGroup.transform.SetParent(cameraManager.UiCamGo.transform);
        _uiGroup.transform.localPosition = new Vector3(0, 0, 0);
        _uiGroup.transform.localRotation = Quaternion.identity;
        _uiGroup.layer = 5;

        _uiLight = new GameObject("light", typeof(Light));
        _uiLight.transform.SetParent(_uiGroup.transform);
        _uiLight.transform.localPosition = new Vector3(0, 1, 0);
        _uiLight.transform.rotation = Quaternion.Euler(90, 0, 0);
        _uiLight.GetComponent<Light>().type = LightType.Directional;
        _uiLight.GetComponent<Light>().color = new Color(0.82f, 0.55f, 0.46f, 1.00f);
        _uiLight.GetComponent<Light>().intensity = 2f;
        _uiLight.GetComponent<Light>().shadows = LightShadows.Hard;
        _uiLight.GetComponent<Light>().cullingMask = 1 << 5;
        _uiLight.layer = 5;

        _mainMenu = new GameObject("mainMenu", typeof(MeshFilter), typeof(MeshRenderer));
        _mainMenu.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        _mainMenu.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _mainMenu.transform.SetParent(_uiGroup.transform);
        _mainMenu.transform.localPosition = new Vector3(0, 0f, 1);
        _mainMenu.transform.localRotation = Quaternion.identity;
        _mainMenu.SetActive(false);
        _mainMenu.layer = 5;

        _bottomText = new GameObject("bottomText", typeof(MeshFilter), typeof(MeshRenderer));
        _bottomText.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        _bottomText.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _bottomText.transform.SetParent(_uiGroup.transform);
        _bottomText.transform.localPosition = new Vector3(0, -0.57f, 1);
        _bottomText.transform.localRotation = Quaternion.identity;
        _bottomText.SetActive(false);
        _bottomText.layer = 5;

        _editUnit = new GameObject("editUnit", typeof(MeshFilter), typeof(MeshRenderer));
        _editUnit.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        _editUnit.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _editUnit.transform.SetParent(_uiGroup.transform);
        _editUnit.transform.localPosition = new Vector3(-0.8f, 0f, 1);
        _editUnit.transform.localRotation = Quaternion.identity;
        _editUnit.SetActive(false);
        _editUnit.layer = 5;

        _editTerrainPreviewGroup = new GameObject("editTerrainPreviewGroup");
        _editTerrainPreviewGroup.transform.SetParent(_uiGroup.transform);
        _editTerrainPreviewGroup.transform.localPosition = new Vector3(0, 0, 0);
        _editTerrainPreviewGroup.transform.localRotation = Quaternion.identity;
        _editTerrainPreviewGroup.SetActive(false);
        _editTerrainPreviewGroup.layer = 5;

        _editTerrainPreviewBg = new GameObject("editTerrainPreviewBg", typeof(MeshFilter), typeof(MeshRenderer));
        _editTerrainPreviewBg.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        _editTerrainPreviewBg.GetComponent<Renderer>().material.color = new Color(0.38f, 0.38f, 0.38f, 1.0f);
        _editTerrainPreviewBg.transform.SetParent(_editTerrainPreviewGroup.transform);
        _editTerrainPreviewBg.transform.localPosition = new Vector3(0, -4f, 10);
        _editTerrainPreviewBg.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        //_editTerrainPreviewBg.SetActive(false);
        _editTerrainPreviewBg.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeSquare(new Vector3(-0.75f, 0, -0.75f), 1.5f, 1, 1.5f));
        _editTerrainPreviewBg.layer = 5;

        _editTerrainPreview = new GameObject("editTerrainPreview", typeof(MeshFilter), typeof(MeshRenderer));
        _editTerrainPreview.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        _editTerrainPreview.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _editTerrainPreview.transform.SetParent(_editTerrainPreviewGroup.transform);
        _editTerrainPreview.transform.localPosition = new Vector3(0, -3.84f, 9.5f);
        _editTerrainPreview.transform.localRotation = Quaternion.Euler(-30, 45, -30);
        //_editTerrainPreview.SetActive(false);
        /*
        _editTerrainPreview.GetComponent<MeshFilter>().mesh = GrassMeshGen.Create(new GrassMeshGenSettings() {
            sourceMesh = MeshGen.Create(MeshGen.MakeSquare(new Vector3(-0.5f, 0, -0.5f), 1, 1, 1))
        });
        */
        _editTerrainPreview.layer = 5;

    }
}
