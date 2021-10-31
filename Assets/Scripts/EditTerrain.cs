using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EditTerrain : MonoBehaviour {
    public TerrainManager terrainManager;
    public DataManager dataManager;
    public UiManager uiManager;
    public MenuMain menuMain;

    GameObject selGo;
    GameObject unitGo;
    GameObject unitTextGo;

    GridPoint selGp = new GridPoint();
    GridPoint mouseGp;

    int _terrainId;
    bool mouseInputRelease;

    public int TerrainId {
        get => _terrainId;
        set => _terrainId = value;
    }

    //change to list
    Dictionary<int, string> mode = new Dictionary<int, string>(){
    {1, "unit"},
    {2, "grass"}
    };
    int selMode = 1;
    int modeMax;

    int selId = 1;
    int idMax = 9999;

    //del this
    int meshRotation;

    private void OnEnable() {
        Debug.Log("Starting terrain editor");
        uiManager.TopText.SetActive(true);
        uiManager.BottomText.SetActive(true);
        uiManager.EditTerrainPreviewGroup.SetActive(true);
        terrainManager.TerrainId = TerrainId;
        terrainManager.gameObject.SetActive(true);
        SetupCam();
    }
    private void OnDisable() {
        uiManager.TopText.SetActive(false);
        uiManager.BottomText.SetActive(false);
        uiManager.EditTerrainPreviewGroup.SetActive(false);
        terrainManager.gameObject.SetActive(false);
    }

    private void Awake() {
        modeMax = mode.Count;
        SetupScene();
    }

    void SetupCam() {
        Camera.main.transform.position = new Vector3(-72, 40, -70);
        Camera.main.transform.rotation = Quaternion.Euler(25f, 0, 0);
    }

    void SetupScene() {
        selGo = new GameObject("selectedGp", typeof(MeshFilter), typeof(MeshRenderer));
        selGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        selGo.GetComponent<Renderer>().material.color = new Color(0.85f, 0.5f, 0.5f, 0.9f);
        selGo.transform.SetParent(this.transform);
        selGo.transform.localPosition = new Vector3(0, .2f, 0);

        unitGo = new GameObject("units", typeof(MeshFilter), typeof(MeshRenderer));
        unitGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/lit");
        unitGo.GetComponent<Renderer>().material.color = new Color(0.85f, 0.5f, 0.5f, 1f);
        unitGo.GetComponent<MeshFilter>().mesh.MarkDynamic();
        unitGo.transform.SetParent(this.transform);
        unitGo.transform.localPosition = new Vector3(0, 0.2f, 0);

        unitTextGo = new GameObject("unitsText", typeof(MeshFilter), typeof(MeshRenderer));
        unitTextGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        unitTextGo.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1f);
        unitTextGo.GetComponent<MeshFilter>().mesh.MarkDynamic();
        unitTextGo.transform.SetParent(this.transform);
        unitTextGo.transform.localPosition = new Vector3(0, 0, 0);
    }

    void Update() {
        GetInput();
    }
    void FixedUpdate() {
        UpdateUi();
        UpdateMesh();
    }

    void GetInput() {
        mouseGp = GetMouseGp();
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) {
            mouseInputRelease = true;
        }
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
            GridPoint oldGp = selGp;
            ChangeSelGp(mouseGp);
            if (oldGp != selGp || mouseInputRelease) {
                mouseInputRelease = false;
                //Debug.Log(terrainManager.GpToChunk(selGp).id);
                if (Input.GetMouseButton(0)) {
                    if (mode[selMode] == "unit") {
                        AddUnit(selGp, dataManager.Units[selId]);
                    } else if (mode[selMode] == "grass") {
                        AddGrass(selGp, dataManager.Grass[selId]);
                    }
                } else if (Input.GetMouseButton(1)) {
                    if (mode[selMode] == "unit") {
                        RemoveUnit(selGp);
                    } else if (mode[selMode] == "grass") {
                        RemoveGrass(selGp);
                    }
                }
            }
        } else if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.G)) {
            terrainManager.GridGrassFillTerrain();
        } else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.G)) {
            terrainManager.GrassGenTexture();
        } else if (Input.GetKeyDown(KeyCode.Tab)) {
            ChangeMode();
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            this.gameObject.SetActive(false);
            menuMain.gameObject.SetActive(true);
        }

        if (Input.mouseScrollDelta.y != 0) {
            ChangeSelId((int)Input.mouseScrollDelta.y * -1);
        }
    }

    void UpdateUi() {
        string uiTopTextString = string.Empty;
        uiTopTextString += "[G] SPAWN GRASS";
        uiTopTextString += " - " + "[SHIFT+G] GENERATE TEXTURE";
        uiManager.SetTopText(uiTopTextString);

        string uiBottomTextString = string.Empty;
        uiBottomTextString += "MODE [TAB]: " + mode[selMode].ToUpper();
        uiBottomTextString += " - " + " ADD/DEL [L-MOUSE] ";
        uiBottomTextString += " - " + " GP: " + selGp.x.ToString("D4") + "x" + selGp.z.ToString("D4");
        uiBottomTextString += " - " + " ID [M-WHEEL]: " + "< " + selId.ToString("D4") + " >";
        uiManager.SetBottomText(uiBottomTextString);
    }

    void UpdateMesh() {
        UpdateMeshUnit();
        UpdateMeshUnitText();
    }

    GridPoint GetMouseGp() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit; if (Physics.Raycast(ray, out hit)) {
            return terrainManager.WorldtoGrid(hit.point);
        } else { return null; }
    }

    void ChangeMode() {
        selMode++;
        if (selMode > modeMax) { selMode = 1; }
        UpdatePreview(selId);
    }

    void ChangeSelId(int i) {
        selId = Utils.IntIncDec(i, selId, idMax, 1);
        UpdatePreview(selId);
    }

    void UpdatePreview(int id) {
        if (mode[selMode] == "grass") {
            uiManager.SetMeshEditTerrainPreview(GrassMeshGen.Create(dataManager.Grass[id], MeshGen.Create(MeshGen.MakeSquare(new Vector3(-0.5f, 0, -0.5f), 1, 1, 1))));
            uiManager.SetMatEditTerrainPreview(Resources.Load<Material>("Grass/" + dataManager.Grass[id].mat));
        }
    }

    void ChangeSelGp(GridPoint gp) {
        if (selGp != gp) {
            selGo.GetComponent<Renderer>().enabled = true;
            Quaternion rotation = Quaternion.FromToRotation(transform.up, gp.normalCenter);
            selGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.ModRotate(MeshGen.MakeTerrainGpDot(gp.posCenter, 2), rotation, gp.posCenter));
            selGp = gp;
        }
    }
    
    void AddGrass(GridPoint gp, Grass grass) {
        terrainManager.GridGrassAdd(gp, grass);
    }
    
    void RemoveGrass(GridPoint gp) {
        terrainManager.GridGrassRemove(gp);
    }
    
    void AddUnit(GridPoint gp, Unit unit) {
        if (!dataManager.UnitsPlaced.ContainsKey(gp)) {
            dataManager.UnitsPlaced.Add(gp, unit);
        }
    }

    void RemoveUnit(GridPoint gp) {
        if (dataManager.UnitsPlaced.ContainsKey(gp)) {
            dataManager.UnitsPlaced.Remove(gp);
        }
    }


    /*
    void EditUnit(GridPoint gp, Unit unit) {
        if (dataManager.UnitsPlaced.ContainsKey(gp)) {
            dataManager.UnitsPlaced[gp] = unit;
        }
    }
    */
    /*
    void EditGrass(Vector2Int gp, int id) {
        if (listManager.GrassPlaced.Exists(g => g.terrainId == _terrainId && g.gp == gp)) {
            listManager.GrassPlaced.Find(g => g.terrainId == _terrainId && g.gp == gp).grass = listManager.Grass[id];
            UpdateMeshGrass();
        }
    }
    */

    void UpdateMeshUnit() {
        unitGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainDictUnit(dataManager.UnitsPlaced, Quaternion.Euler(0, meshRotation, 0)));
        meshRotation++;
    }

    void UpdateMeshUnitText() {
        unitTextGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainDictText(dataManager.UnitsPlaced));
    }


}


