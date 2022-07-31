using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public GameObject pauseMenu, resultMenu;
    public static bool gameEnded = false;
    public static bool isPaused = false;

    void Update()
    {
        if (!gameEnded && Input.GetButtonDown("Pause"))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Pause()
    {
        FindObjectOfType<AudioManager>().Play("Button");
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        FindObjectOfType<AudioManager>().Play("Button");
        pauseMenu.SetActive(false);
        resultMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Restart()
    {
        Resume();
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Resume();
        FindObjectOfType<AudioManager>().StopAll();
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}