using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectiles : MonoBehaviour
{
    public float projectileSpeed;
    public float lifeTime;
    public int dmgDealt;
    private GameObject Player;
    private CharacterMechanics cm;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        cm = Player.GetComponent<CharacterMechanics>();

        if (lifeTime <= 0)
            lifeTime = 3.0f;

        if (dmgDealt <= 0)
            dmgDealt = 3;

        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Destroy(gameObject);
            cm.takeDamage(this.transform, dmgDealt);
        }
    }
}
