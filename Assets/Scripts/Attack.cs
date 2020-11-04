using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            other.SendMessage("takeDamage", 10);
            other.GetComponent<EnemyCombat>().takeDamage(1);
            Debug.Log("takeDamage triggered on" + other.name);
        }
    }
}
