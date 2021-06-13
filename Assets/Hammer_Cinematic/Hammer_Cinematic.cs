using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hammer_Cinematic : MonoBehaviour
{
    #region Scripts

    AnimController ac;

    CharacterMechanics cm;

    InputControl ic;

    AbilitiesCooldown cooldown;

    CinemachineDollyCart dc;

    CameraController cc;

    #endregion

    #region Cinematic Objects

    public GameObject Walls;

    #endregion


    enum Cinematic { HammerGame_Cinematic };

    // Start is called before the first frame update
    void Start()
    {
        #region Initialization

        ic = GameObject.FindGameObjectWithTag("Player").GetComponent<InputControl>();

        cm = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();

        dc = GameObject.FindGameObjectWithTag("hDC").GetComponent<CinemachineDollyCart>();

        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Disable player cursor
            ic.hammerGameCinematic = true;
            //Dolly Cart Speed
            dc.m_Speed = 8f;
            StartCoroutine(HammerGame_Cinematic());

        }
    }

    IEnumerator HammerGame_Cinematic()
    {
        yield return new WaitForSeconds(10f);
        ic.hammerGameCinematic = false;
        Destroy(Walls);

    }
}