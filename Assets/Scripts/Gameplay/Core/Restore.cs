using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restore : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public GameObject FighterPrefab;
    public GameObject GameState;
    public GameObject RBState;

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
        newObject = Instantiate(FighterPrefab,GameState.transform);
       
        //Debug.LogError("Instantiated  NEW Player Fighter");
        try
        {
            //Rollback the positiion of the actual player to RB dummy
            newObject.transform.position = RBTransform.position;
            newObject.transform.rotation = RBTransform.rotation;
            newObject.transform.parent = RBTransform.parent;
          
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
            //TODO add blocking state to restore
            ReplaceAnimationClip(newObjectAnimator,fromAnimator);
            ReplaceAnimatorParameters(newObjectAnimator,fromAnimator);

            HealthScript healthScript = newObject.GetComponent<HealthScript>();
            healthScript.SetValues(from.GetComponent<HealthScript>());


            AttackScript attackScript = newObject.GetComponent<AttackScript>();
            if (from.GetComponent<AttackScript>().IsHitting==true)
            {
                int a = 3;
            }
            attackScript.SetTo(from.GetComponent<AttackScript>());

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

            Rigidbody2D rigidbody2D=newObject.GetComponent<Rigidbody2D>();
            //Extract RB object Input Buffer and reenact it on the player object
            rigidbody2D.velocity = from.GetComponent<Rigidbody2D>().velocity;
            rigidbody2D.position = from.GetComponent<Rigidbody2D>().position;


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


    void SetSimulationState(StateProjectileManager RBProjectileManager,bool state) 
    {
        foreach (var proj in RBProjectileManager.ProjectilesInState)
        {
            proj.GetComponent<Rigidbody2D>().simulated = state;
        }

    }
    void ResimulateProjectiles(StateProjectileManager gameState,
        StateProjectileManager RBProjectileManager) 
    {
        foreach (var proj in gameState.ProjectilesInState)
        {
            DestroyImmediate(proj);
        }

        foreach (var proj in RBProjectileManager.ProjectilesInState)
        {
            GameObject projectileCopy = Instantiate(ProjectilePrefab, proj.transform.position,
                proj.transform.rotation);
            projectileCopy.GetComponent<Projectile>().AddToManager(gameState.gameObject);
            projectileCopy.GetComponent<Rigidbody2D>().velocity =
                proj.GetComponent<Rigidbody2D>().velocity;


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
            //Debug.LogError($"Received Buffer rollback to {RollbackFrames}");
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

    InputBuffer InsertRollbackInput(InputBuffer buffer, InputFrame rollbackFrame) 
    {
       
        InputBuffer restructuredBuffer = new InputBuffer();
        //Setup the proeprties
        restructuredBuffer.SetTo(buffer);
        //Clear the actual contents
        restructuredBuffer.BufferedInput.Clear();
        //int indexForNewInput = rollbackFrame.TimeStamp - buffer.BufferedInput.Peek().TimeStamp;
        //Check which index was rollback found 
        // Debug.LogError($"Index for rollback {indexForNewInput}");
      
        int indexForNewInput = rollbackFrame.TimeStamp - buffer.BufferedInput.Peek().TimeStamp;
        //Debug.LogError($"indexForNewInput : {indexForNewInput}");
        //Debug.LogError($"ToResomulate : { buffer.DelayInput  - indexForNewInput}");
        int nextInputTimestampOffset = 0;
        for (int i = 0; i < buffer.DelayInput; i++)
        {
            if (i>= indexForNewInput)
            {
                
                int predictedFrameCount = (nextInputTimestampOffset + rollbackFrame.TimeStamp);
                restructuredBuffer.Enqueue(new InputFrame(rollbackFrame.Inputs,
                    predictedFrameCount));
                nextInputTimestampOffset++;
            }
            else
            {
                restructuredBuffer.Enqueue(buffer.BufferedInput.Dequeue());
            }
        }
        return restructuredBuffer;

        
    }

    InputBuffer BufferToRollbackWith(InputBuffer RBBuffer, InputBuffer normalBuffer) 
    {
        InputBuffer rollbackBuffer = new InputBuffer();
        rollbackBuffer.SetTo(RBBuffer);

     
        foreach (var inputFrame in normalBuffer.BufferedInput)
        {
            rollbackBuffer.BufferedInput.Enqueue(inputFrame);
        }

        return rollbackBuffer;
        //The RBBuffer will be consumed on this rollback frames
        //However it will retain the pressed keys on on the target buffer;
    }



    public void Rollback(int frames) 
    {
        Debug.LogError($"{frames} Frames Rollback");

        ReplaceObject(ref StaticBuffers.Instance.Player, StaticBuffers.Instance.PlayerRB);
        ReplaceObject(ref StaticBuffers.Instance.Enemy, StaticBuffers.Instance.EnemyRB);
        StaticBuffers.Instance.RenewBuffers();
        EnemyFighterController = StaticBuffers.Instance.Enemy.GetComponent<FighterController>();


        //var playerBufferCopy = new InputBuffer();
        //playerBufferCopy.SetTo(StaticBuffers.Instance.PlayerRB.GetComponent<FighterController>().InputBuffer);
        //InputBuffer playerBuffer= StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>().InputBuffer;

         InputBuffer player =    StaticBuffers.Instance.Player.GetComponent<FighterController>().InputBuffer;
        InputBuffer playerRB =  StaticBuffers.Instance.PlayerRB.GetComponent<FighterController>().InputBuffer;
        InputBuffer enemy  =     StaticBuffers.Instance.Enemy.GetComponent<FighterController>().InputBuffer;
       


        player =
        BufferToRollbackWith(playerRB, player);

        InputBuffer newRollbackBuffer = InsertRollbackInput(StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>().InputBuffer, RollbackInput);
        StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>().InputBuffer.SetTo(newRollbackBuffer);

        newRollbackBuffer = BufferToRollbackWith(newRollbackBuffer, enemy);

        
        StaticBuffers.Instance.PlayerRB.GetComponent<Rigidbody2D>().simulated = false;
        StaticBuffers.Instance.EnemyRB.GetComponent<Rigidbody2D>().simulated = false;
       
        SetSimulationState(RBState.GetComponent<StateProjectileManager>(), false);
        ResimulateProjectiles(GameState.GetComponent<StateProjectileManager>(),
            RBState.GetComponent<StateProjectileManager>());

        

        for (int i = 0; i < 7; i++)
        {


            StaticBuffers.Instance.Player.GetComponent<FighterController>().ResimulateInput(player, 1);
            player.OnUpdate();
            StaticBuffers.Instance.Player.GetComponent<AttackScript>().OnUpdate();
            StaticBuffers.Instance.Player.GetComponent<SimulateAnimator>().ManualUpdateFrame();


            StaticBuffers.Instance.Enemy.GetComponent<FighterController>().ResimulateInput(
            newRollbackBuffer, 1);
            newRollbackBuffer.OnUpdate();
            StaticBuffers.Instance.Enemy.GetComponent<AttackScript>().OnUpdate();
            StaticBuffers.Instance.Enemy.GetComponent<SimulateAnimator>().ManualUpdateFrame();


            Physics2D.Simulate(0.016667f);
        }
        SetSimulationState(RBState.GetComponent<StateProjectileManager>(), true);
        StaticBuffers.Instance.PlayerRB.GetComponent<Rigidbody2D>().simulated = true;
        StaticBuffers.Instance.EnemyRB.GetComponent<Rigidbody2D>().simulated = true;

    }
}
