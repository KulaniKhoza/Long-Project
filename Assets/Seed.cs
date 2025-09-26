using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour 

{
    public CropData cropData;
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Grow());
    }

    // Update is called once per frame
    void Update()
    {
        
    } 
    IEnumerator Grow()
    {
        yield return new WaitForSeconds(cropData.growthTime);
        GameObject newcrops = Instantiate(
            cropData.cropPrefab,
            transform.position + cropData.spawnOffset,
            Quaternion.identity
        );
        ;
        Destroy(this.gameObject);
    }
}
