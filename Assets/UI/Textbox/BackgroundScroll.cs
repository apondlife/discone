using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroll : MonoBehaviour
{

    CanvasRenderer rend;
    RawImage img;
    public float scrollSpeed = .3f;


    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<CanvasRenderer>();
        img = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {

        // float offset = Time.time * scrollSpeed;



        // var mat = rend.GetMaterial();
        // if (mat) {
        //     rend.GetMaterial().SetTextureOffset("_MainTex", new Vector2(-offset, offset));
        // }


        float newX = img.uvRect.x + scrollSpeed;
        float newY = img.uvRect.y + scrollSpeed;
        img.uvRect = new Rect(newX, newY, img.uvRect.width, img.uvRect.height);

    }
}