using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour {
  public string talkButton = "Fire1"; // TODO replace with proper input thing

  public string dialogueMessage = "KnightSpeak";

  public Texture characterPreview;

  public GameObject talkable;

  private bool _canTalk = false;

  public DialogueManager dm;

  private const string _dialogueTargetTag = "PlayerDialogueTarget";

  // Use this for initialization
  void Start () {
    if (talkable) talkable.SetActive(false);
  }
  
  // Update is called once per frame
  void Update () {
    // TODO: probably get input in a nicer way
    if (_canTalk && !dm.IsBusy() && Mouse.current.leftButton.isPressed) {
      Debug.Log("start dialog "+dialogueMessage);

      dm.StartDialogue(dialogueMessage, characterPreview);
    }
  }

  void OnTriggerEnter(Collider other) {
    if (other.CompareTag(_dialogueTargetTag) && !dm.IsTalkAvailable() && !dm.IsBusy()) {
      _canTalk = true;
      dm.SetTalkAvailable(true);
      if (talkable) talkable.SetActive(true);
    }
  }

  void OnTriggerExit(Collider other) {
    if (other.CompareTag(_dialogueTargetTag) && _canTalk) {
      _canTalk = false;
      dm.SetTalkAvailable(false);
      if (talkable) talkable.SetActive(false);
    }
  }
}
