using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
     
public class MousePos : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
     
    public GameObject somethingToTest;
     
    private void Start()
    {
        var tmpScreenPos = Camera.main.WorldToScreenPoint(somethingToTest.transform.position);

        SetCursorPos((int)tmpScreenPos.x + 175, (int)tmpScreenPos.y + 140);
        
    }
    
    void Update()

    {

        

    }
}
