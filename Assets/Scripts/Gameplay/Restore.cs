using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restore : MonoBehaviour
{

    public GameObject GameState;

    public GameObject RestoreFrom; //RB
    public GameObject RestoreTo;    //Actual Fighter
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {

            if (RestoreTo!=null)
            {
              
               //Animator animDst= RestoreTo.GetComponent<Animator>();

               // animDst = Instantiate(RestoreFrom.GetComponentInChildren<Animator>(),RestoreTo.transform);


                Rigidbody2D rgbDst= RestoreTo.GetComponent<Rigidbody2D>();
                
                rgbDst = Instantiate(RestoreFrom.GetComponent<Rigidbody2D>());

                

            }
            Debug.LogError("Created copy of Player Character");
           // RestoreTo = Instantiate(RestoreFrom, RestoreFrom.transform, GameState);
            //Position Update ??? 


            //Executed Saved input buffer to reach the new position and animation

        }
    }
}
