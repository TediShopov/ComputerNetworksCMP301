using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restore : MonoBehaviour
{
    public GameObject fighter;
    public InputBuffer fighterRB;
    public Transform fighterRBTransform;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ClientData.Pause = true;


            //Position Update ??? 

            //Executed Saved input buffer to reach the new position and animation

        }
    }
}
