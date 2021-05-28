using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class VFXenemies : MonoBehaviour
{
    #region Variables
    [Header("Amount of Objects", order = 0)]
    public int objectsToDissolve = 5;

    [Header("Objects to Dissolve", order = 1)]
    public GameObject[] ObjectsDissolving = new GameObject[6];

    //Array to store all the materials that need the dissolve triggered
    Material[] material;

    //Trigger states for Dissolving
    bool isDissolvingIn = false;
    bool isDissolvingOut = false;

    //Triggers for particle systems so they only run once
    bool spawnSandPS = false;
    bool deathSandPS = false;

    //Fade parameter; 1.9f is completely dissolved; -13 is fully visable
    public float fade = 1.9f;

    [Header("Speed of Dissolve", order = 2)]
    public float speed = 10;
    public float spawnSpeedDivider = 2.2f;
    public float deathSpeedDivider = 1;

    [Header("Enemy VFX Transform", order = 3)]
    public GameObject spawnFrom;
    Vector3 deathFrom = new Vector3(0, 1, 0);

    [Header("Enemy VFX Prefabs", order = 4)]
    public GameObject spawnSand;
    public GameObject deathSand;
    #endregion

    void Start()
    {
        //Grabs the materials component so their values can be changed when the function is called
        getMaterials();
    }

    void Update()
    {
        //Gets the time variable
        GetTime();
    }

    #region Functions
    public void getMaterials()
    {
        material = new Material[objectsToDissolve];

        for (int index = 0; index < objectsToDissolve; index++)
        {
            material[index] = ObjectsDissolving[index].GetComponent<Renderer>().material;
        }
    }

    /* Function Name: DissolveIn
    * Use: Gathers every material and dissolves the material of the enemy in; also calls the particle system
    * for the sand and then destroys it at the end.
    * 
    * Input: No Input
    * Output: No Output
    */
    public void DissolveIn()
    {
        float dissolveInSpeed = speed / spawnSpeedDivider;

        isDissolvingIn = true;

        if (isDissolvingIn)
        {
            if (!spawnSandPS)
            {
                SpawnSand(false);
                spawnSandPS = true;
            }

            fade -= GetTime() * dissolveInSpeed;
            if (fade <= -13)
            {
                fade = -13;
                isDissolvingIn = false;
            }

            for (int index = 0; index < objectsToDissolve; index++)
            {
                material[index].SetFloat("_Fade", fade);
            }

            DestroyVFX("VFX_Enemy");
        }
    }

    /* Function Name: DissolveOut
    * Use: Gathers every material and dissolves the material of the enemy out; also calls the particle system
    * for the sand and then destroys it at the end.
    * 
    * Input: No Input
    * Output: No Output
    */
    public void DissolveOut()
    {
        float dissolveOutSpeed = speed / deathSpeedDivider;

        isDissolvingOut = true;

        if (isDissolvingOut)
        {
            if (!deathSandPS)
            {
                SpawnSand(true);
                deathSandPS = true;
            }

            fade += GetTime() * dissolveOutSpeed;
            if (fade >= 1.9)
            {
                fade = 1.9f;
                isDissolvingOut = false;
            }

            for (int index = 0; index < objectsToDissolve; index++)
            {
                material[index].SetFloat("_Fade", fade);
            }

            DestroyVFX("VFX_Enemy");
        }
    }

    /* Function Name: SpawnSand
     * Use: Spawns the sand particle system based on if the enemy is spawning or dying
     * 
     * Input: Takes a boolean value on if the enemy is dead or not.
     * Output: No Output
     */
    void SpawnSand(bool isDead)
    {
        if (!isDead)
        {
            Instantiate(spawnSand, spawnFrom.transform.position, spawnFrom.transform.rotation, spawnFrom.transform);
        } else if (isDead)
        {
            Instantiate(deathSand, spawnFrom.transform.position + deathFrom, spawnFrom.transform.rotation, spawnFrom.transform);
        }
    }

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

        for(int i = 0; i < vfxToDestroy.Length; i++)
        {
            Destroy(vfxToDestroy[i], vfxDuration + 5f);
        }
    }

    /* Function Name: GetTime
     * Use: Gets the time in a float value when called.
     * 
     * Input: No Input
     * Output: Outputs a float of Time.deltaTime when called. 
     */
    float GetTime()
    {
        return Time.deltaTime;
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

       for(int i = 0; i < vfxList.Length; i++)
        {
            vfxPS[i] = vfxList[i].GetComponent<ParticleSystem>();
        }

        return vfxPS;
    }

    #endregion
}
