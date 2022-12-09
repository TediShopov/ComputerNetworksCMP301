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
        newObject.GetComponent<InputBuffer>().SetTo(toReplace.GetComponent<InputBuffer>());
        
           
        //Extract RB object Input Buffer and reenact it on the player object


        Destroy(toReplace);
        Debug.LogError("Destroyed Original Player Fighter Object");
        toReplace = newObject;


        
    }

    //void ResimulateFrames(GameObject fighterObj, In, int frames) 
    //{
    //    var inputBufferCopy = new InputBuffer();
    //    inputBufferCopy.SetTo(RBObject.GetComponent<InputBuffer>());
    //    ResimulateFramesForFighter(
    //              fighterObj.GetComponent<FighterController>(),
    //            inputBufferCopy, frames);
    //}

    void ResimulateFramesForFighter(FighterController fighter, InputBuffer inputBuffer, int frames)
    {
        if (inputBuffer.BufferedInput.Count == 0 ) 
        {
            return;
        }
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

    public int RollbackAllowed { get;private set; }
    public int InputRollback { get; private set; }

    public FighterController EnemyFighterController { get; set; }

    private void Start()
    {
        var playerController= StaticBuffers.Instance.Player.GetComponent<FighterController>();

        EnemyFighterController =  StaticBuffers.Instance.Player.GetComponent<FighterController>();

        StaticBuffers.Instance.EnemyBuffer.OnInputFrameAdded+=
            (InputFrame f) => { InputRollback = EnemyFighterController.TimeStampDifference; };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Rollback(8);
        }
        if (InputRollback<0)
        {
            //Debug.LogError($"Received Buffer  rollback to {InputRollback}");
            //Rollback(-InputRollback);
            InputRollback = 0;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ClientData.Pause = !ClientData.Pause;

        }
    }

    public void Rollback(int frames) 
    {
        ReplaceObject(ref StaticBuffers.Instance.Player, StaticBuffers.Instance.PlayerRB);
        ReplaceObject(ref StaticBuffers.Instance.Enemy, StaticBuffers.Instance.EnemyRB);
        StaticBuffers.Instance.RenewBuffers();
        EnemyFighterController = StaticBuffers.Instance.Player.GetComponent<FighterController>();


        var playerBufferCopy = new InputBuffer();
        playerBufferCopy.SetTo(StaticBuffers.Instance.PlayerRB.GetComponent<InputBuffer>());
        var enemyBufferCopy = new InputBuffer();
        enemyBufferCopy.SetTo(StaticBuffers.Instance.EnemyRB.GetComponent<InputBuffer>());


   

        for (int i = 0; i < frames; i++)
        {
            ResimulateFramesForFighter(
                StaticBuffers.Instance.Player.GetComponent<FighterController>()
                ,playerBufferCopy, 1);
            ResimulateFramesForFighter(
               StaticBuffers.Instance.Enemy.GetComponent<FighterController>()
               , enemyBufferCopy, 1);

        }
    }
}
