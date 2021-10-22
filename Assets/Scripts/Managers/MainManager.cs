using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    GameObject cameraManagerGo;
    GameObject uiManagerGo;
    GameObject listManagerGo;
    GameObject terrainManagerGo;
    GameObject editTerrainGo;
    GameObject editUnitGo;
    GameObject menuMainGo;

    CameraManager cameraManager;
    UiManager uiManager;
    ListManager listManager;
    TerrainManager terrainManager;
    EditTerrain editTerrain;
    EditUnit editUnit;
    MenuMain menuMain;

    void Awake()
    {
        Setup();
        Dependencies();
        Enable();
    }

    void Setup() {
        cameraManagerGo = new GameObject("CameraManager");
        cameraManagerGo.SetActive(false);
        cameraManagerGo.AddComponent<CameraManager>();
        cameraManager = cameraManagerGo.GetComponent<CameraManager>();

        uiManagerGo = new GameObject("UiManager");
        uiManagerGo.SetActive(false);
        uiManagerGo.AddComponent<UiManager>();
        uiManager = uiManagerGo.GetComponent<UiManager>();


        listManagerGo = new GameObject("ListManager");
        listManagerGo.SetActive(false);
        listManagerGo.AddComponent<ListManager>();
        listManager = listManagerGo.GetComponent<ListManager>();

        terrainManagerGo = new GameObject("TerrainManager");
        terrainManagerGo.SetActive(false);
        terrainManagerGo.AddComponent<TerrainManager>();
        terrainManager = terrainManagerGo.GetComponent<TerrainManager>();

        editTerrainGo = new GameObject("EditTerrain");
        editTerrainGo.SetActive(false);
        editTerrainGo.AddComponent<EditTerrain>();
        editTerrain = editTerrainGo.GetComponent<EditTerrain>();

        editUnitGo = new GameObject("EditUnit");
        editUnitGo.SetActive(false);
        editUnitGo.AddComponent<EditUnit>();
        editUnit = editUnitGo.GetComponent<EditUnit>();

        menuMainGo = new GameObject("MenuMain");
        menuMainGo.SetActive(false);
        menuMainGo.AddComponent<MenuMain>();
        menuMain = menuMainGo.GetComponent<MenuMain>();
    }

    void Dependencies() {
        menuMain.uiManager = uiManager;
        menuMain.editTerrain = editTerrain;
        menuMain.editUnit = editUnit;
        menuMain.listManager = listManager;

        terrainManager.listManager = listManager;

        editTerrain.terrainManager = terrainManager;
        editTerrain.listManager = listManager;
        editTerrain.uiManager = uiManager;
        editTerrain.menuMain = menuMain;

        editUnit.listManager = listManager;
        editUnit.uiManager = uiManager;
        editUnit.menuMain = menuMain;

        uiManager.cameraManager = cameraManager;
    }

    void Enable() {
        cameraManagerGo.SetActive(true);
        uiManagerGo.SetActive(true);
        listManagerGo.SetActive(true);
        menuMainGo.SetActive(true);
    }
}
