using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor;

public class TerrainManager : MonoBehaviour {
    public DataManager dataManager;

    GameObject terrainGo;
    GameObject gridGo;
    GameObject grassGo;
    GameObject lightSun;
    Dictionary<Chunk, GameObject> terrainChunkGoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> terrainLod0GoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> terrainLod1GoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> terrainLod2GoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> terrainLod3GoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> terrainLod4GoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> gridChunkGoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> gridGoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> gridSteepGoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, GameObject> grassChunkGoCollection = new Dictionary<Chunk, GameObject>();
    Dictionary<Chunk, Dictionary<Grass, GameObject>> grassLod0GoCollection = new Dictionary<Chunk, Dictionary<Grass, GameObject>>();

    Vector3 _terrainSize;
    Vector3Int _terrainSizeInt;
    Vector3 _terrainPos;

    GridPoint[,] _grid;
    Chunk[,] _chunkGrid;

    int _terrainId;
    float angle = 30; //placeholder
    int chunkSize = 50;
    int lod1Size = 2;
    int lod2Size = 5;
    int lod3Size = 10;
    int lod4Size = 25;

    public int TerrainId {
        get => _terrainId;
        set => _terrainId = value;
    }

    public GridPoint[,] Grid {
        get => _grid;
        set => _grid = value;
    }

    public Chunk[,] ChunkGrid {
        get => _chunkGrid;
        set => _chunkGrid = value;
    }

    public Vector3 TerrainSize {
        get => _terrainSize;
        set => _terrainSize = value;
    }

    public Vector3Int TerrainSizeInt {
        get => _terrainSizeInt;
        set => _terrainSizeInt = value;
    }

    public Vector3 TerrainPos {
        get => _terrainPos;
        set => _terrainPos = value;
    }

    void Awake() {
        Debug.Log("Starting TerrainManager");
        SetupTerrain();
        //GridGrassFillTerrain();
    }

    private void Update() {
        //RotateSun();
    }

    void SetupTerrain() {
        Debug.Log("Setting up terrain");
        //Terrain parent GO
        terrainGo = new GameObject("Terrain");
        terrainGo.transform.SetParent(this.transform);
        terrainGo.transform.localPosition = new Vector3(0, 0, 0);
        //Grass parent GO
        grassGo = new GameObject("Grass");
        grassGo.transform.SetParent(this.transform);
        grassGo.transform.localPosition = new Vector3(0, 0.001f, 0);
        //Grid parent GO
        gridGo = new GameObject("Grid");
        gridGo.transform.SetParent(this.transform);
        gridGo.transform.localPosition = new Vector3(0, .2f, 0);
        //Setup terrain & grid
        ProcessRawTerrain();
        GridMeshGen(angle);
        //Setup light
        lightSun = new GameObject("LightSun", typeof(Light));
        lightSun.transform.SetParent(this.transform);
        lightSun.transform.localPosition = new Vector3(0, 50, 0);
        lightSun.transform.rotation = Quaternion.Euler(30f, -213, 0);
        lightSun.GetComponent<Light>().type = LightType.Directional;
        lightSun.GetComponent<Light>().color = new Color(0.8867924f, 0.7770787f, 0.6734603f, 1.00f);
        lightSun.GetComponent<Light>().intensity = 1.75f;
        lightSun.GetComponent<Light>().shadows = LightShadows.None;
        lightSun.GetComponent<Light>().cullingMask = 1;
        RenderSettings.sun = lightSun.GetComponent<Light>();
        RenderSettings.skybox = Resources.Load<Material>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-skybox");
    }

    void ProcessRawTerrain() {
        Debug.Log("Processing raw terrain");
        GameObject rawTerrain = new GameObject("RawTerrain", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        rawTerrain.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        rawTerrain.transform.SetParent(this.transform);
        rawTerrain.GetComponent<MeshFilter>().sharedMesh = Resources.Load<Mesh>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mesh");
        rawTerrain.GetComponent<MeshCollider>().sharedMesh = Resources.Load<Mesh>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mesh");
        TerrainSize = rawTerrain.GetComponent<MeshRenderer>().bounds.size;
        TerrainPos = rawTerrain.transform.position;
        TerrainSizeInt = new Vector3Int(Mathf.FloorToInt(TerrainSize.x), Mathf.FloorToInt(TerrainSize.y), Mathf.FloorToInt(TerrainSize.z));
        Grid = GridGen(TerrainSizeInt);
        ChunkGrid = ChunkGridGen(TerrainSizeInt);
        TerrainMeshGen();
        Destroy(rawTerrain);
        //rawTerrain.SetActive(false);
        //Grid = GridGen(TerrainSizeInt);
    }

    public void GridGrassFillTerrain() {
        //Grass
        GridGrassAutoGen(Grid, dataManager.Grass[3], 0, 85, 00.0f, 01.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[4], 0, 80, 01.8f, 02.4f);
        GridGrassAutoGen(Grid, dataManager.Grass[5], 0, 75, 02.4f, 03.6f);
        GridGrassAutoGen(Grid, dataManager.Grass[4], 0, 80, 03.6f, 05.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[5], 0, 75, 05.8f, 07.4f);
        GridGrassAutoGen(Grid, dataManager.Grass[3], 0, 85, 07.4f, 09.2f);
        GridGrassAutoGen(Grid, dataManager.Grass[2], 0, 105, 09.2f, 11.6f);
        GridGrassAutoGen(Grid, dataManager.Grass[4], 0, 80, 11.6f, 13.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[3], 0, 85, 13.8f, 15.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[5], 0, 75, 15.8f, 17.2f);
        GridGrassAutoGen(Grid, dataManager.Grass[4], 0, 80, 17.2f, 19.6f);
        GridGrassAutoGen(Grid, dataManager.Grass[5], 0, 75, 19.6f, 20.2f);
        GridGrassAutoGen(Grid, dataManager.Grass[3], 0, 85, 20.2f, 22.2f);
        GridGrassAutoGen(Grid, dataManager.Grass[1], 0, 125, 22.2f, 24.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[2], 0, 105, 24.8f, 27.4f);
        GridGrassAutoGen(Grid, dataManager.Grass[1], 0, 125, 27.4f, 30.2f);
        GridGrassAutoGen(Grid, dataManager.Grass[2], 0, 105, 30.2f, 34.6f);
        GridGrassAutoGen(Grid, dataManager.Grass[1], 0, 125, 34.6f, 38.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[2], 0, 105, 38.8f, 39.2f);
        GridGrassAutoGen(Grid, dataManager.Grass[1], 0, 125, 39.2f, 44.4f);
        //Snow
        GridGrassAutoGen(Grid, dataManager.Grass[17], 120, 130, 00.6f, 17.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[17], 130, 135, 00.0f, 19.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[18], 120, 130, 20.2f, 22.4f);
        GridGrassAutoGen(Grid, dataManager.Grass[18], 130, 135, 19.8f, 24.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[16], 120, 130, 26.2f, 38.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[16], 130, 135, 24.8f, 88.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[19], 135, 200, 00.0f, 19.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[20], 135, 200, 19.8f, 48.8f);
        GridGrassAutoGen(Grid, dataManager.Grass[18], 135, 200, 48.8f, 88.8f);
        //MeshGen
        GridGrassMeshGenAll();
    }

    /*
    void RotateSun() {
        lightSun.transform.Rotate(0.025f, 0, 0);
    }
    */

    public GridPoint WorldtoGrid(Vector3 pos) {
        int x = (int)((pos.x + (TerrainSize.x / 2)) - TerrainPos.x);
        int z = (int)((pos.z + (TerrainSize.z / 2)) - TerrainPos.z);
        return Grid[x, z];
    }

    public Vector3 GridToWorld(GridPoint gp, bool enableHeight = true) {
        float y = 0; if (enableHeight) { y = gp.posCenter.y; }
        float x = (gp.x - (TerrainSize.x / 2)) + TerrainPos.x;
        float z = (gp.z - (TerrainSize.z / 2)) + TerrainPos.z;
        return new Vector3(x, y, z);
    }

    public Chunk GpToChunk(GridPoint gp) {
        Chunk chunk = new Chunk();
        foreach (Chunk item in ChunkGrid) {
            if ((gp.x >= item.start.x && gp.x < item.end.x) && (gp.z >= item.start.z && gp.z < item.end.z)) {
                chunk = item;
            }
        }
        return chunk;
    }

    GridPoint[,] GridGen(Vector3Int terrainSize) {
        Debug.Log("Generating grid");
        GridPoint[,] gridGen = new GridPoint[terrainSize.x, terrainSize.z];
        for (int x = 0; x < terrainSize.x; x++) {
            for (int z = 0; z < terrainSize.z; z++) {
                gridGen[x, z] = new GridPoint();
                gridGen[x, z].x = x;
                gridGen[x, z].z = z;
                Vector3 posGrid = GridToWorld(gridGen[x, z], false);
                Vector3 posCenter = posGrid + new Vector3(0.5f, 0, 0.5f);
                Vector3 rayStartCenter = posCenter + new Vector3(0, 10000, 0);
                Ray rayCenter = new Ray(rayStartCenter, new Vector3(0, -1, 0));
                RaycastHit hitCenter; if (Physics.Raycast(rayCenter, out hitCenter)) {
                    Vector3[] pos = new Vector3[4];
                    Vector3[] normal = new Vector3[4];
                    Vector2[] uv = new Vector2[4];
                    Vector3[] rayStart = new Vector3[4];
                    Ray[] ray = new Ray[4];
                    RaycastHit[] hit = new RaycastHit[4];
                    rayStart[0] = posGrid + new Vector3(0.0f, 100000, 1.0f);
                    rayStart[1] = posGrid + new Vector3(1.0f, 100000, 1.0f);
                    rayStart[2] = posGrid + new Vector3(0.0f, 100000, 0.0f);
                    rayStart[3] = posGrid + new Vector3(1.0f, 100000, 0.0f);
                    ray[0] = new Ray(rayStart[0], new Vector3(0, -1, 0));
                    ray[1] = new Ray(rayStart[1], new Vector3(0, -1, 0));
                    ray[2] = new Ray(rayStart[2], new Vector3(0, -1, 0));
                    ray[3] = new Ray(rayStart[3], new Vector3(0, -1, 0));
                    if (Physics.Raycast(ray[0], out hit[0])) { pos[0] = hit[0].point; normal[0] = hit[0].normal; uv[0] = hit[0].textureCoord; }
                    if (Physics.Raycast(ray[1], out hit[1])) { pos[1] = hit[1].point; normal[1] = hit[1].normal; uv[1] = hit[1].textureCoord; }
                    if (Physics.Raycast(ray[2], out hit[2])) { pos[2] = hit[2].point; normal[2] = hit[2].normal; uv[2] = hit[2].textureCoord; }
                    if (Physics.Raycast(ray[3], out hit[3])) { pos[3] = hit[3].point; normal[3] = hit[3].normal; uv[3] = hit[3].textureCoord; }
                    gridGen[x, z].inBounds = true;
                    gridGen[x, z].posCenter = hitCenter.point;
                    gridGen[x, z].uvCenter = hitCenter.textureCoord;
                    gridGen[x, z].normalCenter = hitCenter.normal;
                    gridGen[x, z].angleCenter = Vector3.Angle(hitCenter.normal, rayStartCenter);
                    gridGen[x, z].pos = pos;
                    gridGen[x, z].uv = uv;
                    gridGen[x, z].normal = normal;
                }
            }
        }
        Debug.Log(terrainSize.x + "x" + terrainSize.z + " grid generated");
        return gridGen;
    }

    Chunk[,] ChunkGridGen(Vector3Int size) {
        Debug.Log("Generating chunk grid");
        int chunkNumX = Mathf.CeilToInt((float)size.x / (float)chunkSize);
        int chunkNumZ = Mathf.CeilToInt((float)size.z / (float)chunkSize);
        Chunk[,] chunkGridGen = new Chunk[chunkNumX, chunkNumZ];
        int chunkCount = 0;
        for (int chunkX = 0; chunkX < chunkNumX; chunkX++) {
            int startX = chunkX * chunkSize;
            int endX = (chunkX + 1) * chunkSize;
            if (endX >= size.x) { endX = size.x - 1; }
            for (int chunkZ = 0; chunkZ < chunkNumZ; chunkZ++) {
                int startZ = chunkZ * chunkSize;
                int endZ = (chunkZ + 1) * chunkSize;
                if (endZ >= size.z) { endZ = size.z - 1; }
                chunkGridGen[chunkX, chunkZ] = new Chunk();
                chunkGridGen[chunkX, chunkZ].x = chunkX;
                chunkGridGen[chunkX, chunkZ].z = chunkZ;
                chunkGridGen[chunkX, chunkZ].size = chunkSize;
                chunkGridGen[chunkX, chunkZ].start = Grid[startX, startZ];
                chunkGridGen[chunkX, chunkZ].end = Grid[endX, endZ];
                chunkCount++;
            }
        }
        Debug.Log(chunkNumX + "x" + chunkNumZ + " chunk grid generated");
        return chunkGridGen;
    }

    void TerrainMeshGen() {
        Debug.Log("Generating terrain mesh");
        foreach (Chunk chunk in ChunkGrid) {
            TerrainChunkMeshGen(chunk);
        }
        Debug.Log("Generated mesh for " + ChunkGrid.GetLength(0) * ChunkGrid.GetLength(1) + " chunks");
    }

    void TerrainChunkMeshGen(Chunk chunk) {
        //Generate meshes
        Mesh meshLod0 = MeshGen.Create(MeshGen.MakeTerrainChunk(Grid, chunk));
        Mesh meshLod1 = MeshGen.Create(MeshGen.MakeTerrainChunkLod(Grid, chunk, lod1Size));
        Mesh meshLod2 = MeshGen.Create(MeshGen.MakeTerrainChunkLod(Grid, chunk, lod2Size));
        Mesh meshLod3 = MeshGen.Create(MeshGen.MakeTerrainChunkLod(Grid, chunk, lod3Size));
        Mesh meshLod4 = MeshGen.Create(MeshGen.MakeTerrainChunkLod(Grid, chunk, lod4Size));
        //Chunk GO
        terrainChunkGoCollection.Add(chunk, new GameObject("CHUNK " + chunk.x.ToString("D4") + "x" + chunk.z.ToString("D4"), typeof(LODGroup)));
        terrainChunkGoCollection[chunk].transform.SetParent(terrainGo.transform);
        terrainChunkGoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
        //LOD0
        terrainLod0GoCollection.Add(chunk, new GameObject("LOD0", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)));
        terrainLod0GoCollection[chunk].transform.SetParent(terrainChunkGoCollection[chunk].transform);
        terrainLod0GoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod0GoCollection[chunk].GetComponent<MeshFilter>().sharedMesh = meshLod0;
        terrainLod0GoCollection[chunk].GetComponent<MeshCollider>().sharedMesh = meshLod0;
        terrainLod0GoCollection[chunk].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mat");
        terrainLod0GoCollection[chunk].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //LOD1
        terrainLod1GoCollection.Add(chunk, new GameObject("LOD1", typeof(MeshFilter), typeof(MeshRenderer)));
        terrainLod1GoCollection[chunk].transform.SetParent(terrainChunkGoCollection[chunk].transform);
        terrainLod1GoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod1GoCollection[chunk].GetComponent<MeshFilter>().sharedMesh = meshLod1;
        terrainLod1GoCollection[chunk].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mat");
        terrainLod1GoCollection[chunk].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //LOD2
        terrainLod2GoCollection.Add(chunk, new GameObject("LOD2", typeof(MeshFilter), typeof(MeshRenderer)));
        terrainLod2GoCollection[chunk].transform.SetParent(terrainChunkGoCollection[chunk].transform);
        terrainLod2GoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod2GoCollection[chunk].GetComponent<MeshFilter>().sharedMesh = meshLod2;
        terrainLod2GoCollection[chunk].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mat");
        terrainLod2GoCollection[chunk].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //LOD3
        terrainLod3GoCollection.Add(chunk, new GameObject("LOD3", typeof(MeshFilter), typeof(MeshRenderer)));
        terrainLod3GoCollection[chunk].transform.SetParent(terrainChunkGoCollection[chunk].transform);
        terrainLod3GoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod3GoCollection[chunk].GetComponent<MeshFilter>().sharedMesh = meshLod3;
        terrainLod3GoCollection[chunk].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mat");
        terrainLod3GoCollection[chunk].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //LOD4
        terrainLod4GoCollection.Add(chunk, new GameObject("LOD4", typeof(MeshFilter), typeof(MeshRenderer)));
        terrainLod4GoCollection[chunk].transform.SetParent(terrainChunkGoCollection[chunk].transform);
        terrainLod4GoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
        terrainLod4GoCollection[chunk].GetComponent<MeshFilter>().sharedMesh = meshLod4;
        terrainLod4GoCollection[chunk].GetComponent<Renderer>().material = Resources.Load<Material>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mat");
        terrainLod4GoCollection[chunk].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        TerrainChunkUpdateLods(chunk);
    }

    void TerrainChunkUpdateLods(Chunk chunk) {
        Renderer[] r0Array = new Renderer[1];
        Renderer[] r1Array = new Renderer[1];
        Renderer[] r2Array = new Renderer[1];
        Renderer[] r3Array = new Renderer[1];
        Renderer[] r4Array = new Renderer[1];
        r0Array[0] = terrainLod0GoCollection[chunk].GetComponent<Renderer>();
        r1Array[0] = terrainLod1GoCollection[chunk].GetComponent<Renderer>();
        r2Array[0] = terrainLod2GoCollection[chunk].GetComponent<Renderer>();
        r3Array[0] = terrainLod3GoCollection[chunk].GetComponent<Renderer>();
        r4Array[0] = terrainLod4GoCollection[chunk].GetComponent<Renderer>();
        LOD[] lods = new LOD[5];
        lods[0] = new LOD(.40f, r0Array);
        lods[1] = new LOD(.20f, r1Array);
        lods[2] = new LOD(.10f, r2Array);
        lods[3] = new LOD(.05f, r3Array);
        lods[4] = new LOD(.0025f, r4Array);
        lods[0].fadeTransitionWidth = 0.25f;
        lods[1].fadeTransitionWidth = 0.25f;
        lods[2].fadeTransitionWidth = 0.25f;
        lods[3].fadeTransitionWidth = 0.25f;
        lods[4].fadeTransitionWidth = 0.25f;
        terrainChunkGoCollection[chunk].GetComponent<LODGroup>().SetLODs(lods);
        terrainChunkGoCollection[chunk].GetComponent<LODGroup>().RecalculateBounds();
        terrainChunkGoCollection[chunk].GetComponent<LODGroup>().fadeMode = LODFadeMode.CrossFade;
    }

    void GridMeshGen(float angle) {
        Debug.Log("Generating grid mesh");
        foreach (Chunk chunk in ChunkGrid) {
            //Chunk GO
            gridChunkGoCollection.Add(chunk, new GameObject("CHUNK " + chunk.x.ToString("D4") + "x" + chunk.z.ToString("D4"), typeof(LODGroup)));
            gridChunkGoCollection[chunk].transform.SetParent(gridGo.transform);
            gridChunkGoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
            //Grid
            gridGoCollection.Add(chunk, new GameObject("Grid", typeof(MeshFilter), typeof(MeshRenderer)));
            gridGoCollection[chunk].transform.SetParent(gridChunkGoCollection[chunk].transform);
            gridGoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
            gridGoCollection[chunk].GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlitTrans");
            gridGoCollection[chunk].GetComponent<Renderer>().material.color = new Color(.8f, .8f, .8f, .3f);
            gridGoCollection[chunk].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            gridGoCollection[chunk].GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainGridChunk(Grid, chunk, angle));
            //Grid steep
            gridSteepGoCollection.Add(chunk, new GameObject("Grid-steep", typeof(MeshFilter), typeof(MeshRenderer)));
            gridSteepGoCollection[chunk].transform.SetParent(gridChunkGoCollection[chunk].transform);
            gridSteepGoCollection[chunk].transform.localPosition = new Vector3(0, 0.15f, 0);
            gridSteepGoCollection[chunk].GetComponent<Renderer>().material = Resources.Load<Material>("Materials/unlitTrans");
            gridSteepGoCollection[chunk].GetComponent<Renderer>().material.color = new Color(0.8867924f, 0.1296724f, 0.1296724f, 0.6f);
            gridSteepGoCollection[chunk].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            gridSteepGoCollection[chunk].GetComponent<MeshFilter>().mesh = MeshGen.Create(MeshGen.MakeTerrainGridChunk(Grid, chunk, angle, true, 1.5f));
            GridChunkUpdateLods(chunk);
        }
        Debug.Log("Generated " + Grid.GetLength(0) * Grid.GetLength(1) + " grid point meshes");
    }
    void GridChunkUpdateLods(Chunk chunk) {
        List<Renderer> r0List = new List<Renderer>();
        if (gridGoCollection.ContainsKey(chunk)) { r0List.Add(gridGoCollection[chunk].GetComponent<Renderer>()); }
        if (gridSteepGoCollection.ContainsKey(chunk)) { r0List.Add(gridSteepGoCollection[chunk].GetComponent<Renderer>()); }
        Renderer[] r0Array = r0List.ToArray();
        r0Array[0] = gridGoCollection[chunk].GetComponent<Renderer>();
        LOD[] lods = new LOD[1];
        lods[0] = new LOD(.6f, r0Array);
        lods[0].fadeTransitionWidth = 0.5f;
        gridChunkGoCollection[chunk].GetComponent<LODGroup>().SetLODs(lods);
        gridChunkGoCollection[chunk].GetComponent<LODGroup>().RecalculateBounds();
        gridChunkGoCollection[chunk].GetComponent<LODGroup>().fadeMode = LODFadeMode.CrossFade;
    }

    void GridGrassAutoGen(GridPoint[,] grid, Grass grass, float heightMin = 0, float heightMax = 999, float angleMin = 0, float angleMax = 10.0f) {
        Debug.Log("Auto generating grass #" + grass.id);
        int count = 0;
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int z = 0; z < grid.GetLength(1); z++) {
                if (grid[x, z].inBounds && (grid[x, z].posCenter.y >= heightMin && grid[x, z].posCenter.y < heightMax) && (grid[x, z].angleCenter >= angleMin && grid[x, z].angleCenter < angleMax)) {
                    GridGrassAdd(Grid[x, z], grass, false);
                    count++;
                }
            }
        }
        Debug.Log("Added " + count + " grass entries");
    }

    public void GridGrassAdd(GridPoint gp, Grass grass, bool doMeshGen = true) {
        Chunk chunk = GpToChunk(gp);
        GridGrassRemove(gp, doMeshGen);
        dataManager.TryAddGrassPlacedList(chunk);
        dataManager.GrassPlaced[chunk].Add(new GrassPlaced() { grass = grass, gp = gp });
        if (doMeshGen) { GridGrassMeshGen(chunk, grass); }
    }

    public void GridGrassRemove(GridPoint gp, bool doMeshGen = true) {
        Chunk chunk = GpToChunk(gp);
        if (dataManager.GrassPlaced.ContainsKey(chunk)) {
            for (int i = dataManager.GrassPlaced[chunk].Count - 1; i >= 0; i--) {
                GrassPlaced entry = dataManager.GrassPlaced[chunk][i];
                if (entry.gp == gp) {
                    dataManager.GrassPlaced[chunk].Remove(entry);
                    if (doMeshGen) { GridGrassMeshGen(chunk, entry.grass); }
                    dataManager.TryRemoveGrassPlacedList(chunk);
                }
            }
        }
    }

    void TryAddGrassGo(Chunk chunk, Grass grass) {
        TryAddGrassChunkGo(chunk);
        if (!grassLod0GoCollection[chunk].ContainsKey(grass)) {
            grassLod0GoCollection[chunk].Add(grass, new GameObject("Grass #" + grass.id.ToString("D4"), typeof(MeshFilter), typeof(MeshRenderer)));
            grassLod0GoCollection[chunk][grass].transform.SetParent(grassChunkGoCollection[chunk].transform);
            grassLod0GoCollection[chunk][grass].transform.localPosition = new Vector3(0, 0, 0);
            grassLod0GoCollection[chunk][grass].GetComponent<Renderer>().material = Resources.Load<Material>("Grass/" + grass.mat);
            grassLod0GoCollection[chunk][grass].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    void TryRemoveGrassGo(Chunk chunk, Grass grass) {
        if (!dataManager.GrassPlaced[chunk].Exists(r => r.grass == grass)) {
            Destroy(grassLod0GoCollection[chunk][grass]);
            grassLod0GoCollection[chunk].Remove(grass);
        }
        TryRemoveGrassChunkGo(chunk);
    }

    void TryAddGrassChunkGo(Chunk chunk) {
        if (!grassLod0GoCollection.ContainsKey(chunk)) {
            grassLod0GoCollection.Add(chunk, new Dictionary<Grass, GameObject>());
            grassChunkGoCollection.Add(chunk, new GameObject("CHUNK " + chunk.x.ToString("D4") + "x" + chunk.z.ToString("D4"), typeof(LODGroup)));
            grassChunkGoCollection[chunk].transform.SetParent(grassGo.transform);
            grassChunkGoCollection[chunk].transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    void TryRemoveGrassChunkGo(Chunk chunk) {
        if (grassLod0GoCollection.ContainsKey(chunk)) {
            if (grassLod0GoCollection[chunk].Count == 0) {
                Destroy(grassChunkGoCollection[chunk]);
                grassChunkGoCollection.Remove(chunk);
                grassLod0GoCollection.Remove(chunk);
            }
        }
    }

    void GridGrassMeshGenAll() {
        foreach (Chunk chunk in ChunkGrid) {
            if (dataManager.GrassPlaced.ContainsKey(chunk)) {
                foreach (GrassPlaced entry in dataManager.GrassPlaced[chunk].GroupBy(r => r.grass.id).Select(r => new List<GrassPlaced>(r)[0]).ToList()) {
                    GridGrassMeshGen(chunk, entry.grass);
                }
            }
        }
    }

    void GridGrassMeshGen(Chunk chunk, Grass grass) {
        Debug.Log("Generating mesh for chunk #" + chunk.x.ToString("D4") + "x" + chunk.z.ToString("D4") + " - grass #" + grass.id.ToString("D4"));
        if (!dataManager.GrassPlaced[chunk].Exists(r => r.grass == grass)) {
            TryRemoveGrassGo(chunk, grass);
        } else {
            TryAddGrassGo(chunk, grass);
            grassLod0GoCollection[chunk][grass].GetComponent<MeshFilter>().mesh = GrassMeshGen.Create(grass, MeshGen.Create(MeshGen.MakeTerrainListGrass(Grid, dataManager.GrassPlaced[chunk].FindAll(r => r.grass == grass))));
            GridGrassChunkUpdateLods(chunk);
            Debug.Log("Generated " + dataManager.GrassPlaced[chunk].FindAll(r => r.grass == grass).Count + " grass meshes");
        }
    }

    void GridGrassChunkUpdateLods(Chunk chunk) {
        List<Renderer> r0List = new List<Renderer>(); foreach (GameObject entry in grassLod0GoCollection[chunk].Values) { r0List.Add(entry.GetComponent<Renderer>()); }
        Renderer[] r0Array = r0List.ToArray();
        LOD[] lods = new LOD[1];
        lods[0] = new LOD(Mathf.Min(.50f, .02f + (dataManager.GrassPlaced[chunk].Count * .0050f)), r0Array);
        lods[0].fadeTransitionWidth = 0.50f;
        grassChunkGoCollection[chunk].GetComponent<LODGroup>().SetLODs(lods);
        grassChunkGoCollection[chunk].GetComponent<LODGroup>().RecalculateBounds();
        grassChunkGoCollection[chunk].GetComponent<LODGroup>().fadeMode = LODFadeMode.CrossFade;
    }

    public void GrassGenTexture() {
        Debug.Log("Generating grass terrain texture");
        Material terrainMat = Resources.Load<Material>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-mat");
        Texture2D terrainTex = terrainMat.GetTexture("_MainTex") as Texture2D;
        int texH = terrainTex.height;
        int texW = terrainTex.width;
        Texture2D grassTex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        //Fill grass texture with transparency
        Color[] fillPixels = new Color[texW * texH];
        for (int i = 0; i < fillPixels.Length; i++) {
            fillPixels[i] = Color.clear;
        }
        grassTex.SetPixels(fillPixels);
        //Set grass texture pixels from grass material
        int count = 0;
        foreach (Chunk chunk in ChunkGrid) {
            if (dataManager.GrassPlaced.ContainsKey(chunk)) {
                foreach (GrassPlaced entry in dataManager.GrassPlaced[chunk]) {
                    Material mat = Resources.Load<Material>("Grass/" + entry.grass.mat);
                    Texture2D noiseA = mat.GetTexture("_NoiseATexture") as Texture2D;
                    Texture2D noiseB = mat.GetTexture("_NoiseBTexture") as Texture2D;
                    float noiseScaleA = mat.GetFloat("_NoiseAScale");
                    float noiseScaleB = mat.GetFloat("_NoiseBScale");
                    float noiseBoostA = mat.GetFloat("_NoiseABoost");
                    float noiseBoostB = mat.GetFloat("_NoiseBBoost");
                    Color colorBase = mat.GetColor("_BaseColor");
                    Color colorTop = mat.GetColor("_TopColor");
                    int xStart = Mathf.FloorToInt(entry.gp.uv[2].x * texW);
                    int yStart = Mathf.FloorToInt(entry.gp.uv[2].y * texH);
                    int xEnd = Mathf.FloorToInt(entry.gp.uv[1].x * texW);
                    int yEnd = Mathf.FloorToInt(entry.gp.uv[1].y * texH);
                    for (int x = xStart; x < xEnd; x++) {
                        for (int y = yStart; y < yEnd; y++) {
                            Color noiseSampleA = noiseA.GetPixel(x, y);
                            Color noiseSampleB = noiseB.GetPixel(x, y);
                            float sampleFloatA = ((noiseSampleA.r + noiseSampleA.g + noiseSampleA.b) / 3) + noiseBoostA;
                            float sampleFloatB = ((noiseSampleB.r + noiseSampleB.g + noiseSampleB.b) / 3) + noiseBoostB;
                            sampleFloatA = 1 - (1 - sampleFloatA) * noiseScaleA;
                            sampleFloatB = 1 - (1 - sampleFloatB) * noiseScaleB;
                            float sampleFloatResult = sampleFloatA * sampleFloatB;
                            Color colorMix = new Color();
                            colorMix = Color.Lerp(colorBase, colorTop, sampleFloatResult);
                            if (sampleFloatResult < 0) { colorMix.a = 0; }
                            grassTex.SetPixel(x, y, colorMix);
                            count++;
                        }
                    }
                }
            }
        }
        //Apply pixels to texture
        grassTex.Apply();
        //grassTex.filterMode = FilterMode.Point;
        //Save grass texture to file
        byte[] bytes = grassTex.EncodeToPNG();
        var dirPath = Application.dataPath + "/Resources/Terrain/" + TerrainId.ToString("D2") + "/";
        if (!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + TerrainId.ToString("D2")+ "-texture-grass" + ".png", bytes);
        //Apply grass texture to terrain mat
        AssetDatabase.Refresh();
        terrainMat.SetTexture("_GrassTex", Resources.Load<Texture>("Terrain/" + TerrainId.ToString("D2") + "/" + TerrainId.ToString("D2") + "-texture-grass"));
        Debug.Log(count + " pixel grass texture generated");
    }
}
