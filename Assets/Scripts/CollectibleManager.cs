using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CollectibleManager : MonoBehaviour
{
    // coin
    // is picked up by player
    // spawns at different areas and evently spaced from each other
    // random spawn?
    [Header("Collectible Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int maxCollectibles = 10;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Spin iconSpin;

    // private spawn points list to keep track of which spawn points have been used, 
    // might want to pass this over to the enemy patrol points
    private List<Transform> spawnPointsList;

    public int collectibleCount = 0;

    public int remainingCount;

    public TextMeshProUGUI collectibleText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPointsList = new List<Transform>(spawnPoints);
        remainingCount = maxCollectibles;
        UpdateCollectibleCountUI();
        for (int i = 0; i < maxCollectibles; i++)
        {
            SpawnCollectible();
        }
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    private void SpawnCollectible()
    {
        if (collectibleCount < maxCollectibles)
        {
            int randomIndex = Random.Range(0, spawnPointsList.Count);
            Transform spawnPoint = spawnPointsList[randomIndex];
            spawnPointsList.RemoveAt(randomIndex);

            Instantiate(coinPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
    
    // updating ui

    public void OnCollected() {
        collectibleCount++;
        remainingCount--;
        UpdateCollectibleCountUI();
        
        if (iconSpin != null) iconSpin.PlayFastSpins();
    }

    public void UpdateCollectibleCountUI()
    {
        if (collectibleText != null)
        {
            collectibleText.text = "Left: " + remainingCount;
        }
    }

}
