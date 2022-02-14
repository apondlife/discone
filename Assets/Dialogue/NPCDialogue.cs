using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour {

  public PlayerInput input;

  public string dialogueMessage;

  public Texture characterPreview;

  public GameObject talkable;

  private bool _canTalk = false;

  public DialogueSystem ds;

  private const string _dialogueTargetTag = "PlayerDialogueTarget";

  InputAction talkAction;

  // Use this for initialization
  void Start () {
    talkAction = input.currentActionMap["Jump"];

    if (talkable) talkable.SetActive(false);
  }
  
  // Update is called once per frame
  void Update () {

    if (_canTalk && !ds.IsBusy() && talkAction.IsPressed()) {
      Debug.Log("start dialog " + dialogueMessage);

      ds.StartDialogue(dialogueMessage, characterPreview);
    }
  }

  void OnTriggerEnter(Collider other) {
    if (other.CompareTag(_dialogueTargetTag) && !ds.IsTalkAvailable() && !ds.IsBusy()) {
      Debug.Log("can talk!!!");
      _canTalk = true;
      ds.SetTalkAvailable(true);
      if (talkable) talkable.SetActive(true);
    }
  }

  void OnTriggerExit(Collider other) {
    if (other.CompareTag(_dialogueTargetTag) && _canTalk) {
      _canTalk = false;
      ds.SetTalkAvailable(false);
      if (talkable) talkable.SetActive(false);
    }
  }
}
