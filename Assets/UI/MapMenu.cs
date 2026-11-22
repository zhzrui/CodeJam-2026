using UnityEngine;
using UnityEngine.InputSystem;

public class MapMenu : MonoBehaviour
{
    public static bool mapOpen = false;
    public GameObject mapMenuUI;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("e press");
            if (mapOpen)
            {
                mapMenuUI.SetActive(false);
                mapOpen = false;    
            } 
            else
            {
                mapMenuUI.SetActive(true);
                mapOpen = true;
            }
        }
    }
}
