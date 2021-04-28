using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_2 : MonoBehaviour
{
    public GameObject player;
    public GameObject tutMenu;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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
