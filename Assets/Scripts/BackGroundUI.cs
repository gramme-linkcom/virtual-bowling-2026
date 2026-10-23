using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundUI : MonoBehaviour
{
   public GameObject backgroundPanel; //背景パネルの参照

   public void ShowModal()
   {
    backgroundPanel.SetActive(true);
    //ここで背景パネルの透明度を設定
   }

   public void HideModal(){
    backgroundPanel.SetActive(false);
   }
}
