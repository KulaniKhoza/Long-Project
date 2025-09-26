using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;





public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update 
    public int Money = 100;
    
    public TextMeshProUGUI MoneyText;
    public int tomatoSeeds = 1;
    public int cornSeeds = 1;
    public int seeds = 1;
    public static GameManager Instance;
    public CropData cropData;
    public GameObject uiPrefab;
    public Transform parentCanvas;

    private bool level3MessageShown = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        Time.timeScale = 1.0f;


    }


    // Update is called once per frame
    void Update()
    {
        MoneyText.text = "R" + Money;

       
        //Harvest();
      
        
    }
    public void SpawnUIAboveField(Transform field, string textToShow)
    {
        if (uiPrefab == null || parentCanvas == null) return;

        // Convert world to screen space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(field.position + new Vector3(1.4f, 0.7f, 0));

        // Instantiate under Canvas
        GameObject newUIElement = Instantiate(uiPrefab, parentCanvas, false);

        // Force position in screen space
        newUIElement.transform.position = screenPos;

        // If prefab has TMP text, set it
        var tmp = newUIElement.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = textToShow;
            tmp.color = Color.red;
        }

        //  Force Unity to refresh UI immediately
        Canvas.ForceUpdateCanvases();
    }
    /* public void Harvest()
     {


         if (Input.GetMouseButtonDown(1))
         {
             if (PlayGrid.Instance != null && !PlayGrid.Instance.Sow && !PlayGrid.Instance.CreateField)
             {
                 var Mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                 Mousepos.z = 0;
                 var mousePos2D = new Vector2(Mousepos.x, Mousepos.y);

                 int layerMask = LayerMask.GetMask("CropsLayer"); // make sure fields are on this layer
                 RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero, Mathf.Infinity, layerMask);
                 if (hit2D.collider != null && hit2D.collider.CompareTag("Crops") && Money >= 0)
                 {
                     Crops crop = hit2D.collider.GetComponent<Crops>();
                     if (crop != null && crop.cropData != null)
                     {
                         Money += crop.cropData.harvestValue;

                         Destroy(hit2D.collider.gameObject);
                     }
                 }
             }
         }
     }
    */





}
