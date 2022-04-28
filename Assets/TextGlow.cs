using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextGlow : MonoBehaviour
{

    private TMP_Text m_TextComponent;
    private bool hasTextChanged;

    private Color32 m_Color;
    
    // Start is called before the first frame update
    void Start()
    {
        // get the right text component
        //m_TextComponent = GetComponent<TMP_Text>();

      //  Coroutine coroutine = StartCoroutine(ShakeText());
        
    }

    public void StartGlowText(TMP_Text text, Color32 glowColor, int startPos, int length) {
        m_TextComponent = text;
        m_Color = glowColor;
        Coroutine coroutine = StartCoroutine(GlowText(startPos, length));

        Debug.Log(startPos);
        Debug.Log(length);

    }

    IEnumerator GlowText(int startPos, int length)
    {
        m_TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        int loopCount = 0;

        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {

            for (int i = startPos; i < startPos + length; i++)
            {
                Color32[] newVertexColors;
                Color32 c0 = m_Color;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                Debug.Log(i);
                Debug.Log(textInfo.characterInfo[i].character);

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                newVertexColors[vertexIndex + 0] = c0;
                newVertexColors[vertexIndex + 1] = c0;
                newVertexColors[vertexIndex + 2] = c0;
                newVertexColors[vertexIndex + 3] = c0;

                // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            }

            // // Push changes into meshes
            // for (int i = 0; i < textInfo.meshInfo.Length; i++)
            // {
            //     textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            //     m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            // }

            yield return new WaitForSeconds(0.1f);
        }

    }
}
