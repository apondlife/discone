using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using Yarn.Unity;
using UnityAtoms.BaseAtoms;

public class DialogueSystem : MonoBehaviour
{
    public RawImage characterFaceUI; // the panel that shows the person's face as they are speaking

    public DialogueRunner yarnDialogueRunner;

    [SerializeField] private BoolVariable m_IsBusy;

    // Start is called before the first frame update
    void Start()
    {
        m_IsBusy.Value = false;
    }

    public void SetFaceTexture(Texture faceTexture) {
      if(characterFaceUI != null) characterFaceUI.texture = faceTexture;
    }

    public void StartDialogue(string yarnDialogueTitle) {
      m_IsBusy.Value = true;
      // Tell Yarn dialogue to start
      yarnDialogueRunner.StartDialogue(yarnDialogueTitle);
    }

    public void NextLine() {
      yarnDialogueRunner.OnViewUserIntentNextLine();
    }

    public void StartDialogue(string yarnDialogueTitle, Texture faceTexture) {
      SetFaceTexture(faceTexture);
      StartDialogue(yarnDialogueTitle);
    }

    public void DialogueEnd() {
      m_IsBusy.Value = false;
    }
}
