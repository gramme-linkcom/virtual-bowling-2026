using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OKbutton : MonoBehaviour
{
    public GameObject inputGameObject;
    public Button button;
    //public bool OKButtonClick;
    public GameContoller gameController;
    public TMP_InputField nameText;
    // Start is called before the first frame update
    public void Onclick()
    {
        Debug.Log(nameText.text);
        //OKButtonClick = true;
        gameController.ResetGame();
    }

    // Update is called once per frame
    void Update()
    {
        
        //nameText.text = "oooo";
    }
    public void ResetText() 
    {
        nameText.text = "";
        Debug.Log("RESETTEXT");
    }
}
