using UnityEngine;

public class Collectible : MonoBehaviour
{
    // coin
    // is picked up by player
    // spawns at different areas and evently spaced from each other
    // random spawn?
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // PLAYER COLLECTS OBJECTS
    // despawn collectible on player collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<CollectibleManager>().UpdateCollectibleCountUI();
            Destroy(gameObject);
        }
    }
}
