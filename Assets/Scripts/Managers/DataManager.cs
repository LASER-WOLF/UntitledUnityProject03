using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour {
    Dictionary<int, Unit> _units = new Dictionary<int, Unit>();
    Dictionary<int, Grass> _grass = new Dictionary<int, Grass>();

    int _numEntities = 9999;

    List<TerrainInfo> _terrains = new List<TerrainInfo>() {
    new TerrainInfo(){ id=13 }
    };

    //add grassmat list
    Dictionary<Chunk, List<GrassPlaced>> _grassPlaced = new Dictionary<Chunk, List<GrassPlaced>>();
    Dictionary<GridPoint, Unit> _unitsPlaced = new Dictionary<GridPoint, Unit>();

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

    public Dictionary<Chunk, List<GrassPlaced>> GrassPlaced {
        get => _grassPlaced;
        set => _grassPlaced = value;
    }

    public Dictionary<GridPoint, Unit> UnitsPlaced {
        get => _unitsPlaced;
        set => _unitsPlaced = value;
    }

    void Awake() {
        _units = UnitsInit();
        _grass = GrassInit();
    }

    public void TryAddGrassPlacedList(Chunk chunk) {
        if (!GrassPlaced.ContainsKey(chunk)) {
            GrassPlaced.Add(chunk, new List<GrassPlaced>());
        }
    }

    public void TryRemoveGrassPlacedList(Chunk chunk) {
        if (GrassPlaced.ContainsKey(chunk)) {
            if (GrassPlaced[chunk].Count == 0) {
                GrassPlaced.Remove(chunk);
            }
        }
    }

    Dictionary<int, Grass> GrassInit() {
        Dictionary<int, Grass> grassEmpty = new Dictionary<int, Grass>();
        for (int x = 1; x <= _numEntities; x++) {
            grassEmpty.Add(x, new Grass() { id = x });
        }
        grassEmpty[1].mat = "grass0001";
        grassEmpty[2].mat = "grass0002";
        grassEmpty[3].mat = "grass0003";
        grassEmpty[4].mat = "grass0004";
        grassEmpty[5].mat = "grass0005";
        grassEmpty[6].mat = "moss0001";
        grassEmpty[7].mat = "moss0002";
        grassEmpty[8].mat = "moss0003";
        grassEmpty[9].mat = "moss0004";
        grassEmpty[10].mat = "moss0005";
        grassEmpty[11].mat = "lava0001";
        grassEmpty[12].mat = "lava0002";
        grassEmpty[13].mat = "lava0003";
        grassEmpty[14].mat = "lava0004";
        grassEmpty[15].mat = "lava0005";
        grassEmpty[16].mat = "snow0001";
        grassEmpty[17].mat = "snow0002";
        grassEmpty[18].mat = "snow0003";
        grassEmpty[19].mat = "snow0004";
        grassEmpty[20].mat = "snow0005";
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
