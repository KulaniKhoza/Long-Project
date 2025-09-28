using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Data", menuName = "Assets/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public int harvestValue;
    public float growthTime;
    public GameObject cropPrefab;
    public Vector3 spawnOffset;
    
    
}
