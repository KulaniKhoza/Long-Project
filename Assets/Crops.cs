using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crops : MonoBehaviour
{
    // Start is called before the first frame update 
    public GameManager MoneyManager;
    public CropData cropData;
    void LateUpdate()
    {



        if (Input.GetMouseButtonDown(1))
        {
            if (FarmGrid.Instance.Sow == false )
            {
                var Mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var mousePosXRounded = Mathf.Round(Mousepos.x);
                var mousePosYRounded = Mathf.Round(Mousepos.y);
                var mousePosRedifined = new Vector3(Mousepos.x, Mousepos.y, 6);
                var mousePos2D = new Vector2(mousePosRedifined.x, mousePosRedifined.y);

                int layerMask = LayerMask.GetMask("CropsLayer"); // make sure fields are on this layer
                RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero, Mathf.Infinity, layerMask);
                if (hit2D.collider != null && hit2D.collider.CompareTag("Crops") && GameManager.Instance.Money >= 0)
                {
                    var hitPosition2D = hit2D.collider.transform.position;
                    MoneyManager.Money += cropData.harvestValue;
                    Destroy(this.gameObject);

                }
            }
        }

    } 
   
}
