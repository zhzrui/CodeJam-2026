using NUnit.Framework;
using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    // coin
    // is picked up by player
    // spawns at different areas and evently spaced from each other
    // random spawn?

    [SerializeField] private GameObject coinPrefab;

    public int collectibleCount = 0;
    public TextMeshProUGUI collectibleText;
    // FOR LATER: ADD COLLECTED NOISE




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{

    //}

    // Update is called once per frame
    //void Update()
    //{

    //}

    // PLAYER COLLECTS OBJECTS
    // despawn collectible on player collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            collectibleCount++;
            UpdateCollectibleCountUI();
            Destroy(other.gameObject);
        }
    }
    
    // updating ui
    private void UpdateCollectibleCountUI()
    {
        if (collectibleText != null)
        {
            collectibleText.text = "Collectec: " + collectibleCount;
        }
    }

}
