using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditUnit : MonoBehaviour {
    public DataManager dataManager;
    public UiManager uiManager;
    public MenuMain menuMain;
    
    GameObject platformGroup;
    GameObject platformBgGo;
    GameObject platformGo;
    GameObject light1;
    GameObject light2;
    GameObject unitGo;

    Dictionary<int, string> mode = new Dictionary<int, string>(){
    {1, "unit"},
    {2, "zone"}
    };
    int selMode = 1;
    int maxMode;

    Dictionary<int, string> inputMode = new Dictionary<int, string>(){
    {1, "default"},
    {2, "incDec"},
    {3, "text"}
    };
    int selInputMode = 1;

    Dictionary<int, string> menu = new Dictionary<int, string>(){
    {1, "name"},
    {2, "mesh"},
    {3, "material"},
    {4, "hp"}
    };
    int selMenu = 1;
    int maxMenu;

    int selId = 1;
    int maxId;

    string selName = string.Empty;

    Dictionary<int, string> meshes = new Dictionary<int, string>(){
    {1, "default"},
    {2, "female01"}
    };
    int selMesh = 1;
    int maxMesh;

    int selMat = 1;
    int maxMat = 1;

    int selHp;
    int maxHp = 9999;

    bool animTextBlink;

    private void Awake() {
        maxMode = mode.Count;
        maxMesh = meshes.Count;
        maxMenu = menu.Count;
        maxId = dataManager.Units.Count;
        SetupScene();
        SelIdInit(selId);
    }

    private void OnEnable() {
        uiManager.BottomText.SetActive(true);
        uiManager.EditUnit.SetActive(true);
        SetupCam();
    }
    private void OnDisable() {
        uiManager.BottomText.SetActive(false);
        uiManager.EditUnit.SetActive(false);
    }

    void Update() {
        GetInput();
    }
    void FixedUpdate() {
        UpdateUi();
        RotateMesh();
    }

    void SetupCam() {
        Camera.main.transform.position = new Vector3(0, 3, 0);
        Camera.main.transform.rotation = Quaternion.Euler(20, 0, 0);
    }

    void SetupScene() {
        platformGroup = new GameObject("platformGroup");
        platformGroup.transform.position = new Vector3(1.75f, 0, 5);
        platformGroup.transform.SetParent(this.transform);

        platformBgGo = new GameObject("platformBg", typeof(MeshFilter), typeof(MeshRenderer));
        platformBgGo.transform.SetParent(platformGroup.transform);
        platformBgGo.transform.localPosition = new Vector3(0, 0, 20);
        platformBgGo.transform.rotation = Quaternion.Euler(-90, 0, 0);
        platformBgGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeSquare(new Vector3(-50, 0, -50), 100, 1, 100));
        platformBgGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/lit");
        platformBgGo.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f, 1);
        platformBgGo.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        platformGo = new GameObject("platform", typeof(MeshFilter), typeof(MeshRenderer));
        platformGo.transform.SetParent(platformGroup.transform);
        platformGo.transform.localPosition = new Vector3(0, 0, 0);
        platformGo.transform.rotation = Quaternion.Euler(0, 45, 0);
        platformGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeSquare(new Vector3(-50,0,-50), 100, 1, 100));
        platformGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/lit");
        platformGo.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f, 1);
        platformGo.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        light1 = new GameObject("light1", typeof(Light));
        light1.transform.SetParent(platformGroup.transform);
        light1.transform.localPosition = new Vector3(0, 3.3f, 0);
        light1.transform.rotation = Quaternion.Euler(90, 0, 0);
        light1.GetComponent<Light>().type = LightType.Spot;
        light1.GetComponent<Light>().range = 10f;
        light1.GetComponent<Light>().innerSpotAngle = 22f;
        light1.GetComponent<Light>().spotAngle = 107f;
        light1.GetComponent<Light>().color = new Color(0.90f, 0.87f, 0.80f, 1.00f);
        light1.GetComponent<Light>().intensity = 4f;
        light1.GetComponent<Light>().shadows = LightShadows.Soft;

        light2 = new GameObject("light2", typeof(Light));
        light2.transform.SetParent(platformGroup.transform);
        light2.transform.localPosition = new Vector3(0.0f, 1.0f, -2.5f);
        light2.GetComponent<Light>().type = LightType.Spot;
        light2.GetComponent<Light>().range = 10f;
        light2.GetComponent<Light>().innerSpotAngle = 0f;
        light2.GetComponent<Light>().spotAngle = 100f;
        light2.GetComponent<Light>().color = new Color(0.90f, 0.87f, 0.80f, 1.00f);
        light2.GetComponent<Light>().intensity = 1.25f;

        unitGo = new GameObject("unit", typeof(MeshFilter), typeof(MeshRenderer));
        unitGo.GetComponent<MeshFilter>().mesh.MarkDynamic();
        unitGo.transform.SetParent(platformGroup.transform);
        unitGo.transform.localPosition = new Vector3(0, 0, 0);
        unitGo.transform.localRotation = Quaternion.identity;
    }

    void GetInput() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            //ModeChange();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            this.gameObject.SetActive(false);
            menuMain.gameObject.SetActive(true);
        }

        if (inputMode[selInputMode] == "default") {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { SelIdChange(-1); }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { SelIdChange(1); }
            if (Input.GetKeyDown(KeyCode.UpArrow)) { SelMenuChange(-1); }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { SelMenuChange(1); }
            if (Input.GetKeyDown(KeyCode.Return)) {
                if (menu[selMenu] == "name") { selInputMode = 3; }
                if (menu[selMenu] == "mesh") { selInputMode = 2; }
                if (menu[selMenu] == "material") { selInputMode = 2; }
                if (menu[selMenu] == "hp") { selInputMode = 2; }
            }
        }
        
        else if (inputMode[selInputMode] == "incDec") {
            if (Input.GetKeyDown(KeyCode.Return)) { selInputMode = 1; } 
            else {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) { ChangeValueIncDec(-1); }
                if (Input.GetKeyDown(KeyCode.RightArrow)) { ChangeValueIncDec(1); }
            }
        } 
        
        else if (inputMode[selInputMode] == "text") {
            if (Input.GetKeyDown(KeyCode.Return)) { selInputMode = 1; } 
            else {
                ChangeValueText();
            }
        }
    }
    
    void ChangeValueText() {
        if (menu[selMenu] == "name") { selName = TextEdit(selName); }
    }

    string TextEdit(string text = "") {
        if (Input.GetKeyDown(KeyCode.Backspace) && text.Length>0) { text = text.Remove(text.Length - 1); }
        if (Input.GetKeyDown(KeyCode.Space)) { text += " "; }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { text += "1"; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { text += "2"; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { text += "3"; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { text += "4"; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { text += "5"; }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { text += "6"; }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { text += "7"; }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { text += "8"; }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { text += "9"; }
        if (Input.GetKeyDown(KeyCode.Alpha0)) { text += "0"; }
        if (Input.GetKeyDown(KeyCode.A)) { text += "a"; }
        if (Input.GetKeyDown(KeyCode.B)) { text += "b"; }
        if (Input.GetKeyDown(KeyCode.C)) { text += "c"; }
        if (Input.GetKeyDown(KeyCode.D)) { text += "d"; }
        if (Input.GetKeyDown(KeyCode.E)) { text += "e"; }
        if (Input.GetKeyDown(KeyCode.F)) { text += "f"; }
        if (Input.GetKeyDown(KeyCode.G)) { text += "g"; }
        if (Input.GetKeyDown(KeyCode.H)) { text += "h"; }
        if (Input.GetKeyDown(KeyCode.I)) { text += "i"; }
        if (Input.GetKeyDown(KeyCode.J)) { text += "j"; }
        if (Input.GetKeyDown(KeyCode.K)) { text += "k"; }
        if (Input.GetKeyDown(KeyCode.L)) { text += "l"; }
        if (Input.GetKeyDown(KeyCode.M)) { text += "m"; }
        if (Input.GetKeyDown(KeyCode.N)) { text += "n"; }
        if (Input.GetKeyDown(KeyCode.O)) { text += "o"; }
        if (Input.GetKeyDown(KeyCode.P)) { text += "p"; }
        if (Input.GetKeyDown(KeyCode.Q)) { text += "q"; }
        if (Input.GetKeyDown(KeyCode.R)) { text += "r"; }
        if (Input.GetKeyDown(KeyCode.S)) { text += "s"; }
        if (Input.GetKeyDown(KeyCode.T)) { text += "t"; }
        if (Input.GetKeyDown(KeyCode.U)) { text += "u"; }
        if (Input.GetKeyDown(KeyCode.V)) { text += "v"; }
        if (Input.GetKeyDown(KeyCode.W)) { text += "w"; }
        if (Input.GetKeyDown(KeyCode.X)) { text += "x"; }
        if (Input.GetKeyDown(KeyCode.Y)) { text += "y"; }
        if (Input.GetKeyDown(KeyCode.Z)) { text += "z"; }
        /*
        if (Input.GetKeyDown(KeyCode.Period)) { text += "."; }
        if (Input.GetKeyDown(KeyCode.Comma)) { text += ","; }
        if (Input.GetKeyDown(KeyCode.Colon)) { text += ":"; }
        if (Input.GetKeyDown(KeyCode.Semicolon)) { text += ";"; }
        if (Input.GetKeyDown(KeyCode.Quote)) { text += "'"; }
        if (Input.GetKeyDown(KeyCode.DoubleQuote)) { text += "\""; }
        if (Input.GetKeyDown(KeyCode.Minus)) { text += "-"; }
        if (Input.GetKeyDown(KeyCode.Underscore)) { text += "_"; }
        if (Input.GetKeyDown(KeyCode.Slash)) { text += "/"; }
        if (Input.GetKeyDown(KeyCode.Backslash)) { text += "\\"; }
        if (Input.GetKeyDown(KeyCode.Pipe)) { text += "|"; }
        if (Input.GetKeyDown(KeyCode.LeftParen)) { text += "("; }
        if (Input.GetKeyDown(KeyCode.RightParen)) { text += ")"; }
        if (Input.GetKeyDown(KeyCode.LeftBracket)) { text += "["; }
        if (Input.GetKeyDown(KeyCode.RightBracket)) { text += "]"; }
        if (Input.GetKeyDown(KeyCode.Less)) { text += "<"; }
        if (Input.GetKeyDown(KeyCode.Greater)) { text += ">"; }
        if (Input.GetKeyDown(KeyCode.Equals)) { text += "="; }
        if (Input.GetKeyDown(KeyCode.Plus)) { text += "+"; }
        if (Input.GetKeyDown(KeyCode.Ampersand)) { text += "&"; }
        if (Input.GetKeyDown(KeyCode.Percent)) { text += "%"; }
        if (Input.GetKeyDown(KeyCode.Exclaim)) { text += "!"; }
        if (Input.GetKeyDown(KeyCode.Question)) { text += "?"; }
        if (Input.GetKeyDown(KeyCode.Hash)) { text += "#"; }
        if (Input.GetKeyDown(KeyCode.Asterisk)) { text += "*"; }
        if (Input.GetKeyDown(KeyCode.Dollar)) { text += "$"; }
        */
        return text;
    }

    void UpdateUi() {
        animTextBlink = !animTextBlink;
        string text1String = string.Empty;
        text1String += "MODE [TAB]: " + mode[selMode].ToUpper();
        text1String += " | " + " ID: ";
        if (inputMode[selInputMode] == "default") { text1String += "<"; } else { text1String += " "; }
        text1String += " ";
        text1String += selId.ToString("D4");
        text1String += " ";
        if (inputMode[selInputMode] == "default") { text1String += ">"; } else { text1String += " "; }
        //uiManager.ui.GetComponent<MeshFilter>().mesh = meshGenManager.MeshGen(meshGenManager.MeshGenMakeGroupAutoAlphaNum(text1String, Vector3.zero, 0.015f, 0.015f, 0.015f), false, true);
        uiManager.SetBottomText(text1String);

        /*
        foreach (var item in menu) {
            if (item.Key > 1) { text2String += "\n"; }
            text2String += MenuTypeIncDec(item.Value, selMesh);
        }
        */

        string text2String = string.Empty;

        text2String += "  EDIT UNIT";
        text2String += "\n" + "  ".PadRight(40,'-');
        text2String += MenuTypeText("name", selName);
        text2String += MenuTypeIncDec("mesh", selMesh, meshes[selMesh].ToUpper());
        text2String += MenuTypeIncDec("material", selMat, meshes[selMesh].ToUpper() + "-"+selMat.ToString("D2"));
        text2String += MenuTypeIncDec("hp", selHp);
        text2String += "\n" + "  ".PadRight(40, '-');

        //text2Go.GetComponent<MeshFilter>().mesh = meshGenManager.MeshGen(meshGenManager.MeshGenMakeGroupAutoAlphaNum(text2String, Vector3.zero, 0.015f, 0.015f, 0.015f), false, true);
        uiManager.SetEditUnit(text2String);
    }

    string MenuTypeText(string name, string value) {
        string text = string.Empty;
        text += "\n";
        if (menu[selMenu] == name && inputMode[selInputMode] == "default") { text += ">"; } else { text += " "; }
        text += " " + name.ToUpper() + ":";
        text = text.PadRight(15);
        text += "  ";
        text += value;
        if (menu[selMenu] == name && inputMode[selInputMode] == "text" && animTextBlink) { text += "<"; } else { text += " "; }
        return text;
    }

    string MenuTypeIncDec(string name, int value, string desc = "") {
        string text = string.Empty;
        text += "\n";
        if (menu[selMenu] == name && inputMode[selInputMode] == "default") { text += ">"; } else { text += " "; }
        text += " " + name.ToUpper() + ":";
        text = text.PadRight(15);
        if (menu[selMenu] == name && inputMode[selInputMode] == "incDec") { text += "<"; } else { text += " "; }
        text += " ";
        text += value.ToString("D4");
        if (desc != "") { text += " - " + desc; }
        text += " ";
        if (menu[selMenu] == name && inputMode[selInputMode] == "incDec") { text += ">"; } else { text += " "; }
        return text;
    }

    void ChangeValueIncDec(int i) {
        if (menu[selMenu] == "mesh") { SelMeshChange(i); }
        if (menu[selMenu] == "material") { SelMatChange(i); }
        if (menu[selMenu] == "hp") { SelHpChange(i); }
    }
    void SelMeshChange(int i) {
        selMesh = IntIncDec(i, selMesh, maxMesh);
        UpdateUnitMesh(selMesh);
    }
    void SelMatChange(int i) {
        selMat = IntIncDec(i, selMat, maxMat);
        UpdateUnitMat(selMesh,selMat);
    }

    void SelHpChange(int i) {
        selHp = IntIncDec(i, selHp, maxHp);
    }

    void SelMenuChange(int i) {
        selMenu = IntIncDec(i, selMenu, maxMenu);
    }

    void RotateMesh() {
        unitGo.transform.Rotate(0, 0.5f, 0);
    }

    void SelIdChange(int i, bool set = false) {
        SaveUnit(selId);
        if (set) {
            selId = i;
        } else {
            selId = IntIncDec(i, selId, maxId);
        }
        SelIdInit(selId);
    }

    void SelIdInit(int i) {
        //selMenu = 1;
        selName = dataManager.Units[i].name;
        selMesh = dataManager.Units[i].mesh;
        selHp = dataManager.Units[i].hp;
        UpdateUnitMesh(selMesh);
    }

    void SaveUnit(int id) {
        dataManager.Units[id].name = selName;
        dataManager.Units[id].mesh = selMesh;
        dataManager.Units[id].hp = selHp;
    }

    int IntIncDec(int i, int val, int max) {
        val += i;
        if (val < 1) { val = max; } else if (val > max) { val = 1; }
        return val;
    }

    void UpdateUnitMesh(int mesh = 1) {
        unitGo.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>("Units/Meshes/" + meshes[mesh]);
        UpdateUnitMat(mesh);
    }
    void UpdateUnitMat(int mesh, int mat = 1) {
        unitGo.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Units/Materials/" + meshes[mesh] + "-" + mat.ToString("D2"));
    }



    /*
    void ModeChange() {
        selMode++;
        if (selMode > modeMax) { selMode = 1; }
    }
    */
}


