using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDestroy : MonoBehaviour
{
    [SerializeField] bool smashDebug;

    public BoxCollider box1;

    public BoxCollider box2;

    public BoxCollider box3;

    public GameObject ability;

    int smashCycle;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("Smash Prefab created!");

        smashCycle = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        Destroy(ability, 3);

        if (smashCycle == 0)
        {
            if (smashDebug)
                Debug.Log("Smash: Collider 1 enabled!");

            box1.enabled = true;
            box2.enabled = false;
            box3.enabled = false;
        }

        if (smashCycle == 1)
        {

            if (smashDebug)
                Debug.Log("Smash: Collider 2 enabled!");

            box1.enabled = false;
            box2.enabled = true;
            box3.enabled = false;
        }

        if (smashCycle == 2)
        {
            if (smashDebug)
                Debug.Log("Smash: Collider 3 enabled!");

            box1.enabled = false;
            box2.enabled = false;
            box3.enabled = true;
        }

        if (smashCycle > 2)
        {
            if(smashDebug)
            Debug.Log("Smash: All colliders  disabled!");

            box1.enabled = false;
            box2.enabled = false;
            box3.enabled = false;
        }

        if(smashCycle < 2)
        smashCycle++;

        if(smashDebug)
        Debug.Log("Smash: smashCycle = " + smashCycle);
    }
}
