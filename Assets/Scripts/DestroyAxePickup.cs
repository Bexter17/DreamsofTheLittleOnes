using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAxePickup : MonoBehaviour
{
    CharacterMechanics cm;

    public GameObject axeAbility;
    //public GameObject childObj;
    public GameObject Player;

    private void Start()
    {
        cm = Player.GetComponent<CharacterMechanics>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            axeAbility.SetActive(true);
            Debug.Log("Bjorn recieves throwing axe");
            cm.hasRangedWeapon = true;
           // childObj.SetActive(false);
            Destroy(gameObject);
        }
    }
}