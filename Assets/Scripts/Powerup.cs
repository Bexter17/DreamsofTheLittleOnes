using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum CollectibleType { HEAL, SPEED, DAMAGE, MAXHEALTH }

    public CollectibleType type;

    Character_Mechanics instance;



    void Start()
    {
        instance = FindObjectOfType<Character_Mechanics>();
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
                instance.currentHealth++;
                Destroy(gameObject);
                break;
            case CollectibleType.SPEED:
                instance.speed++;
                Destroy(gameObject);
                break;
             case CollectibleType.MAXHEALTH:
                instance.maxHealth++;
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
