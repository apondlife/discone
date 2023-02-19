using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Discone {

/// the persistence container
public sealed class Store: ScriptableObject {
    // -- published --
    [Header("events")]
    [Tooltip("when the load finishes")]
    [SerializeField] VoidEvent m_LoadFinished;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entities;

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
    /// sync the in-memory world record
    public void SyncWorld() {
        // grab player flower
        var pf = FindPlayerCharacter()?.Flower;

        // don't add flowers in the same position
        var memo = new HashSet<Vector3>();

        // starting with the player's flower
        if (pf != null) {
            memo.Add(pf.IntoRecord().P);
        }

        // generate records for each unique flower
        var records = m_Entities.Value.Flowers
            .All
            .Where((f) => f != pf)
            .Select((f) => f.IntoRecord())
            .Where((r) => memo.Add(r.P)) // don't add duplicate flowers
            .ToArray();

        // update world record
        m_World.Flowers = records;
    }

    /// try syncing the in-memory player record, fails if a player has no character
    public void SyncPlayer() {
        // find the player's current character
        var character = FindPlayerCharacter();
        if (character == null) {
            Debug.LogError("[store] found no player character to sync!");
            return;
        }

        // and update the record
        m_Player.Character = character.IntoRecord();
        Debug.Log($"[store] updated player record {m_Player}");
    }

    /// save the current state to file
    [ContextMenu("Save Store")]
    public async Task Save() {
        // sync state
        SyncWorld();
        SyncPlayer();

        // ensure we have a directory to write to
        Directory.CreateDirectory(RootPath);

        // write the records to disk
        await Task.WhenAll(
            SaveRecord<WorldRec>(WorldPath, m_World),
            SaveRecord<PlayerRec>(PlayerPath, m_Player)
        );
    }

    /// delete the file at the ath
    public void Delete(string path) {
        File.Delete(ResolvePath(path));
    }

    /// copy the file path to clipboard
    public void CopyPath(string path) {
        GUIUtility.systemCopyBuffer = ResolvePath(path);
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

    /// resolve a relative path to an absolute one
    string ResolvePath(string path) {
       return Path.Combine(RootPath, path);
    }

    /// find a reference to the current player
    /// TOOD: caching???
    DisconeCharacter FindPlayerCharacter() {
        return GameObject
            .FindObjectOfType<DisconePlayer>()
            .Character;
    }

    // -- io --
    /// the data directory path
    string DataPath {
        #if UNITY_EDITOR
        get => Path.Combine(Application.dataPath, "..", "Artifacts", "data");
        #else
        get => Application.persistentDataPath;
        #endif
    }

    /// the root store path
    string RootPath {
        get => Path.Combine(
            DataPath,
            SceneManager.GetActiveScene().name
        );
    }

    /// the path to the world file
    string WorldPath {
        get => ResolvePath("world.json");
    }

    /// the path to the player file
    string PlayerPath {
        get => ResolvePath("player.json");
    }

    /// write the record to disk at path
    async Task SaveRecord<F>(string path, F record) where F : StoreFile {
        // don't save empty files
        if (!record.HasData) {
            return;
        }

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

        Debug.Log($"[store] saved file @ {RenderPath(path)} => {json}");
    }

    /// load the record from disk at path
    async Task<F> LoadRecord<F>(string path) where F: StoreFile {
        // check for file
        if (!File.Exists(path)) {
            Debug.Log($"[store] no file found @ {RenderPath(path)}");
            return default;
        }

        // read data from file
        byte[] data;
        using (var stream = new FileStream(path, FileMode.Open)) {
            data = new byte[stream.Length];
            var read = await stream.ReadAsync(data, 0, (int)stream.Length);

            if (read != stream.Length) {
                Debug.LogError($"[store] only read {read} of {stream.Length} bytes from file @ {RenderPath(path)}");
                throw new System.Exception("couldn't read the entire file!");
            }
        }

        // decode record from json
        var json = Encoding.UTF8.GetString(data);
        var record = JsonUtility.FromJson<F>(json);
        Debug.Log($"[store] loaded file @ {RenderPath(path)} => {json}");

        // check the file version
        var version = record.CurrentVersion();
        if (record.Version != version) {
            Debug.LogWarning($"[store] read file w/ obsolete version: {record.Version} < {version}");
            return default;
        }

        return record;
    }

    // -- helpers --
    /// debug; remove the project dir from the path (for display)
    string RenderPath(string path) {
        path = Path.GetFullPath(path);

        // strip project dir from path if necessary
        var dir = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        if (path.StartsWith(dir)) {
            path = path.Substring(dir.Length);
        }

        return path;
    }
}
}