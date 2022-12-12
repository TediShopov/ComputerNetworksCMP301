using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restore : MonoBehaviour
{
    public GameObject FighterPrefab;
    public GameObject GameState;

    public InputFrame RollbackInput { get; set; }


    void ReplaceAnimationClip(Animator toReplace, Animator from) 
    {
        var animState= from.GetCurrentAnimatorStateInfo(0);

        toReplace.Play(animState.fullPathHash,0,animState.normalizedTime);
    }

    void ReplaceAnimatorParameters(Animator toReplace,Animator from) 
    {
        foreach (var param in from.parameters)
        {
            
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float: toReplace.SetFloat(param.name, from.GetFloat(param.name));

                    break;
                case AnimatorControllerParameterType.Int:
                    toReplace.SetInteger(param.name, from.GetInteger(param.name));
                    break;
                case AnimatorControllerParameterType.Bool:
                    toReplace.SetBool(param.name, from.GetBool(param.name));
                    break;
                case AnimatorControllerParameterType.Trigger:
                //    toReplace.SetTrigger(param.name);
                    break;
                default:
                    break;
            }
        }

      
    }

    void ReplaceObject(ref GameObject toReplace, GameObject from)
    {

        Transform   RBTransform = from.GetComponent<Transform>();
       
        GameObject newObject;
        newObject = Instantiate(FighterPrefab);
       
        //Debug.LogError("Instantiated  NEW Player Fighter");
        try
        {
            //Rollback the positiion of the actual player to RB dummy
            newObject.transform.position = RBTransform.position;
            newObject.transform.rotation = RBTransform.rotation;
            newObject.transform.parent = RBTransform.parent;
            if (RBTransform.localScale.x<0)
            {
                Debug.LogError("RB Dummy is flipped");
            }
            newObject.transform.localScale = RBTransform.localScale;


            //Changed RestoRE !!!!
            newObject.GetComponent<FighterController>().
                SetInnerStateTo(from.GetComponent<FighterRBControlller>());

            newObject.GetComponent<FighterBufferMono>()
                .InputBuffer.SetTo(toReplace.GetComponent<FighterBufferMono>().InputBuffer);



            Animator newObjectAnimator = newObject.GetComponent<Animator>();
            Animator fromAnimator = from.GetComponent<Animator>();

            bool crouchFrom = fromAnimator.GetBool("Crouch");
            bool crouchNew = newObjectAnimator.GetBool("Crouch");

            if (crouchFrom != crouchNew)
            {
                int a=3;
            }

       

            ReplaceAnimationClip(newObjectAnimator,fromAnimator);
            ReplaceAnimatorParameters(newObjectAnimator,fromAnimator);


             crouchFrom = fromAnimator.GetBool("Crouch");
             crouchNew = newObjectAnimator.GetBool("Crouch");
            bool isTrue = crouchFrom == crouchNew;

            HealthScript healthScript = newObject.GetComponent<HealthScript>();
            healthScript = from.GetComponent<HealthScript>();


            bool isEnemy = newObject.GetComponent<FighterController>().isEnemy;


            if (!isEnemy)
            {
                newObject.GetComponent<FighterBufferMono>().CollectInputFromKeyboard = true;

            }
            else
            {
                newObject.GetComponent<SpriteRenderer>().color = Color.red;
                newObject.GetComponent<FighterBufferMono>().InputBuffer.OnInputFrameAdded +=
                 (InputFrame f) => {
               RollbackInput = f;
               RollbackFrames = EnemyFighterController.TimeStampDifference;
           };
            }


            //Extract RB object Input Buffer and reenact it on the player object


            Destroy(toReplace);
            toReplace.SetActive(false);

           // Debug.LogError("Destroyed Original Player Fighter Object");
            toReplace = newObject;
        }
        catch (System.Exception)
        {
            Destroy(toReplace);

            throw;
        }
        


        
    }


    void ResimulateFramesForFighter(FighterController fighter, InputBuffer inputBuffer, int frames)
    {
        if (inputBuffer.BufferedInput.Count == 0 ) 
        {
            return;
        }
        //Debug.LogError($"Resimulating from{FrameLimiter.Instance.FramesInPlay} " +
        //    $" {inputBuffer.BufferedInput.Peek().TimeStamp} Frames");
     
      
        for (int i = 0; i < frames; i++)
        {
            fighter.ResimulateInput(
            inputBuffer,
            frames);
        }

    }

    public int RollbackAllowed { get;private set; }
    public int RollbackFrames { get; private set; }

    public FighterController EnemyFighterController { get; set; }


    public int PauseInterval = 5;
    private int _passedPausedFrames=0;
    private void Start()
    {
        var playerController= StaticBuffers.Instance.Player.GetComponent<FighterController>();
        RollbackAllowed = 7;
        EnemyFighterController =  StaticBuffers.Instance.Enemy.GetComponent<FighterController>();

        StaticBuffers.Instance.EnemyBuffer.OnInputFrameAdded+=
            (InputFrame f) => {
                RollbackInput = f;
                RollbackFrames = EnemyFighterController.TimeStampDifference;
                if (RollbackFrames<0)
                {
                    Debug.LogError($"Received Buffer  rollback to {RollbackFrames}");
                }
            };
    }

    // Update is called once per frame
    void Update()
    {
      
        if (Input.GetKey(KeyCode.B))
        {
            Rollback(7);
        }
        if (RollbackFrames<0)
        {
            Debug.LogError($"Received Buffer  rollback to {RollbackFrames}");
            if (FrameLimiter.Instance.FramesInPlay>this.RollbackAllowed)
            {

                Rollback(-RollbackFrames);
                RollbackFrames = 0;

            }

        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ClientData.Pause = !ClientData.Pause;

        }
        


    }

    InputBuffer InsertRollbackInput(InputBuffer buffer) 
    {
       
        InputBuffer restructuredBuffer = new InputBuffer();
        //Setup the proeprties
        restructuredBuffer.SetTo(buffer);
        //Clear the actual contents
        restructuredBuffer.Clear();
        InputFrame rollbackFrame = buffer.LastFrame;
        int indexForNewInput = rollbackFrame.TimeStamp - buffer.BufferedInput.Peek().TimeStamp;
        //Check which index was rollback found 
        // Debug.LogError($"Index for rollback {indexForNewInput}");

        for (int i = 0; i < buffer.DelayInput; i++)
        {
            if (i>= indexForNewInput)
            {
                int predictedFrameCount = (buffer.BufferedInput.Peek().TimeStamp + indexForNewInput);
                restructuredBuffer.Enqueue(new InputFrame(rollbackFrame._inputInFrame,
                    predictedFrameCount));
            }
            else
            {
                restructuredBuffer.Enqueue(buffer.BufferedInput.Dequeue());
            }
        }
        return restructuredBuffer;

        
    }





    public void Rollback(int frames) 
    {
        Debug.LogError($"Attemping {frames} Frames Rollback");
        ReplaceObject(ref StaticBuffers.Instance.Player, StaticBuffers.Instance.PlayerRB);
        ReplaceObject(ref StaticBuffers.Instance.Enemy, StaticBuffers.Instance.EnemyRB);
        StaticBuffers.Instance.RenewBuffers();
        EnemyFighterController = StaticBuffers.Instance.Enemy.GetComponent<FighterController>();


        var playerBufferCopy = new InputBuffer();
        playerBufferCopy.SetTo(StaticBuffers.Instance.PlayerRB.GetComponent<FighterController>().InputBuffer);
        
        InputBuffer enemyRBBuffer = StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>().InputBuffer;

        InputBuffer newRollbackBuffer = InsertRollbackInput(enemyRBBuffer);

        enemyRBBuffer.SetTo(newRollbackBuffer);

        for (int i = 0; i < 7; i++)
        {
            
                ResimulateFramesForFighter(
                    StaticBuffers.Instance.Player.GetComponent<FighterController>()
                    , playerBufferCopy, 1);
                ResimulateFramesForFighter(
                   StaticBuffers.Instance.Enemy.GetComponent<FighterController>()
                   , newRollbackBuffer, 1);

        }


    }
}
