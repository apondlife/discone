using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

public class RegionDetector : MonoBehaviour
{

    [Tooltip("player entered region")]
    [SerializeField] private StringEvent m_RegionEntered;

    [Tooltip("name of the region that this detector is detecting")]
    [SerializeField] private string m_RegionName;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Physics settings should be set so that only things on the 
    // Player layer will trigger RegionDetector layer
    void OnTriggerEnter() {
        Debug.Log("enter enter enter enter");
        m_RegionEntered.Raise(m_RegionName);

    }
}
