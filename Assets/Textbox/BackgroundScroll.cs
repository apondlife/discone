using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{

    CanvasRenderer rend;
    public float scrollSpeed = .3f;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<CanvasRenderer>();
        Debug.Log(rend);
        
    }

    // Update is called once per frame
    void Update()
    {

        float offset = Time.time * scrollSpeed;
        rend.GetMaterial().SetTextureOffset("_MainTex", new Vector2(-offset, offset));
        
    }
}
