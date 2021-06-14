using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
   

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Menu Music");
        if (objs.Length > 1)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        
    }

    private void Update()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "Level_1")
        {
            Destroy(gameObject);
        }

    }
    public void stopMusic()
    {

    }

}
