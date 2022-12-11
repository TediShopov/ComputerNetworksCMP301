using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Restore : MonoBehaviour
{
    public GameObject FighterPrefab;
    public GameObject GameState;

    public InputFrame RollbackInput { get; set; }

    void ReplaceObject(ref GameObject toReplace, GameObject from)
    {

        Transform   RBTransform = from.GetComponent<Transform>();
       
        GameObject newObject;
        newObject = Instantiate(FighterPrefab);
        Debug.LogError("Instantiated  NEW Player Fighter");
        try
        {
            //Rollback the positiion of the actual player to RB dummy
            newObject.transform.position = RBTransform.position;
            newObject.transform.rotation = RBTransform.rotation;
            newObject.transform.parent = RBTransform.parent;
            bool isEnemy = from.GetComponent<FighterRBControlller>().isEnemy;
            newObject.GetComponent<FighterController>().isEnemy = isEnemy;
            newObject.GetComponent<FighterBufferMono>()
                .InputBuffer.SetTo(toReplace.GetComponent<FighterBufferMono>().InputBuffer);
            if (!isEnemy)
            {
                newObject.GetComponent<FighterBufferMono>().CollectInputFromKeyboard = true;

            }
            else
            {
                newObject.GetComponent<SpriteRenderer>().color = Color.red;
            }


            //Extract RB object Input Buffer and reenact it on the player object


            Destroy(toReplace);
            toReplace.SetActive(false);

            Debug.LogError("Destroyed Original Player Fighter Object");
            toReplace = newObject;
        }
        catch (System.Exception)
        {
            Destroy(toReplace);

            throw;
        }
        


        
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
    public int RollbackFrames { get; private set; }

    public FighterController EnemyFighterController { get; set; }

    private void Start()
    {
        var playerController= StaticBuffers.Instance.Player.GetComponent<FighterController>();
        RollbackAllowed = 8;
        EnemyFighterController =  StaticBuffers.Instance.Player.GetComponent<FighterController>();

        StaticBuffers.Instance.EnemyBuffer.OnInputFrameAdded+=
            (InputFrame f) => {
                RollbackInput = f;
                RollbackFrames = EnemyFighterController.TimeStampDifference; };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Rollback(8);
        }
        if (RollbackFrames<0)
        {
            Debug.LogError($"Received Buffer  rollback to {RollbackFrames}");
            if (FrameLimiter.Instance.FramesInPlay>this.RollbackAllowed)
            {

               // Rollback(-RollbackFrames);
                RollbackFrames = 0;

            }

        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ClientData.Pause = !ClientData.Pause;

        }
    }

    InputBuffer InsertRollbackInput(InputBuffer buffer, InputFrame rollbackFrame) 
    {
        if (rollbackFrame.TimeStamp>=buffer.LastFrame.TimeStamp)
        {
            //Input arrived for the input delay
            return buffer;
        }


        InputBuffer restructuredBuffer = new InputBuffer();
      
        int indexForNewInput = rollbackFrame.TimeStamp - buffer.BufferedInput.Peek().TimeStamp;
        //Check which index was rollback found 
        Debug.LogError($"Index for rollback {indexForNewInput}");
        for (int i = 0; i < buffer.BufferedInput.Count; i++)
        {
            if (i>=indexForNewInput)
            {
                //Insert The Rollback Input in the buffer
                //Also change the next oputput as predicted
                restructuredBuffer.BufferedInput.Enqueue(rollbackFrame);

            }
            else
            {
                restructuredBuffer.BufferedInput.Enqueue(buffer.BufferedInput.Dequeue());
            }


        }
        return restructuredBuffer;

        
    }





    public void Rollback(int frames) 
    {

        ReplaceObject(ref StaticBuffers.Instance.Player, StaticBuffers.Instance.PlayerRB);
        ReplaceObject(ref StaticBuffers.Instance.Enemy, StaticBuffers.Instance.EnemyRB);
        StaticBuffers.Instance.RenewBuffers();
        EnemyFighterController = StaticBuffers.Instance.Player.GetComponent<FighterController>();


        var playerBufferCopy = new InputBuffer();
        playerBufferCopy.SetTo(StaticBuffers.Instance.PlayerRB.GetComponent<FighterController>().InputBuffer);
        var enemyBufferCopy = new InputBuffer();
        enemyBufferCopy.SetTo(StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>().InputBuffer);

        //enemyBufferCopy = InsertRollbackInput(enemyBufferCopy,RollbackInput);

        //StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>().InputBuffer.SetTo(enemyBufferCopy);



        for (int i = 0; i < frames; i++)
        {
            
                ResimulateFramesForFighter(
                    StaticBuffers.Instance.Player.GetComponent<FighterController>()
                    , playerBufferCopy, 1);
                ResimulateFramesForFighter(
                   StaticBuffers.Instance.Enemy.GetComponent<FighterController>()
                   , enemyBufferCopy, 1);

        }
    }
}
