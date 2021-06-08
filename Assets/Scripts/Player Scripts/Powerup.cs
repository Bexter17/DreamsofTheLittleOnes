using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum CollectibleType { HEAL, SPEED, DAMAGE, MAXHEALTH }

    public CollectibleType type;

    CharacterMechanics instance;



    void Start()
    {
        instance = FindObjectOfType<CharacterMechanics>();
    }
    // activating power up for player
    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            Pickup();
        }

    }


    void Pickup()
    {
        switch(type)
        {
            case CollectibleType.HEAL:
                instance.currentHealth += 25;
                Destroy(gameObject);
                break;
            case CollectibleType.SPEED:
                //instance.speed++;
                Destroy(gameObject);
                break;
             case CollectibleType.MAXHEALTH:
                instance.maxHealth += 5;
                Destroy(gameObject);
                break;
           // case CollectibleType.DAMAGE:
            //    instance.damage++;
            //    Destroy(gameObject);
             //   break;
                
        }
        Debug.Log("Power up picked up");
        

    }



}
