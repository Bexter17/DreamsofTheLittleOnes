using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    #region Variables 

    [Header("VFX Spawn Position", order = 0)]
    [Tooltip("Takes position of where this parent object is; this is where the effect will instantiate from.")]
    public GameObject bjornHammerHitBox;
    public GameObject bjornFeet;
    public GameObject frontSpawn;
    [Space (order = 1)]

    [Header("VFX Prefabs", order = 2)]
    [Tooltip("The prefabs of the VFX are input here")]

    [Header("Bjorn Effects", order = 3)]
    public GameObject hammerTrail;
    public GameObject hammerCharge;
    public GameObject hammerSpin;
    public GameObject downSmash;
    public GameObject throwAxe;
    public GameObject bjornDeath;

    //Positions are stored in these variables and these are used for where the VFX will be spawned
    private Vector3 hammerPos;
    private Vector3 bjornFeetPos;
    private Vector3 frontSpawnPos;

    #endregion

    #region Position Updates
    void Update()
    {
        hammerPos = bjornHammerHitBox.transform.position;
        bjornFeetPos = bjornFeet.transform.position;
        //frontSpawnPos = frontSpawn.transform.position;
    }

    #endregion

    #region VFX Functions

        #region Bjorn VFX
    void HammerTrail() 
    {
        Instantiate(hammerTrail, hammerPos, Quaternion.identity, bjornHammerHitBox.transform);
    }

    void Hammerspin()
    {
        Instantiate(hammerTrail, hammerPos, Quaternion.identity, bjornHammerHitBox.transform);
    }
    void BjornDamageTaken() { }
        #endregion

    #endregion

    #region Other Functions

    /* Function Name: DestroyVFX
    * Use: Finds every instance of VFX called in the scene, gets the longest playing VFX for that particular 
    * effect and kills that entire object after a set amount of time.
    * 
    * Input: Takes the tag of the vfx tag to be found and destroyed
    * Output: No Output
    */
    void DestroyVFX(string vfxTag)
    {
        GameObject[] vfxToDestroy = GameObject.FindGameObjectsWithTag(vfxTag);
        ParticleSystem[] vfxPS = GatherPSInScene(vfxToDestroy);
        float vfxDuration = 0;

        foreach (ParticleSystem child in vfxPS)
        {
            if (child.main.duration > vfxDuration)
            {
                vfxDuration = child.main.duration;
            }
        }

        for (int i = 0; i < vfxToDestroy.Length; i++)
        {
            Destroy(vfxToDestroy[i], vfxDuration + 1f);
        }
    }

    /* Function Name: GatherPSInScene
     * Use: Gathers every particle system of every child of the vfxList so they can be referenced
     * 
     * Input: Takes an array of GameObjects of all VFX objects instantiated in the scene at that instance
     * Output: Outputs an array of every Particle System for earch of the VFX Game Objects
     */
    ParticleSystem[] GatherPSInScene(GameObject[] vfxList)
    {
        ParticleSystem[] vfxPS = new ParticleSystem[vfxList.Length];

        for (int i = 0; i < vfxList.Length; i++)
        {
            vfxPS[i] = vfxList[i].GetComponent<ParticleSystem>();
        }

        return vfxPS;
    }
    #endregion

}
