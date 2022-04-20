// using UnityEngine;
// using System.Collections;
// using TMPro;

// public class VertexAttributeModifier : MonoBehaviour {


//     public enum AnimationMode { VertexColor, UiVertexColor, Wave, Jitter, Warp, WarpUI, Dangling, Reveal };
//     public AnimationMode MeshAnimationMode = AnimationMode.Wave;
//     public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
//     public float AngleMultiplier = 1.0f;
//     public float SpeedMultiplier = 1.0f;
//     public float CurveScale = 1.0f;

//     private TextMeshPro m_TextMeshPro;
//     private TextMeshProUGUI m_TextMeshProUGUI;
//     private TextContainer m_TextContainer;

//     private TMP_TextInfo m_textInfo;

//     private string textLabel = "Text <#ff8000>silliness</color> with TextMesh<#00aaff>Pro!</color>";
   

   
//     private struct VertexAnim
//     {
//         public float angleRange;
//         public float angle;
//         public float speed;
//     }

//     void Awake()
//     {       
//         // Get a reference to the TextMeshPro Component if one exists. If not add one.
//         m_TextMeshPro = GetComponent<TextMeshPro>() ?? gameObject.AddComponent<TextMeshPro>();
//         m_TextMeshProUGUI = GetComponent<TextMeshProUGUI>(); // ?? gameObject.AddComponent<TextMeshProUGUI>();
              
//         m_TextMeshPro.alignment = TextAlignmentOptions.Center;
//         //m_TextMeshPro.enableWordWrapping = true;
//         //m_TextMeshPro.colorGradient = new VertexGradient(Color.white, Color.white, Color.blue, Color.cyan);
//         //m_TextMeshPro.enableVertexGradient = true;

//         m_TextContainer = GetComponent<TextContainer>();
//         //m_TextContainer.width = 40f;

            
//     }


//     void Start()
//     {
//         m_TextMeshPro.ForceMeshUpdate(); // We force the mesh update in order to get the mesh created so we can have valid data to play with :)   
        
//         switch (MeshAnimationMode)
//         {
//             case AnimationMode.VertexColor:
//                 StartCoroutine(AnimateVertexColors());
//                 break;
//             case AnimationMode.UiVertexColor:
//                 StartCoroutine(AnimateUIVertexColors());
//                 break;
//             case AnimationMode.Wave:
//                 StartCoroutine(AnimateVertexPositions());
//                 break;
//             case AnimationMode.Jitter:
//                 StartCoroutine(AnimateVertexPositionsII());
//                 break;
//             case AnimationMode.Warp:
//                 StartCoroutine(AnimateVertexPositionsIII());
//                 break;
//             case AnimationMode.WarpUI:
//                 StartCoroutine(AnimateUIVertexPositionsIII());
//                 break;
//             case AnimationMode.Dangling:
//                 StartCoroutine(AnimateVertexPositionsIV());
//                 break;
//             case AnimationMode.Reveal:
//                 StartCoroutine(AnimateVertexPositionsVI());
//                 break;
//             //case AnimationMode.Test:
//             //    StartCoroutine(AnimateVertexPositionsV());
//             //    break;

//         }              
//     }


//     IEnumerator AnimateVertexColors()
//     {

//         TMP_TextInfo textInfo = m_TextMeshPro.textInfo;       
//         int currentCharacter = 0;     

//         Color32[] newVertexColors = textInfo.meshInfo.vertexColors;
//         Color32 c0 = m_TextMeshPro.color;
//         c0.a = 127;
//         Color32 c1 = c0;

//         m_TextMeshPro.renderMode = TextRenderFlags.DontRender;

//         while (true)
//         {
//             int characterCount = textInfo.characterCount;
            
//             // If No Characters then just yield and wait for some text to be added
//             if (characterCount == 0)
//             {
//                 yield return new WaiForSectonds(0.25f);
//                 continue;
//             }

//             newVertexColors = textInfo.meshInfo.vertexColors;

//             currentCharacter = (currentCharacter + 1) % characterCount;
//             int vertexIndex = textInfo.characterInfo[currentCharacter].vertexIndex;

//             if (!textInfo.characterInfo[currentCharacter].isVisible)
//                 continue;

//             if (currentCharacter == 0)
//             {
//                 c0 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 127);
                      
//             }

//             c1 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 127);                         
            
//             newVertexColors[vertexIndex + 0] = c1;
//             newVertexColors[vertexIndex + 1] = c1;           
//             newVertexColors[vertexIndex + 2] = c1;
//             newVertexColors[vertexIndex + 3] = c1;
            
//             m_TextMeshPro.mesh.vertices = textInfo.meshInfo.vertices;
//             m_TextMeshPro.mesh.uv = textInfo.meshInfo.uv0s;
//             m_TextMeshPro.mesh.uv2 = textInfo.meshInfo.uv2s;
//             m_TextMeshPro.mesh.colors32 = newVertexColors;

//             yield return new WaitForSeconds(0.05f);
//         }
//     }



//     IEnumerator AnimateUIVertexColors()
//     {

//         TMP_TextInfo textInfo = m_TextMeshProUGUI.textInfo;
//         CanvasRenderer uiRenderer = m_TextMeshProUGUI.canvasRenderer;

//         int currentCharacter = 0;

//         UIVertex[] uiVertices; // = textInfo.meshInfo.uiVertices;

//         Color32 c0 = m_TextMeshProUGUI.color;
//         c0.a = 127; // Since we are modifying the vertex color directly, we need to be mindful that bold information is encoded in the alpha. 0 - 127 is normal weight and 128 - 255 is bold.
//         Color32 c1 = c0;

//         m_TextMeshPro.renderMode = TextRenderFlags.DontRender;

//         while (true)
//         {
//             uiVertices = textInfo.meshInfo.uiVertices;
            
//             int characterCount = textInfo.characterCount;

//             // If No Characters then just yield and wait for some text to be added
//             if (characterCount == 0)
//             {
//                 yield return new WaitForSeconds(0.25f);
//                 continue;
//             }

//             if (textInfo.characterInfo[currentCharacter].isVisible)
//             {

//                 int vertexIndex = textInfo.characterInfo[currentCharacter].vertexIndex;

//                 // Pick new bottom color once per cycle
//                 if (currentCharacter == 0)
//                 {
//                     c0 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 127);
//                 }

//                 c1 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 127);

//                 uiVertices[vertexIndex + 0].color = c0;
//                 uiVertices[vertexIndex + 1].color = c1;
//                 uiVertices[vertexIndex + 2].color = c1;
//                 uiVertices[vertexIndex + 3].color = c0;

//                 uiRenderer.SetVertices(uiVertices, uiVertices.Length);
//             }

//             currentCharacter = (currentCharacter + 1) % characterCount;
           
//             yield return new WaitForSeconds(0.05f);
//         }
//     }



//     IEnumerator AnimateVertexPositions()
//     {
//         VertexCurve.preWrapMode = WrapMode.Loop;
//         VertexCurve.postWrapMode = WrapMode.Loop;
       
//         Vector3[] newVertexPositions;
//         //Matrix4x4 matrix;
            
//         int loopCount = 0;

//         while (true)
//         {
//             m_TextMeshPro.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
//             m_TextMeshPro.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.
            
//             TMP_TextInfo textInfo = m_TextMeshPro.textInfo;
//             int characterCount = textInfo.characterCount;
           
            
//             newVertexPositions = textInfo.meshInfo.vertices;
                               
//             for (int i = 0; i < characterCount; i++)
//             {
//                 if (!textInfo.characterInfo[i].isVisible)
//                     continue;
                                 
//                 int vertexIndex = textInfo.characterInfo[i].vertexIndex;

//                 float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f) * CurveScale; // Random.Range(-0.25f, 0.25f);                    

//                 newVertexPositions[vertexIndex + 0].y += offsetY;
//                 newVertexPositions[vertexIndex + 1].y += offsetY;
//                 newVertexPositions[vertexIndex + 2].y += offsetY;
//                 newVertexPositions[vertexIndex + 3].y += offsetY;
                   
//             }

//             loopCount += 1;

//             // Upload the mesh with the revised information
//             m_TextMeshPro.mesh.vertices = newVertexPositions;           
//             m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo.uv0s;
//             m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo.uv2s;
//             m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo.vertexColors;
                
//             yield return new WaitForSeconds(0.025f);
//         }
                            
//     }


//     IEnumerator AnimateVertexPositionsII()
//     {
       
//         Matrix4x4 matrix;
//         Vector3[] vertices; 
         
//         int loopCount = 0;
          
//         // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
//         VertexAnim[] vertexAnim = new VertexAnim[1024];
//         for (int i = 0; i < 1024; i++ )
//         {
//             vertexAnim[i].angleRange = Random.Range(10f, 25f);
//             vertexAnim[i].speed = Random.Range(1f, 3f);
//         }

//         m_TextMeshPro.renderMode = TextRenderFlags.DontRender;

//         while (loopCount < 10000)
//         {
//             m_TextMeshPro.ForceMeshUpdate();
//             vertices = m_TextMeshPro.textInfo.meshInfo.vertices;           

//             int characterCount = m_TextMeshPro.textInfo.characterCount;

//             for (int i = 0; i < characterCount; i++)
//             {
//                 // Setup initial random values
//                 VertexAnim vertAnim = vertexAnim[i];
//                 TMP_CharacterInfo charInfo = m_TextMeshPro.textInfo.characterInfo[i];
                
//                 // Skip Characters that are not visible
//                 if (!charInfo.isVisible)
//                     continue;
                
//                 int vertexIndex = charInfo.vertexIndex;
                             
//                 //Vector2 charMidTopline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.topRight.y);
//                 Vector2 charMidBasline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.baseLine);

//                 // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
//                 //Vector3 offset = charMidTopline;
//                 Vector3 offset = charMidBasline;
                
//                 vertices[vertexIndex + 0] += -offset;
//                 vertices[vertexIndex + 1] += -offset;
//                 vertices[vertexIndex + 2] += -offset;
//                 vertices[vertexIndex + 3] += -offset;

//                 vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
//                 Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

//                 //matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, vertexAnim[i].angle), Vector3.one);
//                 //matrix = Matrix4x4.TRS(jitterOffset, Quaternion.identity, Vector3.one);
//                 matrix = Matrix4x4.TRS(jitterOffset * CurveScale, Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * AngleMultiplier), Vector3.one);

//                 vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
//                 vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
//                 vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
//                 vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                              
//                 vertices[vertexIndex + 0] += offset;
//                 vertices[vertexIndex + 1] += offset;
//                 vertices[vertexIndex + 2] += offset;
//                 vertices[vertexIndex + 3] += offset;

//                 vertexAnim[i] = vertAnim;
//             }

//             loopCount += 1;           

//             m_TextMeshPro.mesh.vertices = vertices;
//             m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo.uv0s;
//             m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo.uv2s;
//             //m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo.vertexColors;


//             //Debug.Log("Vertex Attributes Modified.");
//             yield return new WaitForSeconds(0.1f * SpeedMultiplier);
//         }          
//     }


//     private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
//     {
//         AnimationCurve newCurve = new AnimationCurve();
       
//         newCurve.keys = curve.keys;

//         return newCurve;
//     }


//     IEnumerator AnimateVertexPositionsIII()
//     {
//         VertexCurve.preWrapMode = WrapMode.Clamp;
//         VertexCurve.postWrapMode = WrapMode.Clamp;

//         Vector3[] vertexPositions;
//         Matrix4x4 matrix;

//         //int loopCount = 0;

//         m_TextMeshPro.hasChanged = true; // Need to force the TextMeshPro Object to be updated.
//         float old_CurveScale = CurveScale;
//         AnimationCurve old_curve = CopyAnimationCurve(VertexCurve);        

//         while (true)
//         {           
//             if (!m_TextMeshPro.hasChanged && old_CurveScale == CurveScale && old_curve.keys[1].value == VertexCurve.keys[1].value)
//             {
//                 yield return null;
//                 continue;
//             }

//             old_CurveScale = CurveScale;
//             old_curve = CopyAnimationCurve(VertexCurve);
//             //Debug.Log("Updating object!");

//             m_TextMeshPro.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
//             m_TextMeshPro.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

//             TMP_TextInfo textInfo = m_TextMeshPro.textInfo;
//             int characterCount = textInfo.characterCount;

//             Debug.Log(characterCount);

//             if (characterCount == 0) continue;

//             vertexPositions = textInfo.meshInfo.vertices;
//             //int lastVertexIndex = textInfo.characterInfo[characterCount - 1].vertexIndex;

//             float boundsMinX = m_TextMeshPro.bounds.min.x;
//             float boundsMaxX = m_TextMeshPro.bounds.max.x;

                           
//             for (int i = 0; i < characterCount; i++)
//             {
//                 if (!textInfo.characterInfo[i].isVisible)
//                     continue;

//                 int vertexIndex = textInfo.characterInfo[i].vertexIndex;

//                 // Compute the baseline mid point for each character
//                 Vector3 offsetToMidBaseline = new Vector2((vertexPositions[vertexIndex + 0].x + vertexPositions[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
//                 //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);                    

//                 // Apply offset to adjust our pivot point.
//                 vertexPositions[vertexIndex + 0] += -offsetToMidBaseline;
//                 vertexPositions[vertexIndex + 1] += -offsetToMidBaseline;
//                 vertexPositions[vertexIndex + 2] += -offsetToMidBaseline;
//                 vertexPositions[vertexIndex + 3] += -offsetToMidBaseline;

//                 // Compute the angle of rotation for each character based on the animation curve
//                 float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
//                 float x1 = x0 + 0.0001f;
//                 float y0 = VertexCurve.Evaluate(x0) * CurveScale;
//                 float y1 = VertexCurve.Evaluate(x1) * CurveScale;

//                 Vector3 horizontal = new Vector3(1, 0, 0);
//                 //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
//                 Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

//                 float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
//                 Vector3 cross = Vector3.Cross(horizontal, tangent);
//                 float angle = cross.z > 0 ? dot : 360 - dot;

//                 matrix = Matrix4x4.TRS(new Vector3(0, 0, y0), Quaternion.Euler(0, -angle, 0), Vector3.one);

//                 vertexPositions[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 0]);
//                 vertexPositions[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 1]);
//                 vertexPositions[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 2]);
//                 vertexPositions[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 3]);
                   
//                 vertexPositions[vertexIndex + 0] += offsetToMidBaseline;
//                 vertexPositions[vertexIndex + 1] += offsetToMidBaseline;
//                 vertexPositions[vertexIndex + 2] += offsetToMidBaseline;
//                 vertexPositions[vertexIndex + 3] += offsetToMidBaseline;
//             }
           

//             // Upload the mesh with the revised information
//             m_TextMeshPro.mesh.vertices = vertexPositions;
//             m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo.uv0s;
//             m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo.uv2s;
//             m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo.vertexColors;
           
//             yield return new WaitForSeconds(0.025f);
//         }

//     }



//     IEnumerator AnimateUIVertexPositionsIII()
//     {
//         VertexCurve.preWrapMode = WrapMode.Clamp;
//         VertexCurve.postWrapMode = WrapMode.Clamp;

//         CanvasRenderer uiRenderer = m_TextMeshProUGUI.canvasRenderer;
        
//         UIVertex[] uiVertices;
//         Matrix4x4 matrix;

//         //int loopCount = 0;

//         m_TextMeshProUGUI.hasChanged = true; // Need to force the TextMeshPro Object to be updated.
//         float old_CurveScale = CurveScale;
//         AnimationCurve old_curve = CopyAnimationCurve(VertexCurve);

//         while (true)
//         {
//             if (!m_TextMeshProUGUI.hasChanged && old_CurveScale == CurveScale && old_curve.keys[1].value == VertexCurve.keys[1].value)
//             {
//                 yield return null;
//                 continue;
//             }

//             old_CurveScale = CurveScale;
//             old_curve = CopyAnimationCurve(VertexCurve);
//             //Debug.Log("Updating object!");

//             m_TextMeshProUGUI.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
//             m_TextMeshProUGUI.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

//             TMP_TextInfo textInfo = m_TextMeshProUGUI.textInfo;
//             int characterCount = textInfo.characterCount;


//             if (characterCount == 0) continue;

//             uiVertices = textInfo.meshInfo.uiVertices;
//             //int lastVertexIndex = textInfo.characterInfo[characterCount - 1].vertexIndex;

//             float boundsMinX = m_TextMeshProUGUI.bounds.min.x;
//             float boundsMaxX = m_TextMeshProUGUI.bounds.max.x;


//             for (int i = 0; i < characterCount; i++)
//             {
//                 if (!textInfo.characterInfo[i].isVisible)
//                     continue;

//                 int vertexIndex = textInfo.characterInfo[i].vertexIndex;

//                 // Compute the baseline mid point for each character
//                 Vector3 offsetToMidBaseline = new Vector2((uiVertices[vertexIndex + 0].position.x + uiVertices[vertexIndex + 2].position.x) / 2, textInfo.characterInfo[i].baseLine);
//                 //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);                    

//                 // Apply offset to adjust our pivot point.
//                 uiVertices[vertexIndex + 0].position += -offsetToMidBaseline;
//                 uiVertices[vertexIndex + 1].position += -offsetToMidBaseline;
//                 uiVertices[vertexIndex + 2].position += -offsetToMidBaseline;
//                 uiVertices[vertexIndex + 3].position += -offsetToMidBaseline;

//                 // Compute the angle of rotation for each character based on the animation curve
//                 float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
//                 float x1 = x0 + 0.0001f;
//                 float y0 = VertexCurve.Evaluate(x0) * CurveScale;
//                 float y1 = VertexCurve.Evaluate(x1) * CurveScale;

//                 Vector3 horizontal = new Vector3(1, 0, 0);
//                 //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
//                 Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

//                 float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
//                 Vector3 cross = Vector3.Cross(horizontal, tangent);
//                 float angle = cross.z > 0 ? dot : 360 - dot;

//                 matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

//                 uiVertices[vertexIndex + 0].position = matrix.MultiplyPoint3x4(uiVertices[vertexIndex + 0].position);
//                 uiVertices[vertexIndex + 1].position = matrix.MultiplyPoint3x4(uiVertices[vertexIndex + 1].position);
//                 uiVertices[vertexIndex + 2].position = matrix.MultiplyPoint3x4(uiVertices[vertexIndex + 2].position);
//                 uiVertices[vertexIndex + 3].position = matrix.MultiplyPoint3x4(uiVertices[vertexIndex + 3].position);

//                 uiVertices[vertexIndex + 0].position += offsetToMidBaseline;
//                 uiVertices[vertexIndex + 1].position += offsetToMidBaseline;
//                 uiVertices[vertexIndex + 2].position += offsetToMidBaseline;
//                 uiVertices[vertexIndex + 3].position += offsetToMidBaseline;
//             }


//             // Upload the mesh with the revised information
//             uiRenderer.SetVertices(uiVertices, uiVertices.Length);

//             yield return new WaitForSeconds(0.025f);
//         }

//     }


//     IEnumerator AnimateVertexPositionsIV()
//     {

//         Matrix4x4 matrix;
//         Vector3[] vertices;

//         int loopCount = 0;

//         // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
//         VertexAnim[] vertexAnim = new VertexAnim[1024];
//         for (int i = 0; i < 1024; i++)
//         {
//             vertexAnim[i].angleRange = Random.Range(10f, 25f);
//             vertexAnim[i].speed = Random.Range(1f, 3f);
//         }

//         m_TextMeshPro.renderMode = TextRenderFlags.DontRender;

//         while (loopCount < 10000)
//         {
//             m_TextMeshPro.ForceMeshUpdate();
//             vertices = m_TextMeshPro.textInfo.meshInfo.vertices;

//             int characterCount = m_TextMeshPro.textInfo.characterCount;

//             for (int i = 0; i < characterCount; i++)
//             {
//                 // Setup initial random values
//                 VertexAnim vertAnim = vertexAnim[i];
//                 TMP_CharacterInfo charInfo = m_TextMeshPro.textInfo.characterInfo[i];

//                 // Skip Characters that are not visible
//                 if (!charInfo.isVisible)
//                     continue;

//                 int vertexIndex = charInfo.vertexIndex;

//                 Vector2 charMidTopline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.topRight.y);
//                 // Vector2 charMidBasline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.baseLine);

//                 // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
//                 Vector3 offset = charMidTopline;
//                 // Vector3 offset = charMidBasline;

//                 vertices[vertexIndex + 0] += -offset;
//                 vertices[vertexIndex + 1] += -offset;
//                 vertices[vertexIndex + 2] += -offset;
//                 vertices[vertexIndex + 3] += -offset;

//                 vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
//                 Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

//                 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, vertexAnim[i].angle), Vector3.one);
//                 //matrix = Matrix4x4.TRS(jitterOffset, Quaternion.identity, Vector3.one);
//                 //matrix = Matrix4x4.TRS(jitterOffset, Quaternion.Euler(0, 0, Random.Range(-5f, 5f)), Vector3.one);

//                 vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
//                 vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
//                 vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
//                 vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);


//                 vertices[vertexIndex + 0] += offset;
//                 vertices[vertexIndex + 1] += offset;
//                 vertices[vertexIndex + 2] += offset;
//                 vertices[vertexIndex + 3] += offset;

//                 vertexAnim[i] = vertAnim;
//             }

//             loopCount += 1;

//             m_TextMeshPro.mesh.vertices = vertices;
//             m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo.uv0s;
//             m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo.uv2s;
//             m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo.vertexColors;


//             //Debug.Log("Vertex Attributes Modified.");
//             yield return new WaitForSeconds(0.1f);
//         }
//     }


//     IEnumerator AnimateVertexPositionsV()
//     {
//         VertexCurve.preWrapMode = WrapMode.Loop;
//         VertexCurve.postWrapMode = WrapMode.Loop;

//         Vector3[] newVertexPositions;
//         //Matrix4x4 matrix;

//         int loopCount = 0;

//         while (true)
//         {
//             m_TextMeshPro.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
//             m_TextMeshPro.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

//             TMP_TextInfo textInfo = m_TextMeshPro.textInfo;
//             int characterCount = textInfo.characterCount;


//             newVertexPositions = textInfo.meshInfo.vertices;

//             for (int i = 0; i < characterCount; i++)
//             {
//                 if (!textInfo.characterInfo[i].isVisible)
//                     continue;

//                 int vertexIndex = textInfo.characterInfo[i].vertexIndex;

//                 float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);                    

//                 newVertexPositions[vertexIndex + 0].y += offsetY;
//                 newVertexPositions[vertexIndex + 1].y += offsetY;
//                 newVertexPositions[vertexIndex + 2].y += offsetY;
//                 newVertexPositions[vertexIndex + 3].y += offsetY;

//             }

//             loopCount += 1;

//             // Upload the mesh with the revised information           
//             m_TextMeshPro.mesh.vertices = newVertexPositions;
//             m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo.uv0s;
//             m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo.uv2s;
//             m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo.vertexColors;

//             yield return new WaitForSeconds(0.025f);
//         }

//     }



//     IEnumerator AnimateVertexPositionsVI()
//     {

//         Matrix4x4 matrix;
//         Vector3[] vertices;

//         int loopCount = 0;


//         // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
//         VertexAnim[] vertexAnim = new VertexAnim[1024];
//         for (int i = 0; i < 1024; i++)
//         {
//             vertexAnim[i].angleRange = Random.Range(90f, 90f);
//             vertexAnim[i].speed = Random.Range(1f, 3f);
//         }

//         m_TextMeshPro.renderMode = TextRenderFlags.DontRender;

//         int direction = 1;

//         m_TextMeshPro.ForceMeshUpdate();
//         vertices = m_TextMeshPro.textInfo.meshInfo.vertices;

//         while (loopCount < 10000)
//         {
//             //m_TextMeshPro.ForceMeshUpdate();
//             //vertices = m_TextMeshPro.textInfo.meshInfo.vertices;

//             int characterCount = m_TextMeshPro.textInfo.characterCount;

//             for (int i = 0; i < characterCount; i++)
//             {
//                 // Setup initial random values
//                 VertexAnim vertAnim = vertexAnim[i];
//                 TMP_CharacterInfo charInfo = m_TextMeshPro.textInfo.characterInfo[i];

//                 // Skip Characters that are not visible
//                 if (!charInfo.isVisible)
//                     continue;

//                 int vertexIndex = charInfo.vertexIndex;

//                 Vector2 charMidTopline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.topRight.y);
//                 // Vector2 charMidBasline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.baseLine);

//                 // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
//                 Vector3 offset = charMidTopline;
//                 // Vector3 offset = charMidBasline;

              

//                 float angle = 0;

//                 while (angle < 90)
//                 {
//                     vertices[vertexIndex + 0] += -offset;
//                     vertices[vertexIndex + 1] += -offset;
//                     vertices[vertexIndex + 2] += -offset;
//                     vertices[vertexIndex + 3] += -offset;
              
//                     matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 15 * direction, 0), Vector3.one);                  

//                     vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
//                     vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
//                     vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
//                     vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);


//                     vertices[vertexIndex + 0] += offset;
//                     vertices[vertexIndex + 1] += offset;
//                     vertices[vertexIndex + 2] += offset;
//                     vertices[vertexIndex + 3] += offset;  
                    
//                     m_TextMeshPro.mesh.vertices = vertices;
//                     m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo.uv0s;
//                     m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo.uv2s;
//                     m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo.vertexColors;

//                     angle += 15;

//                     yield return null;
//                     //vertexAnim[i] = vertAnim;  
//                 }

                       
//             }

//             loopCount += 1;

//             direction *= -1;


//             //Debug.Log("Vertex Attributes Modified.");
//             yield return new WaitForSeconds(0.1f);
//         }
//     }
// }
