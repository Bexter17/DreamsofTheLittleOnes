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

    public Button startButton;
    public Button quitButton;
    public Button returnButton;
    public Button tryAgainButton;

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
        if (cm)
        {
            if (!cm.isPlaying)
            {
                if (!isPaused)
                {
                    if (pauseMenu)
                    {
                        pauseMenu.SetActive(!pauseMenu.activeSelf);

                        if (pauseMenu.activeSelf)
                        {
                            //pauseaudio.play();
                        }

                        Time.timeScale = 0.0f;

                        isPaused = true;
                    }

                }
            }

            else if (cm.isPlaying)
            {
                pauseMenu.SetActive(false);

                Time.timeScale = 1.0f;

                isPaused = false;
            }
        }

    }

}

