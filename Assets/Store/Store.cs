using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// the persistence container
public sealed class Store: ScriptableObject {
    // -- published --
    [Header("events")]
    [Tooltip("when the load finishes")]
    [SerializeField] VoidEvent m_LoadFinished;

    // -- props --
    /// the current world on disk
    WorldRec m_World;

    /// the current player on disk
    PlayerRec m_Player;

    // -- commands --
    /// load data from disk
    public async void Load() {
        // ensure we have a directory to read from
        Directory.CreateDirectory(RootPath);

        // load the records
        var w = LoadRecord<WorldRec>(WorldPath);
        var p = LoadRecord<PlayerRec>(PlayerPath);
        await Task.WhenAll(w, p);

        // store the records
        m_World = w.Result;
        m_Player = p.Result;

        // dispatch completion
        m_LoadFinished.Raise();
    }

    /// save the current state
    [ContextMenu("Save Store")]
    public async void Save() {
        // grab player character and flower
        var pc = GameObject
            .FindObjectOfType<DisconePlayer>()
            .Character;

        var pf = pc.Checkpoint.Flower;

        // update player rec
        m_Player.Character = CharacterRec.From(pc);

        // update flowers recs
        m_World.Flowers = GameObject
            .FindObjectsOfType<CharacterFlower>()
            // don't save the player flower, it gets loaded by player code
            .Where(flower => flower != pf)
            .Select(FlowerRec.From)
            .ToArray();

        // ensure we have a directory to write to
        Directory.CreateDirectory(RootPath);

        // write the records to disk
        await Task.WhenAll(
            SaveRecord<PlayerRec>(PlayerPath, m_Player),
            SaveRecord<WorldRec>(WorldPath, m_World)
        );
    }

    /// reset all state
    [ContextMenu("Reset Store")]
    void Reset() {
        File.Delete(WorldPath);
        File.Delete(PlayerPath);
    }

    // -- queries --
    /// the world record
    public WorldRec World {
        get => m_World;
    }

    /// the player record
    public PlayerRec Player {
        get => m_Player;
    }

    /// when the load finishes
    public VoidEvent LoadFinished {
        get => m_LoadFinished;
    }

    /// the root store path
    string RootPath {
        #if UNITY_EDITOR
        get => Path.Combine(Application.dataPath, "..", "Artifacts", "data");
        #else
        get => Application.persistentDataPath;
        #endif
    }

    /// the path to the world file
    string WorldPath {
        get => Path.Combine(RootPath, "world.json");
    }

    /// the path to the player file
    string PlayerPath {
        get => Path.Combine(RootPath, "player.json");
    }

    /// write the record to disk at path
    async Task SaveRecord<T>(string path, T record) {
        // encode the json
        #if UNITY_EDITOR
        var json = JsonUtility.ToJson(record, true);
        #else
        var json = JsonUtility.ToJson(record);
        #endif

        // write the data to disk, truncating whatever is there
        byte[] data;
        using (var stream = new FileStream(path, FileMode.Create)) {
            data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);
        }

        Debug.Log($"[store] saved file @ {path} => {json}");
    }

    /// load the record from disk at path
    async Task<T> LoadRecord<T>(string path) {
        // check for file
        if (!File.Exists(path)) {
            Debug.Log($"[store] no save file found @ {path}");
            return default;
        }

        // read data from file
        byte[] data;
        using (var stream = new FileStream(path, FileMode.Open)) {
            data = new byte[stream.Length];
            var read = await stream.ReadAsync(data, 0, (int)stream.Length);

            if (read != stream.Length) {
                Debug.LogError($"[store] only read ${read} of ${stream.Length} bytes from file @ {path}");
                throw new System.Exception("couldn't read the entire file!");
            }
        }

        // decode record from json
        var json = Encoding.UTF8.GetString(data);
        var record = JsonUtility.FromJson<T>(json);

        Debug.Log($"[store] loaded file @ {path} => {json}");

        return record;
    }
}