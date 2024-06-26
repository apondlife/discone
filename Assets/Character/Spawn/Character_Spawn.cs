using System;
using Mirror;
using UnityEngine;

namespace Discone {

/// spawns the character at a particular position on start
sealed class Character_Spawn: NetworkBehaviour {
    /// -- statics --
    /// the amount to hue shift the next character by
    static float s_NextShift = 0f;

    // -- fields --
    [Header("fields")]
    [Tooltip("the character to spawn")]
    [SerializeField] CharacterKey m_Character;

    // -- lifecycle --
    public override void OnStartServer() {
        base.OnStartServer();

        var t = transform;
        var record = new CharacterRec(
            key: m_Character,
            pos: t.position,
            rot: t.rotation,
            null
        );

        Server_Spawn(
            record,
            srcNetId: "server",
            srcName: name
        );

        Destroy(this);
    }

    // -- factories --
    /// create a character for the record
    [Server]
    public static Character Server_Create(
        CharacterRec record,
        string srcNetId,
        string srcName = null
    ) {
        var prefab = CharacterDefs.Instance.Find(record.Key).Character;

        // TODO: character spawns exactly in the ground, and because of chunk
        // delay it ends up falling through the ground
        const float offset = 1f;
        var newCharacter = Instantiate(
            prefab,
            record.Pos + offset * Vector3.up,
            record.Rot
        );

        // shift the hue of the characters in fibonnacci
        newCharacter.Online.ShiftHue(s_NextShift);
        s_NextShift += Soil.Mathx.PHI;

        #if UNITY_EDITOR
        var name = $"{record.Key.Name()} <spawned@{srcNetId}>";
        if (srcName != null) {
            name += $" [{srcName}]";
        }
        newCharacter.name = name;
        #endif

        return newCharacter;
    }

    /// spawn a character for the record
    [Server]
    public static Character Server_Spawn(
        CharacterRec record,
        string srcNetId,
        string srcName
    ) {
        var character = Server_Create(record, srcNetId, srcName);
        Server_Spawn(character);
        Server_FinishSpawn(character, record);
        return character;
    }

    /// spawn a character on the server
    [Server]
    public static void Server_Spawn(
        Character character
    ) {
        NetworkServer.Spawn(character.gameObject);
    }

    /// finish spawning a character for the record
    [Server]
    public static void Server_FinishSpawn(
        Character character,
        CharacterRec record
    ) {
        // TODO: should this be an event?
        if (record.Flower != null) {
            character.Checkpoint.Server_CreateCheckpoint(record.Flower);
        }
    }

    // -- debug --
    #if UNITY_EDITOR
    /// the placeholder model
    GameObject m_Placeholder;

    /// the renderers w/ attached mesh
    (Renderer, Mesh)[] m_PlaceholderRenderers = Array.Empty<(Renderer, Mesh)>();

    void OnDrawGizmos() {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        var character = CharacterDefs
            .Instance
            .Find(m_Character);

        var placeholder = character.Placeholder;
        if (placeholder != m_Placeholder) {
            var renderers = placeholder.GetComponentsInChildren<Renderer>();

            Array.Clear(m_PlaceholderRenderers, 0, m_PlaceholderRenderers.Length);
            if (renderers.Length > m_PlaceholderRenderers.Length) {
                Array.Resize(ref m_PlaceholderRenderers, renderers.Length);
            }

            var i = 0;
            foreach (var renderer in renderers) {
                var mesh = null as Mesh;
                if (renderer is SkinnedMeshRenderer s) {
                    mesh = s.sharedMesh;
                } else {
                    var meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter) {
                        mesh = meshFilter.sharedMesh;
                    }
                }

                m_PlaceholderRenderers[i] = (renderer, mesh);
                i += 1;
            }

            m_Placeholder = placeholder;
        }

        var matrix = transform.localToWorldMatrix * Matrix4x4.Translate(character.Placeholder_Offset);

        foreach (var (renderer, mesh) in m_PlaceholderRenderers) {
            if (mesh == null) {
                continue;
            }

            var isMaterialSet = renderer.sharedMaterial.SetPass(0);
            if (isMaterialSet) {
                Graphics.DrawMeshNow(
                    mesh,
                    matrix * renderer.transform.localToWorldMatrix
                );
            }
        }
    }
    #endif
}

}