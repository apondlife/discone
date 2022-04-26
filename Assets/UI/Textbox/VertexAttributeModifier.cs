using UnityEngine;
using System.Collections;
using TMPro;

public class VertexAttributeModifier : MonoBehaviour
{
    public bool PlayOnEnable = true;

    public enum AnimationMode
    {
        Color = 0,
        Wave = 1,
        Jitter = 2,
        Dangling = 3,
        Reveal = 4,
        ShakeA = 5,
        ShakeB = 6,
        Warp = 7,
    }

    public AnimationMode MeshAnimationMode = AnimationMode.Wave;
    public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
    public float AngleMultiplier = 1.0f;
    public float SpeedMultiplier = 1.0f;
    public float CurveScale = 1.0f;

    [Header("Dangle")]
    [Range(.01f, .1f)] public float DangleRefresh = .05f;
    public Vector2 DanglingRange = new Vector2(10f, 25f);
    public Vector2 DanglingSpeed = new Vector2(1f, 3f);

    [Header("Shake")]
    public float ScaleMultiplier = 1.0f;
    public float RotationMultiplier = 1.0f;

    private bool hasTextChanged;
    private TMP_Text m_TextComponent;

    void OnEnable()
    {
        if (PlayOnEnable)
            PlayAnimation(MeshAnimationMode);
    }

    private struct VertexAnim
    {
        public float angleRange;
        public float angle;
        public float speed;
    }

    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    // [Button("Play")]
    private void Play()
    {
        PlayAnimation(MeshAnimationMode);
    }

//    [Button("Stop")]
    private void Stop()
    {
        StopAllCoroutines();
        ResetText();
    }

    public void PlayAnimation(AnimationMode _mode)
    {
        Stop();

        switch (_mode)
        {
            case AnimationMode.Color:
                StartCoroutine(AnimateColors());
                break;
            case AnimationMode.Wave:
                StartCoroutine(AnimateWave());
                break;
            case AnimationMode.Jitter:
                StartCoroutine(AnimateJitter());
                break;
            case AnimationMode.Warp:
                StartCoroutine(AnimateWarp());
                break;
            case AnimationMode.Dangling:
                StartCoroutine(AnimateDangling());
                break;
            case AnimationMode.Reveal:
                StartCoroutine(AnimateReveal());
                break;
            case AnimationMode.ShakeA:
                StartCoroutine(AnimateShakeA());
                break;
            case AnimationMode.ShakeB:
                StartCoroutine(AnimateShakeB());
                break;
        }
    }


    IEnumerator AnimateColors()
    {
        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        int currentCharacter = 0;

        Color32[] newVertexColors = textInfo.meshInfo[0].colors32;
        Color32 c0 = m_TextComponent.color;
        c0.a = 127;
        Color32 c1 = c0;

        m_TextComponent.renderMode = TextRenderFlags.DontRender;

        m_TextComponent.ForceMeshUpdate();

        while (true)
        {
            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            newVertexColors = textInfo.meshInfo[0].colors32;

            currentCharacter = (currentCharacter + 1) % characterCount;
            int vertexIndex = textInfo.characterInfo[currentCharacter].vertexIndex;

            if (!textInfo.characterInfo[currentCharacter].isVisible)
                continue;

            if (currentCharacter == 0)
            {
                c0 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 127);

            }

            c1 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 127);

            newVertexColors[vertexIndex + 0] = c1;
            newVertexColors[vertexIndex + 1] = c1;
            newVertexColors[vertexIndex + 2] = c1;
            newVertexColors[vertexIndex + 3] = c1;

            var mesh = m_TextComponent.mesh;

            mesh.vertices = textInfo.meshInfo[0].vertices;
            mesh.uv = textInfo.meshInfo[0].uvs0;
            mesh.uv2 = textInfo.meshInfo[0].uvs2;
            mesh.colors32 = newVertexColors;

            yield return new WaitForSeconds(0.05f);
        }
    }

    void ResetText()
    {
        ResetGeometry();
    }

    void ResetGeometry()
    {
        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var newVertexPositions = textInfo.meshInfo[i].vertices;

            // Upload the mesh with the revised information
            UpdateMesh(newVertexPositions, 0);
        }

        m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.
    }

    private void UpdateMesh(Vector3[] _vertex, int index)
    {
        m_TextComponent.mesh.vertices = _vertex;
        m_TextComponent.mesh.uv = m_TextComponent.textInfo.meshInfo[index].uvs0;
        m_TextComponent.mesh.uv2 = m_TextComponent.textInfo.meshInfo[index].uvs2;
        m_TextComponent.mesh.colors32 = m_TextComponent.textInfo.meshInfo[index].colors32;
    }

    IEnumerator AnimateWave()
    {
        VertexCurve.preWrapMode = WrapMode.Loop;
        VertexCurve.postWrapMode = WrapMode.Loop;

        Vector3[] newVertexPositions;
        //Matrix4x4 matrix;

        int loopCount = 0;

        while (true)
        {

            m_TextComponent.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
            //m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

            ResetGeometry();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            int characterCount = textInfo.characterCount;

            newVertexPositions = textInfo.meshInfo[0].vertices;

            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f) * CurveScale; // Random.Range(-0.25f, 0.25f);                    

                newVertexPositions[vertexIndex + 0].y += offsetY;
                newVertexPositions[vertexIndex + 1].y += offsetY;
                newVertexPositions[vertexIndex + 2].y += offsetY;
                newVertexPositions[vertexIndex + 3].y += offsetY;
            }

            loopCount += 1;

            // Upload the mesh with the revised information
            //m_TextComponent.mesh.vertices = newVertexPositions;
            //m_TextComponent.mesh.uv = m_TextComponent.textInfo.meshInfo[0].uvs0;
            //m_TextComponent.mesh.uv2 = m_TextComponent.textInfo.meshInfo[0].uvs2;
            //m_TextComponent.mesh.colors32 = m_TextComponent.textInfo.meshInfo[0].colors32;

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return new WaitForSeconds(0.025f);
        }
    }

    IEnumerator AnimateJitter()
    {
        Matrix4x4 matrix;
        Vector3[] vertices;

        int loopCount = 0;

        // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
        VertexAnim[] vertexAnim = new VertexAnim[1024];
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(10f, 25f);
            vertexAnim[i].speed = Random.Range(1f, 3f);
        }

        m_TextComponent.renderMode = TextRenderFlags.DontRender;

        while (loopCount < 10000)
        {
            m_TextComponent.ForceMeshUpdate();
            vertices = m_TextComponent.textInfo.meshInfo[0].vertices;

            int characterCount = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < characterCount; i++)
            {
                // Setup initial random values
                VertexAnim vertAnim = vertexAnim[i];
                TMP_CharacterInfo charInfo = m_TextComponent.textInfo.characterInfo[i];

                // Skip Characters that are not visible
                if (!charInfo.isVisible)
                    continue;

                int vertexIndex = charInfo.vertexIndex;

                //Vector2 charMidTopline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.topRight.y);
                Vector2 charMidBasline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.baseLine);

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                //Vector3 offset = charMidTopline;
                Vector3 offset = charMidBasline;

                vertices[vertexIndex + 0] += -offset;
                vertices[vertexIndex + 1] += -offset;
                vertices[vertexIndex + 2] += -offset;
                vertices[vertexIndex + 3] += -offset;

                vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

                //matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, vertexAnim[i].angle), Vector3.one);
                //matrix = Matrix4x4.TRS(jitterOffset, Quaternion.identity, Vector3.one);
                matrix = Matrix4x4.TRS(jitterOffset * CurveScale, Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * AngleMultiplier), Vector3.one);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);


                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;

                vertexAnim[i] = vertAnim;
            }

            var textInfo = m_TextComponent.textInfo;
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            loopCount += 1;

            //Debug.Log("Vertex Attributes Modified.");
            yield return new WaitForSeconds(0.1f * SpeedMultiplier);
        }
    }


    private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
    {
        AnimationCurve newCurve = new AnimationCurve();

        newCurve.keys = curve.keys;

        return newCurve;
    }


    // TODO: This doesn't do anything?
    IEnumerator AnimateWarp()
    {
        VertexCurve.preWrapMode = WrapMode.Clamp;
        VertexCurve.postWrapMode = WrapMode.Clamp;

        Vector3[] vertexPositions;
        Matrix4x4 matrix;

        //int loopCount = 0;

        m_TextComponent.ForceMeshUpdate();
        m_TextComponent.UpdateVertexData(); // Need to force the TextMeshPro Object to be updated.
        float old_CurveScale = CurveScale;
        AnimationCurve old_curve = CopyAnimationCurve(VertexCurve);

        while (true)
        {
            //!m_TextComponent.hasChanged && 
            if (old_CurveScale == CurveScale && old_curve.keys[1].value == VertexCurve.keys[1].value)
            {
                yield return null;
                Debug.Log("backing");
                continue;
            }

            old_CurveScale = CurveScale;
            old_curve = CopyAnimationCurve(VertexCurve);
            //Debug.Log("Updating object!");

            m_TextComponent.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
            m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            int characterCount = textInfo.characterCount;

            if (characterCount == 0) continue;

            vertexPositions = textInfo.meshInfo[0].vertices;
            //int lastVertexIndex = textInfo.characterInfo[characterCount - 1].vertexIndex;

            float boundsMinX = m_TextComponent.bounds.min.x;
            float boundsMaxX = m_TextComponent.bounds.max.x;


            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Compute the baseline mid point for each character
                Vector3 offsetToMidBaseline = new Vector2((vertexPositions[vertexIndex + 0].x + vertexPositions[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);                    

                // Apply offset to adjust our pivot point.
                vertexPositions[vertexIndex + 0] += -offsetToMidBaseline;
                vertexPositions[vertexIndex + 1] += -offsetToMidBaseline;
                vertexPositions[vertexIndex + 2] += -offsetToMidBaseline;
                vertexPositions[vertexIndex + 3] += -offsetToMidBaseline;

                // Compute the angle of rotation for each character based on the animation curve
                float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                float x1 = x0 + 0.0001f;
                float y0 = VertexCurve.Evaluate(x0) * CurveScale;
                float y1 = VertexCurve.Evaluate(x1) * CurveScale;

                Vector3 horizontal = new Vector3(1, 0, 0);
                //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

                float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                Vector3 cross = Vector3.Cross(horizontal, tangent);
                float angle = cross.z > 0 ? dot : 360 - dot;

                matrix = Matrix4x4.TRS(new Vector3(0, 0, y0), Quaternion.Euler(0, -angle, 0), Vector3.one);

                vertexPositions[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 0]);
                vertexPositions[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 1]);
                vertexPositions[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 2]);
                vertexPositions[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertexPositions[vertexIndex + 3]);

                vertexPositions[vertexIndex + 0] += offsetToMidBaseline;
                vertexPositions[vertexIndex + 1] += offsetToMidBaseline;
                vertexPositions[vertexIndex + 2] += offsetToMidBaseline;
                vertexPositions[vertexIndex + 3] += offsetToMidBaseline;
            }


            // Upload the mesh with the revised information
            m_TextComponent.mesh.vertices = vertexPositions;
            m_TextComponent.mesh.uv = m_TextComponent.textInfo.meshInfo[0].uvs0;
            m_TextComponent.mesh.uv2 = m_TextComponent.textInfo.meshInfo[0].uvs2;
            m_TextComponent.mesh.colors32 = m_TextComponent.textInfo.meshInfo[0].colors32;

            //for (int i = 0; i < textInfo.meshInfo.Length; i++)
            //{
            //    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            //    m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            //}

            yield return new WaitForSeconds(0.025f);
        }

    }


    IEnumerator AnimateDangling()
    {
        Matrix4x4 matrix;
        Vector3[] vertices;

        int loopCount = 0;

        // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
        VertexAnim[] vertexAnim = new VertexAnim[1024];
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(DanglingRange.x, DanglingRange.y);
            vertexAnim[i].speed = Random.Range(DanglingSpeed.x, DanglingSpeed.y);
        }

        m_TextComponent.renderMode = TextRenderFlags.DontRender;

        while (loopCount < 10000)
        {
            m_TextComponent.ForceMeshUpdate();
            vertices = m_TextComponent.textInfo.meshInfo[0].vertices;

            int characterCount = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < characterCount; i++)
            {
                // Setup initial random values
                VertexAnim vertAnim = vertexAnim[i];
                TMP_CharacterInfo charInfo = m_TextComponent.textInfo.characterInfo[i];

                // Skip Characters that are not visible
                if (!charInfo.isVisible)
                    continue;

                int vertexIndex = charInfo.vertexIndex;

                Vector2 charMidTopline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.topRight.y);
                // Vector2 charMidBasline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.baseLine);

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                Vector3 offset = charMidTopline;
                // Vector3 offset = charMidBasline;

                vertices[vertexIndex + 0] += -offset;
                vertices[vertexIndex + 1] += -offset;
                vertices[vertexIndex + 2] += -offset;
                vertices[vertexIndex + 3] += -offset;

                vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                //Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

                matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, vertexAnim[i].angle), Vector3.one);
                //matrix = Matrix4x4.TRS(jitterOffset, Quaternion.identity, Vector3.one);
                //matrix = Matrix4x4.TRS(jitterOffset, Quaternion.Euler(0, 0, Random.Range(-5f, 5f)), Vector3.one);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);


                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;

                vertexAnim[i] = vertAnim;
            }

            loopCount += 1;

            //m_TextComponent.mesh.vertices = vertices;
            //m_TextComponent.mesh.uv = m_TextComponent.textInfo.meshInfo[0].uvs0;
            //m_TextComponent.mesh.uv2 = m_TextComponent.textInfo.meshInfo[0].uvs2;
            //m_TextComponent.mesh.colors32 = m_TextComponent.textInfo.meshInfo[0].colors32;

            var textInfo = m_TextComponent.textInfo;
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }


            //Debug.Log("Vertex Attributes Modified.");
            yield return new WaitForSeconds(DangleRefresh);
        }
    }

    IEnumerator AnimateReveal()
    {
        Matrix4x4 matrix;
        Vector3[] vertices;

        int loopCount = 0;

        // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
        VertexAnim[] vertexAnim = new VertexAnim[1024];
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(90f, 90f);
            vertexAnim[i].speed = Random.Range(1f, 3f);
        }

        m_TextComponent.renderMode = TextRenderFlags.DontRender;

        int direction = 1;

        m_TextComponent.ForceMeshUpdate();
        vertices = m_TextComponent.textInfo.meshInfo[0].vertices;

        while (loopCount < 10000)
        {
            //m_TextMeshPro.ForceMeshUpdate();
            //vertices = m_TextMeshPro.textInfo.meshInfo.vertices;

            int characterCount = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < characterCount; i++)
            {
                // Setup initial random values
                VertexAnim vertAnim = vertexAnim[i];
                TMP_CharacterInfo charInfo = m_TextComponent.textInfo.characterInfo[i];

                // Skip Characters that are not visible
                if (!charInfo.isVisible)
                    continue;

                int vertexIndex = charInfo.vertexIndex;

                Vector2 charMidTopline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.topRight.y);
                // Vector2 charMidBasline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.baseLine);

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                Vector3 offset = charMidTopline;
                // Vector3 offset = charMidBasline;

                float angle = 0;

                while (angle < 90)
                {
                    vertices[vertexIndex + 0] += -offset;
                    vertices[vertexIndex + 1] += -offset;
                    vertices[vertexIndex + 2] += -offset;
                    vertices[vertexIndex + 3] += -offset;

                    matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 15 * direction, 0), Vector3.one);

                    vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);


                    vertices[vertexIndex + 0] += offset;
                    vertices[vertexIndex + 1] += offset;
                    vertices[vertexIndex + 2] += offset;
                    vertices[vertexIndex + 3] += offset;

                    //m_TextComponent.mesh.vertices = vertices;
                    //m_TextComponent.mesh.uv = m_TextComponent.textInfo.meshInfo[0].uvs0;
                    //m_TextComponent.mesh.uv2 = m_TextComponent.textInfo.meshInfo[0].uvs2;
                    //m_TextComponent.mesh.colors32 = m_TextComponent.textInfo.meshInfo[0].colors32;


                    var textInfo = m_TextComponent.textInfo;
                    for (int j = 0; j < textInfo.meshInfo.Length; j++)
                    {
                        textInfo.meshInfo[j].mesh.vertices = textInfo.meshInfo[j].vertices;
                        m_TextComponent.UpdateGeometry(textInfo.meshInfo[j].mesh, j);
                    }

                    angle += 15;

                    yield return null;
                    //vertexAnim[i] = vertAnim;  
                }
            }

            loopCount += 1;

            direction *= -1;


            //Debug.Log("Vertex Attributes Modified.");
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator AnimateShakeA()
    {
        // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
        // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
        m_TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        Matrix4x4 matrix;
        Vector3[][] copyOfVertices = new Vector3[0][];

        hasTextChanged = true;

        while (true)
        {
            // Allocate new vertices 
            if (hasTextChanged)
            {
                if (copyOfVertices.Length < textInfo.meshInfo.Length)
                    copyOfVertices = new Vector3[textInfo.meshInfo.Length][];

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    int length = textInfo.meshInfo[i].vertices.Length;
                    copyOfVertices[i] = new Vector3[length];
                }

                hasTextChanged = false;
            }

            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            int lineCount = textInfo.lineCount;

            // Iterate through each line of the text.
            for (int i = 0; i < lineCount; i++)
            {

                int first = textInfo.lineInfo[i].firstCharacterIndex;
                int last = textInfo.lineInfo[i].lastCharacterIndex;

                // Determine the center of each line
                Vector3 centerOfLine = (textInfo.characterInfo[first].bottomLeft + textInfo.characterInfo[last].topRight) / 2;
                Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(-0.25f, 0.25f) * RotationMultiplier);

                // Iterate through each character of the line.
                for (int j = first; j <= last; j++)
                {
                    // Skip characters that are not visible and thus have no geometry to manipulate.
                    if (!textInfo.characterInfo[j].isVisible)
                        continue;

                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[j].materialReferenceIndex;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[j].vertexIndex;

                    // Get the vertices of the mesh used by this text element (character or sprite).
                    Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

                    // Need to translate all 4 vertices of each quad to aligned with center of character.
                    // This is needed so the matrix TRS is applied at the origin for each character.
                    copyOfVertices[materialIndex][vertexIndex + 0] = sourceVertices[vertexIndex + 0] - centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 1] = sourceVertices[vertexIndex + 1] - centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 2] = sourceVertices[vertexIndex + 2] - centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 3] = sourceVertices[vertexIndex + 3] - centerOfLine;

                    // Determine the random scale change for each character.
                    float randomScale = Random.Range(0.995f - 0.001f * ScaleMultiplier, 1.005f + 0.001f * ScaleMultiplier);

                    // Setup the matrix rotation.
                    matrix = Matrix4x4.TRS(Vector3.one, rotation, Vector3.one * randomScale);

                    // Apply the matrix TRS to the individual characters relative to the center of the current line.
                    copyOfVertices[materialIndex][vertexIndex + 0] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 0]);
                    copyOfVertices[materialIndex][vertexIndex + 1] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 1]);
                    copyOfVertices[materialIndex][vertexIndex + 2] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 2]);
                    copyOfVertices[materialIndex][vertexIndex + 3] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 3]);

                    // Revert the translation change.
                    copyOfVertices[materialIndex][vertexIndex + 0] += centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 1] += centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 2] += centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 3] += centerOfLine;
                }
            }

            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = copyOfVertices[i];
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator AnimateShakeB()
    {
        // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
        // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
        m_TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        Vector3[][] copyOfVertices = new Vector3[0][];

        hasTextChanged = true;

        while (true)
        {
            // Allocate new vertices 
            if (hasTextChanged)
            {
                if (copyOfVertices.Length < textInfo.meshInfo.Length)
                    copyOfVertices = new Vector3[textInfo.meshInfo.Length][];

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    int length = textInfo.meshInfo[i].vertices.Length;
                    copyOfVertices[i] = new Vector3[length];
                }

                hasTextChanged = false;
            }

            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            int lineCount = textInfo.lineCount;

            // Iterate through each line of the text.
            for (int i = 0; i < lineCount; i++)
            {
                int first = textInfo.lineInfo[i].firstCharacterIndex;
                int last = textInfo.lineInfo[i].lastCharacterIndex;

                // Determine the center of each line
                Vector3 centerOfLine = (textInfo.characterInfo[first].bottomLeft + textInfo.characterInfo[last].topRight) / 2;
                Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(-RotationMultiplier, RotationMultiplier));

                // Iterate through each character of the line.
                for (int j = first; j <= last; j++)
                {
                    // Skip characters that are not visible and thus have no geometry to manipulate.
                    if (!textInfo.characterInfo[j].isVisible)
                        continue;

                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[j].materialReferenceIndex;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[j].vertexIndex;

                    // Get the vertices of the mesh used by this text element (character or sprite).
                    Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

                    // Determine the center point of each character at the baseline.
                    Vector3 charCenter = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                    // Need to translate all 4 vertices of each quad to aligned with center of character.
                    // This is needed so the matrix TRS is applied at the origin for each character.
                    copyOfVertices[materialIndex][vertexIndex + 0] = sourceVertices[vertexIndex + 0] - charCenter;
                    copyOfVertices[materialIndex][vertexIndex + 1] = sourceVertices[vertexIndex + 1] - charCenter;
                    copyOfVertices[materialIndex][vertexIndex + 2] = sourceVertices[vertexIndex + 2] - charCenter;
                    copyOfVertices[materialIndex][vertexIndex + 3] = sourceVertices[vertexIndex + 3] - charCenter;

                    // Determine the random scale change for each character.
                    float randomScale = Random.Range(1f - ScaleMultiplier, 1 + ScaleMultiplier);

                    // Setup the matrix for the scale change.
                    var matrix = Matrix4x4.TRS(Vector3.one, Quaternion.identity, Vector3.one * randomScale);

                    // Apply the scale change relative to the center of each character.
                    copyOfVertices[materialIndex][vertexIndex + 0] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 0]);
                    copyOfVertices[materialIndex][vertexIndex + 1] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 1]);
                    copyOfVertices[materialIndex][vertexIndex + 2] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 2]);
                    copyOfVertices[materialIndex][vertexIndex + 3] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 3]);

                    // Revert the translation change.
                    copyOfVertices[materialIndex][vertexIndex + 0] += charCenter;
                    copyOfVertices[materialIndex][vertexIndex + 1] += charCenter;
                    copyOfVertices[materialIndex][vertexIndex + 2] += charCenter;
                    copyOfVertices[materialIndex][vertexIndex + 3] += charCenter;

                    // Need to translate all 4 vertices of each quad to aligned with the center of the line.
                    // This is needed so the matrix TRS is applied from the center of the line.
                    copyOfVertices[materialIndex][vertexIndex + 0] -= centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 1] -= centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 2] -= centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 3] -= centerOfLine;

                    // Setup the matrix rotation.
                    matrix = Matrix4x4.TRS(Vector3.one, rotation, Vector3.one);

                    // Apply the matrix TRS to the individual characters relative to the center of the current line.
                    copyOfVertices[materialIndex][vertexIndex + 0] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 0]);
                    copyOfVertices[materialIndex][vertexIndex + 1] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 1]);
                    copyOfVertices[materialIndex][vertexIndex + 2] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 2]);
                    copyOfVertices[materialIndex][vertexIndex + 3] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 3]);

                    // Revert the translation change.
                    copyOfVertices[materialIndex][vertexIndex + 0] += centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 1] += centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 2] += centerOfLine;
                    copyOfVertices[materialIndex][vertexIndex + 3] += centerOfLine;
                }
            }

            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = copyOfVertices[i];
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}