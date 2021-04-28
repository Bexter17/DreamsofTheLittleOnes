using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSorter : IComparer {
    int IComparer.Compare(object x, object y) {
        return ((new CaseInsensitiveComparer()).Compare(((GameObject)x).name, ((GameObject)y).name));
    }
}

public class GameManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    public void BuildCheckpointsList()
    {
        IComparer sorter = new CheckpointSorter();
        checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
        Array.Sort(checkPoints, sorter);
    }

    public void UpdateCheckpoint(GameObject newCheckPoint)
    {
        currentCheckpoint = Array.IndexOf(checkPoints, newCheckPoint);
    }

    public GameObject GetCurrentCheckpoint()
    {
        return checkPoints[currentCheckpoint];
    
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (SceneManager.GetActiveScene().name == "MainMenu")
        //    {
        //        SceneManager.LoadScene("MainScene");
        //    }
        //    else if (SceneManager.GetActiveScene().name == "MainScene")
        //    {
        //        SceneManager.LoadScene("MainMenu");
        //    }
        //    else if (SceneManager.GetActiveScene().name == "EndScene")
        //    {
        //        SceneManager.LoadScene("MainMenu");
        //    }
        //}
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void Return()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TryAgain()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private static GameManager instance;

    public static GameManager Instance { get => instance; }
    public GameObject[] checkPoints;
    public int currentCheckpoint;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);



        }
    }
}