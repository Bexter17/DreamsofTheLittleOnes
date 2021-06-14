using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectiles : MonoBehaviour
{
    public float projectileSpeed;
    public float lifeTime;
    public int dmgDealt;
    private GameObject Player;
    //private CharacterMechanics cm;
    CombatManager CombatScript;
    CharacterMechanics CharacterMechanicsScript;

    float currentXRot;
    [SerializeField] float balloonAngle = 5;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        //cm = Player.GetComponent<CharacterMechanics>();
        CombatScript = GameObject.Find("GameManager").GetComponent<CombatManager>();
        CharacterMechanicsScript = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();

        if (lifeTime <= 0)
            lifeTime = 3.0f;

        if (dmgDealt <= 0)
            dmgDealt = 3;
        currentXRot = transform.localEulerAngles.x;
        Debug.Log("Big Boy Clown: " + currentXRot);
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        //transform.rotation = Quaternion.Euler(currentXRot, -60, transform.rotation.z);
        //currentXRot += 1;
        //transform.localEulerAngles = new Vector3(currentXRot, transform.localEulerAngles.y, transform.localEulerAngles.z);

        transform.rotation *= Quaternion.AngleAxis(balloonAngle, Vector3.right);



    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            CharacterMechanicsScript.takeDamage(this.transform, dmgDealt);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
