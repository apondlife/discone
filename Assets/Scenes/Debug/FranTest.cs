using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityAtoms.Discone;
using Yarn.Unity;

public class FranTest : MonoBehaviour
{
    [Tooltip("when to switch character")]
    [SerializeField] GameObjectEvent m_StartDialogue;

    [Tooltip("when to switch character")]
    [SerializeField] GameObject testNPC;

    [Tooltip("the dialogue runner")]
    [SerializeField] DialogueRunner yarnDialogueRunner;

    [Tooltip("region event")]
    [SerializeField] RegionEvent m_RegionEntered;
    [SerializeField] private RegionConstant m_Region;

    // Start is called before the first frame update
    void Start()
    {
        m_RegionEntered.Raise(m_Region.Value);
        //StartCoroutine(RunDialogueNodeWhenItsLoaded());

        StartCoroutine(CoroutineHelpers.DoAfterRealtimeTimeCoroutine(3, RaiseRegionEvent));
        
    }

    void RaiseRegionEvent() {
        m_RegionEntered.Raise(m_Region.Value);

    }

    IEnumerator RunDialogueNodeWhenItsLoaded() {
        while (true) {
            if (yarnDialogueRunner.NodeExists(testNPC.GetComponent<NPCDialogue>().NodeTitle)) {
                m_StartDialogue.Raise(testNPC);
                break;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
