using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    GameManager gm;

    GameObject Player;

    CharacterMechanics cm;

    bool isPaused;

    [SerializeField] bool canvasDebug;

    public Button startButton;
    public Button quitButton;
    public Button returnButton;
    public Button tryAgainButton;
    public Button controlsButton;
    public Button optionsButton;

    public GameObject pauseMenu;
   
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (startButton)
        {
            startButton.onClick.AddListener(gm.StartGame);
        }

        if (quitButton)
        {
            quitButton.onClick.AddListener(gm.QuitGame);
        }

        if (returnButton)
        {
            returnButton.onClick.AddListener(gm.Return);
        }

        if(tryAgainButton)
        {
            tryAgainButton.onClick.AddListener(gm.TryAgain);
        }

        if(controlsButton)
        {
            controlsButton.onClick.AddListener(gm.Controls);
        }

        if(optionsButton)
        {
            optionsButton.onClick.AddListener(gm.Options);
        } 
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Level_1")
        {
            try
            {
                Player = GameObject.FindGameObjectWithTag("Player");

                cm = Player.GetComponent<CharacterMechanics>();
            }

            catch (MissingReferenceException e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Level_1" || SceneManager.GetActiveScene().name == "MazeScene")
        {
            if (cm)
            {
                #region Debug
                if (canvasDebug)
                    Debug.Log("Canvas: cm is attached");
                #endregion

                if (!cm.isPlaying)
                {
                    #region Debug
                    if (canvasDebug)
                        Debug.Log("Canvas: cm.isPlaying = " + cm.isPlaying);
                    #endregion

                    if (!isPaused)
                    {
                        #region Debug
                        if (canvasDebug)
                            Debug.Log("Canvas: isPaused = " + isPaused);
                        #endregion

                        if (pauseMenu)
                        {
                            #region Debug
                            if (canvasDebug)
                                Debug.Log("Canvas: pauseMenu attached");
                            #endregion

                            pauseMenu.SetActive(!pauseMenu.activeSelf);

                            if (pauseMenu.activeSelf)
                            {
                                //pauseaudio.play();
                            }

                            Time.timeScale = 0.0f;

                            isPaused = true;

                            #region Debug
                            if (canvasDebug)
                                Debug.Log("Canvas: isPaused set to = " + isPaused);
                            #endregion
                        }

                    }
                }

                else if (cm.isPlaying)
                {
                    #region Debug
                    if (canvasDebug)
                        Debug.Log("Canvas: cm.isPlaying = " + cm.isPlaying);
                    #endregion

                    if (pauseMenu)
                    {
                        #region Debug
                        if (canvasDebug)
                            Debug.Log("Canvas: pauseMenu attached");
                        #endregion

                        pauseMenu.SetActive(false);

                        Time.timeScale = 1.0f;

                        isPaused = false;

                        #region Debug
                        if (canvasDebug)
                            Debug.Log("Canvas: isPaused set to = " + isPaused);
                        #endregion
                    }
                }
            }

            else
            {
                Debug.LogError("Canvas: cm not attached!");

                try
                {
                    cm = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();
                }

                catch (MissingComponentException e)
                {
                    Debug.LogError(e.Message);
                }

                if (cm)
                    Debug.Log("CharacterMechanics successfully attached!");
            }
        }
    }

}

