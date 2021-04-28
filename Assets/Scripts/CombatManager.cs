using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    CharacterMechanics cm;

    // Start is called before the first frame update
    void Start()
    {
        cm = GameObject.Find("Player").GetComponent<CharacterMechanics>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GivePlayerDamage(Transform dmgDealer, int dmg)
    {
        cm.takeDamage(dmgDealer, dmg);
    }
}
