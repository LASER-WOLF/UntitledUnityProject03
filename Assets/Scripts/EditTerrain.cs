using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EditTerrain : MonoBehaviour
{
    public TerrainManager terrainManager;
    public DataManager dataManager;
    public UiManager uiManager;
    public MenuMain menuMain;

    GameObject selGo;
    GameObject unitGo;
    GameObject unitTextGo;
    
    Vector2Int selGp;
    Vector2Int mouseGp;

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
        uiManager.BottomText.SetActive(true);
        uiManager.EditTerrainPreviewGroup.SetActive(true);
        terrainManager.TerrainId = TerrainId;
        terrainManager.gameObject.SetActive(true);
        SetupCam();
    }
    private void OnDisable() {
        uiManager.BottomText.SetActive(false);
        uiManager.EditTerrainPreviewGroup.SetActive(false);
        terrainManager.gameObject.SetActive(false);
        }

    private void Awake() {
        modeMax = mode.Count;
        SetupScene();
    }

    void SetupCam() {
        Camera.main.transform.position = new Vector3(-72, 36, -70);
        Camera.main.transform.rotation = Quaternion.Euler(17.5f, 0, 0);
    }

    void SetupScene() {
        selGo = new GameObject("selectedGp", typeof(MeshFilter), typeof(MeshRenderer));
        selGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        selGo.GetComponent<Renderer>().material.color = new Color(0.85f, 0.5f, 0.5f, 0.9f);
        selGo.transform.SetParent(this.transform);

        unitGo = new GameObject("units", typeof(MeshFilter), typeof(MeshRenderer));
        unitGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/lit");
        unitGo.GetComponent<Renderer>().material.color = new Color(0.85f, 0.5f, 0.5f, 1f);
        unitGo.GetComponent<MeshFilter>().mesh.MarkDynamic();
        unitGo.transform.SetParent(this.transform);

        unitTextGo = new GameObject("unitsText", typeof(MeshFilter), typeof(MeshRenderer));
        unitTextGo.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlit");
        unitTextGo.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1f);
        unitTextGo.GetComponent<MeshFilter>().mesh.MarkDynamic();
        unitTextGo.transform.SetParent(this.transform);
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
            Vector2Int oldGp = selGp;
            ChangeSelGp(mouseGp);
            if ( oldGp != selGp || mouseInputRelease) {
                mouseInputRelease = false;
                //Debug.Log(terrainManager.GpToChunk(selGp).id);
                if (Input.GetMouseButton(0)) {
                    if (mode[selMode] == "unit") {
                        AddUnit(selGp, selId);
                    } else if (mode[selMode] == "grass") {
                        AddGrass(selGp, selId);
                    }
                } else if (Input.GetMouseButton(1)) {
                    if (mode[selMode] == "unit") {
                        RemoveUnit(selGp, selId);
                    } else if (mode[selMode] == "grass") {
                        RemoveGrass(selGp, selId);
                    }
                }
            }
            
        }
        else if (Input.GetKeyDown(KeyCode.Tab)) {
            ChangeMode();
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) {
            this.gameObject.SetActive(false);
            menuMain.gameObject.SetActive(true);
        }

        if (Input.mouseScrollDelta.y != 0) {
            ChangeSelId((int)Input.mouseScrollDelta.y * -1);
        }

        /*
        if (Input.GetKeyDown(KeyCode.Space)) { //temp
            terrainManager.GridGrassSetLods(Camera.main.transform.position);
        }
        */
    }

    void UpdateUi() {
        string textString = string.Empty;
        textString += "MODE [TAB]: " + mode[selMode].ToUpper();
        textString += " - " + " ADD/DEL [L-MOUSE] ";
        textString += " - " + " GP: " + selGp.x.ToString("D4") + "x" + selGp.y.ToString("D4");
        textString += " - " + " ID [M-WHEEL]: " + "< " + selId.ToString("D4") + " >";
        uiManager.SetBottomText(textString);
    }

    void UpdateMesh() {
        UpdateMeshUnit();
        UpdateMeshUnitText();
    }

    Vector2Int GetMouseGp() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit; if (Physics.Raycast(ray, out hit)) {
            return terrainManager.WorldtoGrid(hit.point);
        } else { return new Vector2Int(0,0); }
    }

    void ChangeMode() {
        selMode++;
        if (selMode > modeMax) { selMode = 1; }
        UpdatePreview(selId);
    }

    void ChangeSelId(int i) {
        selId = Utils.IntIncDec(i, selId, idMax, 1);
        /*
        if (mode[selMode] == "unit") {
            EditUnit(selGp, selId);
        } else if (mode[selMode] == "grass") {
            EditGrass(selGp, selId);
        }
        */
        UpdatePreview(selId);
    }

    void UpdatePreview(int id) {
        if (mode[selMode] == "grass") {
            uiManager.SetMeshEditTerrainPreview(GrassMeshGen.Create(dataManager.Grass[id], MeshGen.Create(MeshGen.MakeSquare(new Vector3(-0.5f, 0, -0.5f), 1, 1, 1))));
            uiManager.SetMatEditTerrainPreview(Resources.Load<Material>("Grass/" + dataManager.Grass[id].mat));
        }
    }

    void ChangeSelGp(Vector2Int gp) {
        if (selGp != gp) {
            selGo.GetComponent<Renderer>().enabled = true;
            Quaternion rotation = Quaternion.FromToRotation(transform.up, terrainManager.Grid[gp.x, gp.y].normal);
            selGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainGpDotRotated(rotation, terrainManager.Grid[gp.x, gp.y].position, 2, 2, 2, 0.02f));
            selGp = gp;
        }
        //if (mode[selMode] == "unit" && listManager.UnitsPlaced.ContainsKey(gp)) { selId = listManager.UnitsPlaced[gp]; }
        //else if (mode[selMode] == "grass" && listManager.GrassPlaced.Exists(g => g.terrainId == _terrainId && g.gp == gp)) { selId = listManager.GrassPlaced.Find(g => g.terrainId == _terrainId && g.gp == gp).grass.id; }
    }

    void AddUnit(Vector2Int gp, int id) {
        if (!dataManager.UnitsPlaced.ContainsKey(gp)) {
            dataManager.UnitsPlaced.Add(gp, id);
        }
    }

    void AddGrass(Vector2Int gp, int id) {
        //listManager.GrassPlaced.RemoveAll(g => g.terrainId == _terrainId && g.gp == gp);
        //listManager.GrassPlaced.Add(new GrassPlaced() { grass = listManager.Grass[id], gp = gp, terrainId = _terrainId });
        terrainManager.GridGrassAdd(gp,id);
    }

    void RemoveUnit(Vector2Int gp, int id) {
        if (dataManager.UnitsPlaced.ContainsKey(gp)) {
            dataManager.UnitsPlaced.Remove(gp);
        }
    }

    void RemoveGrass(Vector2Int gp, int id) {
        //if (listManager.GrassPlaced.Exists(g => g.terrainId == _terrainId && g.gp == gp)) {}
        //listManager.GrassPlaced.RemoveAll(g => g.terrainId == _terrainId && g.gp == gp);
        terrainManager.GridGrassRemove(gp);
    }

    void EditUnit(Vector2Int gp, int id) {
        if (dataManager.UnitsPlaced.ContainsKey(gp)) {
            dataManager.UnitsPlaced[gp] = id; 
        }
    }

    /*
    void EditGrass(Vector2Int gp, int id) {
        if (listManager.GrassPlaced.Exists(g => g.terrainId == _terrainId && g.gp == gp)) {
            listManager.GrassPlaced.Find(g => g.terrainId == _terrainId && g.gp == gp).grass = listManager.Grass[id];
            UpdateMeshGrass();
        }
    }
    */

    void UpdateMeshUnit() {
        unitGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainDictUnit(terrainManager.Grid, dataManager.UnitsPlaced, Quaternion.Euler(0, meshRotation, 0)));
        meshRotation++;
    }

    void UpdateMeshUnitText() {
        unitTextGo.GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainDictText(terrainManager.Grid, dataManager.UnitsPlaced));
    }


}


