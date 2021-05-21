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

    [Header("Enemy Effects", order = 4)]
    public GameObject enemySpawn;
    public GameObject enemyDeath;
    public GameObject enemyTrail;
    public GameObject enemyProjectile;
    public GameObject enemyDamageRecieved;

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
    void HammerTrail() {
        Instantiate(hammerTrail, hammerPos, Quaternion.identity, bjornHammerHitBox.transform);
    }

    void Hammerspin()
    {
        Instantiate(hammerTrail, hammerPos, Quaternion.identity, bjornHammerHitBox.transform);
    }
    void BjornDamageTaken() { }
    #endregion

    #region Enemy VFX

    #endregion

    #endregion

    #region Other Functions

    void DestroyVFX(string vfxname)
    {
        Destroy(GameObject.Find(vfxname));
    }
    GameObject[] GetChildren(GameObject vfxPrefab)
    {
        GameObject[] vfxChildren = vfxPrefab.GetComponents<GameObject>();
        
        return vfxChildren;
    }
    #endregion

}
