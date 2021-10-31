using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    GameObject cameraManagerGo;
    GameObject uiManagerGo;
    GameObject dataManagerGo;
    GameObject terrainManagerGo;
    GameObject editTerrainGo;
    GameObject editUnitGo;
    GameObject menuMainGo;

    CameraManager cameraManager;
    UiManager uiManager;
    DataManager dataManager;
    TerrainManager terrainManager;
    EditTerrain editTerrain;
    EditUnit editUnit;
    MenuMain menuMain;

    void Awake()
    {
        Debug.Log("Starting game");
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


        dataManagerGo = new GameObject("ListManager");
        dataManagerGo.SetActive(false);
        dataManagerGo.AddComponent<DataManager>();
        dataManager = dataManagerGo.GetComponent<DataManager>();

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
        menuMain.dataManager = dataManager;

        terrainManager.dataManager = dataManager;

        editTerrain.terrainManager = terrainManager;
        editTerrain.dataManager = dataManager;
        editTerrain.uiManager = uiManager;
        editTerrain.menuMain = menuMain;

        editUnit.dataManager = dataManager;
        editUnit.uiManager = uiManager;
        editUnit.menuMain = menuMain;

        uiManager.cameraManager = cameraManager;
    }

    void Enable() {
        cameraManagerGo.SetActive(true);
        uiManagerGo.SetActive(true);
        dataManagerGo.SetActive(true);
        menuMainGo.SetActive(true);
    }
}
