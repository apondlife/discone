using System;
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

    // -- config --
    [Header("config")]
    [Tooltip("if the store syncs the player file")]
    [SerializeField] BoolReference m_IsSyncingPlayer;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entities;

    // -- props --
    /// the current world on disk
    WorldRec m_World;

    /// the current player on disk
    PlayerRec m_Player;

    /// if the store has finished loading data
    bool m_IsLoadFinished;

    // -- commands --
    /// load data from disk
    public async void Load() {
        Log.Store.I($"start load");

        // ensure we have a directory to read from
        Directory.CreateDirectory(RootPath);

        // try and load the records
        var w = LoadRecord<WorldRec>(WorldPath);
        var p = LoadRecord<PlayerRec>(PlayerPath);

        try {
            await Task.WhenAll(w, p);
        } catch (Exception e) {
            Log.Store.E($"unhandled error during load: {e}");
        }

        // store the records
        m_World = ResultFrom(w) ?? new WorldRec();
        m_Player = ResultFrom(p) ?? new PlayerRec();

        // dispatch completion
        m_IsLoadFinished = true;
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
        // don't sync if disabled
        if (!m_IsSyncingPlayer) {
            return;
        }

        // find the player's current character
        var character = FindPlayerCharacter();
        if (character == null) {
            Log.Store.E($"found no player character to sync!");
            return;
        }

        // and update the record
        m_Player.Character = character.IntoRecord();
        Log.Store.I($"updated player record {m_Player}");
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
            SaveRecord(WorldPath, m_World),
            SaveRecord(PlayerPath, m_Player)
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
    public void Reset() {
        ResetPlayer();
        ResetWorld();
    }

    /// reset player state
    public void ResetPlayer() {
        Log.Store.I($"resetting player path @ {PlayerPath}");
        File.Delete(PlayerPath);
    }

    /// reset player state
    public void ResetWorld() {
        Log.Store.I($"resetting world path @ {WorldPath}");
        File.Delete(WorldPath);
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

    /// if the load has finished
    public bool IsLoadFinished {
        get => m_IsLoadFinished;
    }

    /// resolve a relative path to an absolute one
    string ResolvePath(string path) {
       return Path.Combine(RootPath, path);
    }

    /// get a nullable result from the task
    T ResultFrom<T>(Task<T> task) {
        return task.Status == TaskStatus.RanToCompletion ? task.Result : default;
    }

    /// find a reference to the current player
    /// TOOD: caching???
    Character FindPlayerCharacter() {
        return GameObject
            .FindObjectOfType<Player>()
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
        using (
            var stream = new FileStream(path, FileMode.Create)
        ) {
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);
        }

        Log.Store.I($"saved file @ {RenderPath(path)} => {json}");
    }

    /// load the record from disk at path
    async Task<F> LoadRecord<F>(string path) where F: StoreFile {
        // check for file
        if (!File.Exists(path)) {
            Log.Store.I($"no file found @ {RenderPath(path)}");
            return default;
        }

        // read data from file
        byte[] data;
        using (
            var stream = new FileStream(path, FileMode.Open)
        ) {
            data = new byte[stream.Length];
            var read = await stream.ReadAsync(data, 0, (int)stream.Length);

            if (read != stream.Length) {
                Log.Store.E($"only read {read} of {stream.Length} bytes from file @ {RenderPath(path)}");
                throw new System.Exception("couldn't read the entire file!");
            }
        }

        // decode record from json
        var json = Encoding.UTF8.GetString(data);
        var record = JsonUtility.FromJson<F>(json);
        Log.Store.I($"loaded file @ {RenderPath(path)} => {json}");

        // check the file version
        var version = record.CurrentVersion();
        if (record.Version != version) {
            Log.Store.W($"read file w/ obsolete version: {record.Version} < {version}");
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