using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class Flashbang : MonoBehaviour
{
    [Header("Flashbang Settings")]
    [SerializeField] private Light lightSource;
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private float flashIntensity = 500000f;
    [SerializeField] private float flashCooldown = 5f;
    [SerializeField] private Animator animator;
    private float lastFlashTime;

    [Header("UI Cooldown Display")]
    [SerializeField] private Image cooldownBar; // <-- 2. Drag CooldownBar_Fill here

    private EnemyChase enemyChaseScript;

    void Start()
    {
        enemyChaseScript = FindFirstObjectByType<EnemyChase>();

        // Ensures the flashbang is ready immediately when the game starts
        lastFlashTime = -flashCooldown;
    }

    void Update()
    {
        float timeSinceLastFlash = Time.time - lastFlashTime;
        float cooldownProgress = Mathf.Clamp01(timeSinceLastFlash / flashCooldown);

        if (cooldownBar != null)
        {
            // Fills from 0 to 1 as it recharges (use 1f - cooldownProgress to drain instead)
            cooldownBar.fillAmount = cooldownProgress;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && Time.time - lastFlashTime >= flashCooldown)
        {
            lastFlashTime = Time.time;
            
            AudioManager.Instance.PlaySFX(AudioManager.Instance.cameraSFX);
            
            // camera goes close to the screen
            animator.Play("Picture Time", 0, 0f); 
            
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
        
        // Wait one frame
        yield return null;

        float elapsedTime = 0f;

        while (elapsedTime < flashDuration) 
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / flashDuration);
            
            // Exponential curve drops from 500 000 to small value quickly, then fade slower
            float curve = Mathf.Pow(1f - t, 4f); 
            lightSource.intensity = flashIntensity * curve;
            
            yield return null;
        }


        lightSource.intensity = 0f;
        lightSource.gameObject.SetActive(false);
        Debug.Log("Flashbang effect ended.");
    }


}
