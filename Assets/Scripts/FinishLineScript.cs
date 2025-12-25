using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishLineScript : MonoBehaviour
{
    [Header("Level Settings")]
    public string nextLevel = ""; // Leave empty to reload current level
    public float delayBeforeNext = 2f;

    [Header("UI")]
    public GameObject winScreen;
    public Text winText;

    private bool finished = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if player reached finish line
        if (other.CompareTag("Player") && !finished)
        {
            finished = true;
            WinGame();
        }
    }

    void WinGame()
    {

        //PlayerControllerScript.instance.FinishLevel();
        PlayerControllerScript.instance.OnLevelComplete();
        

        
           winScreen.SetActive(true);
        
       
    }

}