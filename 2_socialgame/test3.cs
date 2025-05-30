using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kadai1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // キーボード操作に追従して、カーソルが移動する.
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int currentCursorIndex = JJC_ShopUI.cursor.GetCurrentIndex();
            Debug.Log (currentCursorIndex);
            int newCursorIndex = currentCursorIndex - 1;
            if (newCursorIndex < 0)
            {
                newCursorIndex = 5;
            }
            JJC_ShopUI.cursor.SetCurrentIndex(newCursorIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int currentCursorIndex = JJC_ShopUI.cursor.GetCurrentIndex();
            int newCursorIndex = currentCursorIndex + 1;
            JJC_ShopUI.cursor.SetCurrentIndex(newCursorIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            int currentCursorIndex = JJC_ShopUI.cursor.GetCurrentIndex();
            ItemFixData itemData = JJC_ShopUI.itemFixDataManager.GetFixData(currentCursorIndex);
            Debug.Log(itemData._name);
        }
    }
}
