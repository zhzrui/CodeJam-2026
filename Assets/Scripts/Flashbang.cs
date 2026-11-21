using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Flashbang : MonoBehaviour
{
    [Header("Flashbang Settings")]
    [SerializeField] private Light lightSource;
    [SerializeField] private float flashDuration = 1f;
    [SerializeField] private float flashIntensity = 5f;

    private EnemyChase enemyChaseScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyChaseScript = FindFirstObjectByType<EnemyChase>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            StartCoroutine(FlashEffect());
            if (enemyChaseScript != null)
            {
                enemyChaseScript.Stun();
            }
        }
    }

    public IEnumerator FlashEffect() 
    {
        Debug.Log("Flashbang activated!");
        
        lightSource.intensity = flashIntensity;
        lightSource.gameObject.SetActive(true);
        
        // Wait one frame so the player sees the peak flash before fading begins
        yield return null;

        float elapsedTime = 0f;

        while (elapsedTime < flashDuration) 
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / flashDuration);
            
            // Exponential curve: drops from 1,000,000 to ~1,000 quickly, then fades smoothly
            float curve = Mathf.Pow(1f - t, 4f); 
            lightSource.intensity = flashIntensity * curve;
            
            yield return null;
        }


        lightSource.intensity = 0f;
        lightSource.gameObject.SetActive(false);
        Debug.Log("Flashbang effect ended.");
    }


}
