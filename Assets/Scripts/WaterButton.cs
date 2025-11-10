using UnityEngine;
using UnityEngine.UI;

public class WaterCropButton : MonoBehaviour
{
    private Button button;

    public GameObject WaterPanel;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnWaterButtonClicked);
    }

    void Update()
    {
        // Update button interactability based on whether there's a selected crop that can be watered
        button.interactable = Crops.CanWaterAnyCrop();
    }

    public void OnWaterButtonClicked()
    {
        Crops.WaterSelectedCrop();
        WaterPanel.SetActive(false);
    }
}