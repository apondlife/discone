using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextColor: MonoBehaviour
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


    public void ColorText(TMP_Text text, Color32 glowColor, int startPos, int length)
    {
        m_TextComponent = text;
        m_Color = glowColor;

        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        Debug.Log(text);
        Debug.Log(textInfo);

        int loopCount = 0;

        //TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        // yield return new WaitForSeconds(0.1f);
        // while (true)
        // {

            for (int i = startPos; i < startPos + length; i++)
            {
                Color32[] newVertexColors;
                Color32 c0 = m_Color;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Debug.Log(i);
                // Debug.Log(textInfo.characterInfo[i].character);

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // overwrite all color channels except alpha
                c0 = newVertexColors[vertexIndex];
                c0.r = m_Color.r;
                c0.g = m_Color.g;
                c0.b = m_Color.b;
                Debug.Log(textInfo.characterInfo[i].character);
                Debug.Log(c0);

                newVertexColors[vertexIndex + 0] = c0;
                newVertexColors[vertexIndex + 1] = c0;
                newVertexColors[vertexIndex + 2] = c0;
                newVertexColors[vertexIndex + 3] = c0;

                // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

                Debug.Log(textInfo.meshInfo[materialIndex].colors32[vertexIndex]);
            }

            // // Push changes into meshes
            // for (int i = 0; i < textInfo.meshInfo.Length; i++)
            // {
            //     textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            //     m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            // }

            // yield return new WaitForSeconds(0.1f);
        // }

    }
}
