using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using Yarn.Unity;

public class FranTest : MonoBehaviour
{
    [Tooltip("when to switch character")]
    [SerializeField] GameObjectEvent m_StartDialogue;

    [Tooltip("when to switch character")]
    [SerializeField] GameObject testNPC;

    [Tooltip("the dialogue runner")]
    [SerializeField] DialogueRunner yarnDialogueRunner;

    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(RunDialogueNodeWhenItsLoaded());
        
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
