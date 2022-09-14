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
        m_World = w.Result ?? new WorldRec();
        m_Player = p.Result ?? new PlayerRec();

        // dispatch completion
        m_LoadFinished.Raise();
    }

    // -- c/syncing
    /// sync the in-memory records for all objects
    void SyncAll() {
        SyncWorld();
        SyncPlayer();
    }

    /// sync the in-memory world record
    public void SyncWorld() {
        // grab player flower
        var pf = FindPlayerCharacter()?.Flower;

        // update flowers recs
        // TODO: flower repo
        m_World.Flowers = GameObject
            .FindObjectsOfType<CharacterFlower>()
            .Where(flower => flower != pf) // don't save player flower as part of world
            .Select((f) => f.IntoRecord())
            .ToArray();
    }

    /// sync the in-memory player record
    public void SyncPlayer() {
        var character = FindPlayerCharacter();
        if (character == null) {
            Debug.LogError("[store] found no player character to sync!");
            return;
        }

        // update player rec
        m_Player.Character = character.IntoRecord();
        Debug.Log($"[store] updated player record {m_Player}");
    }

    /// save the current state to file
    [ContextMenu("Save Store")]
    public async Task Save() {
        // sync all in-memory records
        SyncAll();

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

    /// the player's character record
    public CharacterRec PlayerCharacter {
        get => m_Player?.Character;
    }

    /// when the load finishes
    public VoidEvent LoadFinished {
        get => m_LoadFinished;
    }

    /// find a reference to the current player
    /// TOOD: caching???
    DisconeCharacter FindPlayerCharacter() {
        return GameObject
            .FindObjectOfType<DisconePlayer>()
            .Character;
    }

    // -- io --
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