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
        
     
       
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
           
            //Invoke("Kill", 0.1f);

           // tutMenu.SetActive(!tutMenu.activeSelf);
            tutMenu.SetActive(true);
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

    private void OnTriggerExit(Collider other)
    {
        tutMenu.SetActive(false);
        Kill();
        
    }

    public void Kill()
     {
        Destroy(gameObject);
     }
    
}


  

