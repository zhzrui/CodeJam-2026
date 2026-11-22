using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private int coinsToWin = 10;

    private int currentCoins = 0;
    private bool gameEnded = false;

    void Awake()
    {
        Instance = this;
    }

    public void CollectCoin()
    {
        if (gameEnded) return;
        currentCoins++;
        if (currentCoins >= coinsToWin)
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        SceneManager.LoadScene("WinScene");
    }

    public void LoseGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        SceneManager.LoadScene("LoseScene");
    }
}
