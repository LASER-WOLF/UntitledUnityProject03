using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMain : MonoBehaviour {
    public UiManager uiManager;
    public ListManager listManager;
    public EditTerrain editTerrain;
    public EditUnit editUnit;

    GameObject bgGo;

    List<List<MenuItem>> menu = new List<List<MenuItem>>() {
        new List<MenuItem>() {          //0 main
            {new MenuItem(){ name = "edit terrain", type = "sel" } },   //0
            {new MenuItem(){ name = "edit units", type = "sel" } }      //1
        },
        new List<MenuItem>() {          //1 edit terrain
        }
    };

    int selMenu = 0;
    int selSubMenu = 0;
    int maxSubMenu;

    private void Awake() {
        SelMenuChange(selMenu);
        SetupScene();
        UpdateUi();
    }

    private void OnEnable() {
        uiManager.MainMenu.SetActive(true);
        SetupCam();
        SetupEditTerrainMenu();
    }
    private void OnDisable() {
        uiManager.MainMenu.SetActive(false);
    }

    void Update() {
        GetInput();
    }

    void SetupCam() {
        Camera.main.transform.position = new Vector3(0, 0, 0);
        Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void SetupScene() {
        bgGo = new GameObject("bg", typeof(MeshFilter), typeof(MeshRenderer));
        bgGo.transform.SetParent(this.transform);
        bgGo.transform.localPosition = new Vector3(0, 0, 20);
        bgGo.transform.rotation = Quaternion.Euler(-90, 0, 0);
        bgGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeSquare(new Vector3(-50, 0, -50), 100, 1, 100));
        bgGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/lit");
        bgGo.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.05f, 1);
        bgGo.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void SetupEditTerrainMenu() {
        foreach (TerrainInfo terrain in listManager.Terrains) {
            menu[1].Add(new MenuItem() { name = terrain.id.ToString("D2"), type = "sel" });
        }
    }

    void GetInput() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { SelSubMenuChange(-1); UpdateUi(); }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) { SelSubMenuChange(1); UpdateUi(); }
        else if (Input.GetKeyDown(KeyCode.Return)) {
            if (selMenu == 0 && selSubMenu == 0) { SelMenuChange(1); }
            else if (selMenu == 0 && selSubMenu == 1) { this.gameObject.SetActive(false); editUnit.gameObject.SetActive(true); }
            else if (selMenu == 1 && selSubMenu == 0) { this.gameObject.SetActive(false); editTerrain.TerrainId = 13; editTerrain.gameObject.SetActive(true); }
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) {
            SelMenuChange(0);
        }
    }

    void UpdateUi() {
        string menuString = string.Empty;

        menuString += "  MAIN MENU";
        menuString += "\n" + "  ".PadRight(40, '-');
        foreach (var item in menu[selMenu]) {
            menuString += MenuTypeSel(item.name);
        }
        menuString += "\n" + "  ".PadRight(40, '-');

        uiManager.SetMainMenu(menuString);
    }

    string MenuTypeSel(string name) {
        string text = string.Empty;
        text += "\n";
        if (menu[selMenu][selSubMenu].name == name) { text += ">"; } else { text += " "; }
        text += " " + name.ToUpper();
        return text;
    }

    void SelMenuChange(int i) {
        selMenu = i;
        selSubMenu = 0;
        maxSubMenu = menu[selMenu].Count - 1;
        UpdateUi();
    }

    void SelSubMenuChange(int i) {
        selSubMenu = Utils.IntIncDec(i, selSubMenu, maxSubMenu);
    }
}
