using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirdPerson;

public class CycleCharacters : MonoBehaviour
{
    private Player player;
    private ThirdPerson.ThirdPerson[] characters;
    private int current = 0;
    // Start is called before the first frame update
    void Start()
    {
        characters = FindObjectsOfType<ThirdPerson.ThirdPerson>();
        player = GetComponentInParent<Player>();
    }

    public void Cycle() {
        current = (current + 1) % characters.Length;
        player.Drive(characters[current]);
    }
}
