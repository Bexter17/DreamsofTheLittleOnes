using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    GameManager gm;

    public Button startButton;
    public Button quitButton;
    public Button returnButton;

    public GameObject pauseMenu;

    AudioSource pauseAudio;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            if (pauseMenu.activeSelf)
            {
                // pauseAudio.Play();
            }
        }
        if (pauseMenu)
        {
            if (pauseMenu.activeSelf)
            {
                Time.timeScale = 0.0f;

            }
            else
            {
                Time.timeScale = 1.0f;
            }

        }

    }
}