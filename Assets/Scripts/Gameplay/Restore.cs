using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Restore : MonoBehaviour
{
    public GameObject FighterPrefab;
    public GameObject GameState;

  

    void ReplaceObject(ref GameObject toReplace, GameObject from)
    {

        Transform   RBTransform = from.GetComponent<Transform>();
       
        GameObject newObject;
        newObject = Instantiate(FighterPrefab);
        Debug.LogError("Instantiated  NEW Player Fighter");
        
        //Rollback the positiion of the actual player to RB dummy
        newObject.transform.position = RBTransform.position;
        newObject.transform.rotation = RBTransform.rotation;
        newObject.transform.parent = RBTransform.parent;
        newObject.GetComponent<FighterController>().isEnemy = from.GetComponent<FighterController>().isEnemy;

        //Extract RB object Input Buffer and reenact it on the player object
        

        Destroy(toReplace);
        Debug.LogError("Destroyed Original Player Fighter Object");
        toReplace = newObject;


        
    }

    void ResimulateFrames(GameObject fighterObj, GameObject RBObject) 
    {
        ResimulateFramesForFighter(
                  fighterObj.GetComponent<FighterController>(),
                 RBObject.GetComponent<InputBuffer>(), 7);
    }

    void ResimulateFramesForFighter(FighterController fighter, InputBuffer inputBuffer, int frames)
    {
        Debug.LogError($"Resimulating from{FrameLimiter.Instance.FramesInPlay} " +
            $" {inputBuffer.BufferedInput.Peek().TimeStamp} Frames");
        fighter.ResimulateInput(
            inputBuffer,
            frames);


        for (int i = 0; i < frames; i++)
        {
            inputBuffer.BufferedInput.Dequeue();
        }

    }

    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (StaticBuffers.Instance.Player != null)
            {
                ClientData.Pause = true;
                ReplaceObject( ref StaticBuffers.Instance.Player, StaticBuffers.Instance.PlayerRB);

                StaticBuffers.Instance.RenewBuffers();
                ResimulateFrames(
                    StaticBuffers.Instance.Player,
                    StaticBuffers.Instance.PlayerRB);


            }
        }
        if (Input.GetKeyDown(KeyCode.P) && ClientData.Pause)
        {
            ClientData.Pause = false;

        }
    }
}
