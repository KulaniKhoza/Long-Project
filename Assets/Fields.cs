using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Fields : MonoBehaviour
{/*
    // Start is called before the first frame update  
    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (PlayGrid.Instance.Sow == false && PlayGrid.Instance.CreateField == false)
            {
                var Mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var mousePosXRounded = Mathf.Round(Mousepos.x);
                var mousePosYRounded = Mathf.Round(Mousepos.y);
                var mousePosRedifined = new Vector3(Mousepos.x, Mousepos.y, 6);
                var mousePos2D = new Vector2(mousePosRedifined.x, mousePosRedifined.y);

                RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit2D.collider.CompareTag("field"))
                {
                    var hitPosition2D = hit2D.collider.transform.position;
                    MoneyManager.Instance.Money += 15;
                    Destroy(this.gameObject);

                }
            }

        }


    }
*/
   

   

}
