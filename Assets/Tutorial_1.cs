using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class Tutorial_1 : MonoBehaviour
{
    public Transform first_position;
    public GameObject player;
    public GameObject thing;
    public GameObject tutMenu;

    public bool FreezeTime;
    public bool done_Tutorial_1;
    // Start is called before the first frame update
    void Start()
    {
        FreezeTime = false;
        player = GameObject.FindGameObjectWithTag("Player");
        done_Tutorial_1 = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        //Time.timeScale = 1;
        Debug.LogWarning("Test - " + Time.timeScale + " - " + FreezeTime);
        if(FreezeTime)
        {
            Time.timeScale = 0;
            Debug.Log(Time.time + " Freeze Game - " + Time.timeScale);

        }
       
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
            FreezeTime = true;
            Debug.Log(player);
            player.transform.position = new Vector3(first_position.position.x, first_position.position.y, first_position.position.z);
            Invoke("Kill", 0.1f);

            tutMenu.SetActive(!tutMenu.activeSelf);
            if (tutMenu.activeSelf)
            {
              //pauseAudio.Play();
            }
            //if (pauseMenu)
            //{
            //    if (pauseMenu.activeSelf)
            //    {
            //        Time.timeScale = 0.0f;

            //    }
            //    else if (pauseMenu && Input.GetKeyDown(KeyCode.P))
            //    {
            //        Time.timeScale = 1.0f;
            //    }
            
        }

        
    }

    public void Kill()
     {
        Destroy(gameObject);
     }
    
}


  

