using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using Yarn.Unity;

public class DialogueSystem : MonoBehaviour
{
    public PlayerInput input;
   //public Action OnUnfreeze;

    public RawImage characterFaceUI; // the panel that shows the person's face as they are speaking

    public DialogueRunner yarnDialogueRunner;

    private bool _busy;

    private bool _isTalkAvailable;

    // Start is called before the first frame update
    void Start()
    {
        _busy = false;
        _isTalkAvailable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTalkAvailable(bool b) {
      _isTalkAvailable = b;
    }
    public bool IsTalkAvailable() {
      return _isTalkAvailable;
    }

    public void StartDialogue(string yarnDialogueTitle, Texture faceTexture) {
      Freeze();
      _busy = true;

      if(characterFaceUI != null) characterFaceUI.texture = faceTexture;

      // Tell Yarn dialogue to start
      yarnDialogueRunner.StartDialogue(yarnDialogueTitle);
    }

    private void Freeze() {
      // freeze player character.
      if (input != null) input.enabled = false;

      Debug.Log("Freeze");
    }   
    public void DialogueEnd() {
      _busy = false;
      // unfreeze player character.
      if (input != null) input.enabled = true;
      Debug.Log("Unfreeze");
      //OnUnfreeze();
    }

    public bool IsBusy() {
      return _busy;
    }
}
