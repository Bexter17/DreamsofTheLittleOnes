using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    CharacterMechanics cm;

    // Start is called before the first frame update
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Level_1")
        cm = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!cm)
        {
            if (SceneManager.GetActiveScene().name == "Level_1")
                cm = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();
        }
    }
    public void GivePlayerDamage(Transform dmgDealer, int dmg)
    {
        if (cm)
            cm.takeDamage(dmgDealer, dmg);
        else
            Debug.LogError("CharacterMechanics not attached to the CombatManager!");
    }
}
