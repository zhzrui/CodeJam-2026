using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }
    public void PlayGame(){
        SceneManager.LoadScene("MainScene_Saucy");
    }

    public void QuitGame(){
        Application.Quit();
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene("TitleScreen");
    }

}
