using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ListManager : MonoBehaviour
{
    Dictionary<int, Unit> _units = new Dictionary<int, Unit>();
    Dictionary<int, Grass> _grass = new Dictionary<int, Grass>();
    
    int _numEntities=9999;

    List<TerrainInfo> _terrains = new List<TerrainInfo>() { 
    new TerrainInfo(){ id=13 }
    };

    //add grassmat list

    //change: store terrain id, gp, id


    //chunkid
    Dictionary<int, List<GrassPlaced>> _grassPlaced = new Dictionary<int, List<GrassPlaced>>();

    Dictionary<Vector2Int, int> _unitsPlaced = new Dictionary<Vector2Int, int>();

    public Dictionary<int, Grass> Grass {
        get => _grass;
        set => _grass = value;
    }

    public Dictionary<int, Unit> Units {
        get => _units;
        set => _units = value;
    }

    public List<TerrainInfo> Terrains {
        get => _terrains;
        set => _terrains = value;
    }

    public Dictionary<int, List<GrassPlaced>> GrassPlaced {
        get => _grassPlaced;
        set => _grassPlaced = value;
    }

    public Dictionary<Vector2Int, int> UnitsPlaced {
        get => _unitsPlaced;
        set => _unitsPlaced = value;
    }

    void Awake()
    {
        _units = UnitsInit();
        _grass = GrassInit();
    }

    public void TryAddGrassPlacedList(int chunkId) {
        if (!GrassPlaced.ContainsKey(chunkId)) {
            GrassPlaced.Add(chunkId, new List<GrassPlaced>());
        }
    }

    public void TryRemoveGrassPlacedList(int chunkId) {
        if (GrassPlaced.ContainsKey(chunkId)) {
            if (GrassPlaced[chunkId].Count == 0) {
                GrassPlaced.Remove(chunkId);
            }
        }
    }

    Dictionary<int, Grass> GrassInit() {
        Dictionary<int, Grass> grassEmpty = new Dictionary<int, Grass>();
        for (int x = 1; x <= _numEntities; x++) {
            
            //temp
            //grassEmpty.Add(x, new Grass() { id = x, mat = "grassDefault", settings = grassSettings });
            string mat = "grass" + x.ToString("D4");

            grassEmpty.Add(x, new Grass() { id = x });
        }
        grassEmpty[1].mat = "grass0001";
        grassEmpty[2].mat = "grass0002";
        grassEmpty[3].mat = "grass0003";
        grassEmpty[4].mat = "grass0004";
        grassEmpty[5].mat = "grass0005";
        
        grassEmpty[6].mat = "moss0001";
        //grassEmpty[6].height = 0.25f;
        //grassEmpty[6].numLayers = 5;
        grassEmpty[7].mat = "moss0002";
        //grassEmpty[7].height = 0.25f;
        //grassEmpty[7].numLayers = 5;
        grassEmpty[8].mat = "moss0003";
        //grassEmpty[8].height = 0.25f;
        //grassEmpty[8].numLayers = 5;
        grassEmpty[9].mat = "moss0004";
        //grassEmpty[9].height = 0.25f;
        //grassEmpty[9].numLayers = 5;
        grassEmpty[10].mat = "moss0005";
        //grassEmpty[10].height = 0.25f;
        //grassEmpty[10].numLayers = 5;

        grassEmpty[11].mat = "lava0001";
        //grassEmpty[11].height = 0.25f;
        //grassEmpty[11].numLayers = 5;
        grassEmpty[12].mat = "lava0002";
        //grassEmpty[12].height = 0.25f;
        //grassEmpty[12].numLayers = 5;
        grassEmpty[13].mat = "lava0003";
        //grassEmpty[13].height = 0.25f;
        //grassEmpty[13].numLayers = 5;
        grassEmpty[14].mat = "lava0004";
        //grassEmpty[14].height = 0.25f;
        //grassEmpty[14].numLayers = 5;
        grassEmpty[15].mat = "lava0005";
        //grassEmpty[15].height = 0.25f;
        //grassEmpty[15].numLayers = 5;


        grassEmpty[16].mat = "snow0001";
        //grassEmpty[16].height = 0.25f;
        //grassEmpty[16].numLayers = 5;

        grassEmpty[17].mat = "snow0002";
        //grassEmpty[17].height = 0.25f;
        //grassEmpty[17].numLayers = 5;

        grassEmpty[18].mat = "snow0003";
        //grassEmpty[18].height = 0.25f;
        //grassEmpty[18].numLayers = 5;

        grassEmpty[19].mat = "snow0004";
        //grassEmpty[19].height = 0.25f;
        //grassEmpty[19].numLayers = 5;

        grassEmpty[20].mat = "snow0005";
        //grassEmpty[20].height = 0.25f;
        //grassEmpty[20].numLayers = 5;
        return grassEmpty;
    }

    Dictionary<int, Unit> UnitsInit() {
        Dictionary<int, Unit> unitsEmpty = new Dictionary<int, Unit>();
        for (int x = 1; x <= _numEntities; x++) {
            unitsEmpty.Add(x, new Unit() { id = x, name = "unit #" + x.ToString("D4") });
        }
        return unitsEmpty;
    }
}
