using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    public CropData cropData;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        // Get the SpriteRenderer component
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(Grow());
    }

    IEnumerator Grow()
    {
        // Optional: You could add seedling growth stages here before spawning the main crop
        yield return new WaitForSeconds(cropData.growthTime);

        // Instantiate the crop
        GameObject newCrop = Instantiate(
            cropData.cropPrefab,
            transform.position + cropData.spawnOffset,
            Quaternion.identity
        );

        // Get the Crops component and set up its data
        Crops cropComponent = newCrop.GetComponent<Crops>();
        if (cropComponent != null)
        {
            cropComponent.cropData = this.cropData;
        }

        Destroy(this.gameObject);
    }
}