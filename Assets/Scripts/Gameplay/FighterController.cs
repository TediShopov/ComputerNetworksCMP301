using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterController : MonoBehaviour
{

    //public FighterController enemy;

    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float jumpPower = 10f;

    public Projectile projectilePrefab;
    public Transform projectileFirePoint;


    public float waitAfterJump;
    public BoxCollider2D groundCollider;
    //public GameObject enemy;


    private bool isGrounded = true;
    private bool isCrouched = false;
    //private Vector3 horizontalMovement;
     
    private Rigidbody2D rigidbody2d;

    //[SerializeField]
    public InputBuffer InputBuffer { get; set; }
    private int LastFrameProcessed;

    //[SerializeField]
    private Animator animator;
    //[SerializeField]
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update

    //public delegate void InputFrameDelegate(InputFrame inputFrame);  // delegate
    //public event InputFrameDelegate OnInputProcessed; // event


    private void Awake()
    {
        LastFrameProcessed = 0;

        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

public void ResimulateInput(InputBuffer inputBuffer,int frames) 
    {
        for (int i = 0; i < frames; i++)
        {
            OrientToEnemy(GetEnemy()?.transform);

            //Simulate the input buffer as the frame it was send in
            // this will always pass
            ProcessInputBuffer(inputBuffer, inputBuffer.BufferedInput.Peek().TimeStamp,true);
        }


       
    }

    public bool isEnemy;
    private GameObject GetEnemy() 
    {
        //If in rollback testing layer
        if (this.gameObject.layer!=9)
        {
            if (!isEnemy)
            {
                return StaticBuffers.Instance.EnemyRB;
            }
            else
            {
                return StaticBuffers.Instance.PlayerRB;
            }
        }
        else
        {
            if (!isEnemy)
            {
                return StaticBuffers.Instance.Enemy;
            }
            else
            {
                return StaticBuffers.Instance.Player;
            }
        }
      
    }


    // Update is called once per frame
     public virtual void Update()
    {
      
        if (ClientData.Pause)
        {
            return;
        }

        OrientToEnemy(GetEnemy()?.transform);
        if (FrameLimiter.Instance.FramesInPlay >=  InputBuffer.DelayInput)
        {
            ProcessInputBuffer(InputBuffer, GetGameFrame());
        }

    }



  
    void OrientToEnemy(Transform enemyTransform) 
    {
        Vector2 enemyPos;
        enemyPos.x = enemyTransform.position.x; enemyPos.y = enemyTransform.position.y;
        Vector2 dir = enemyPos - rigidbody2d.position;
        if (dir.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
    public int OffsetGameFrame=0;

    public bool ExecuteOnlyOnOverflow;
    public int GetGameFrame() 
    {
        return FrameLimiter.Instance.FramesInPlay + OffsetGameFrame;
    }

    public int TimeStampDifference=>
        InputBuffer.LastFrame.TimeStamp - GetGameFrame();
       
    InputFrame GetPredictedInput(InputFrame lastReceived) 
    {
        //Debug.LogError($"Predicted input from {lastReceived.TimeStamp}");

        return new InputFrame(lastReceived._inputInFrame,
                  FrameLimiter.Instance.FramesInPlay);
    }
    void ProcessInputBuffer(InputBuffer inputBuffer,int frameToSimulate=0,bool isSilent = false ) 
    {
        if (inputBuffer == null)
        {
            return;
        }

 

        //var keyDownBuff = this.InputBuffer.GetKeyDownBuffer();
        if (inputBuffer.PressedKeys!=null && inputBuffer.PressedKeys.Count!=0)
        {

            if (CheckFireball(inputBuffer.PressedKeys.ToArray()))
            {
               Debug.LogError("Fireball Input Detected");
                InputBuffer.PressedKeys.Clear();
                Instantiate(projectilePrefab, projectileFirePoint.position, projectileFirePoint.rotation);
            }
        }


        //If combos/special attack is not found 
        //Todo this will not aacount for multiple pressed keys at once

        //Mode for the RB to simulate delay
        if (ExecuteOnlyOnOverflow)
        {
            if (inputBuffer.IsOverflow)
            {
                InputFrame input = inputBuffer.Dequeue();
                int diff = GetGameFrame() - input.TimeStamp;
               // Debug.LogError($" RB Timestamp difference is {diff}");
                ProcessInputs(input._inputInFrame);
                LastFrameProcessed = input.TimeStamp;
            }
          
        }
        else
        {

            InputFrame input;
            
            if (inputBuffer.BufferedInput.Count<=0)
                {
              
                    inputBuffer.Enqueue(GetPredictedInput(inputBuffer.LastFrame));
              
                }
              
              
                if (isSilent)
                {
                     input = inputBuffer.BufferedInput.Dequeue();
                }
                else
                {
                    input = inputBuffer.Dequeue();
                }

                ProcessInputs(input._inputInFrame);
                LastFrameProcessed = input.TimeStamp;
            

        }




    }

    bool CheckFireball(KeyCode[] inputElements) 
    {
        
        KeyCode[] consecKey = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D };
        bool fireballDone = true;

        if (inputElements.Length < 3)
        {
            return false;
        }

        //EX Press keys Buffer D D A D A
        // [0-2]

        for (int index = 0; index <= inputElements.Length - 3; index++)
        {
            fireballDone = true;
            for (int i = 0; i <= 2; i++)
            {
                if (inputElements[index + i] != consecKey[i])
                {
                    fireballDone = false;
                    break;
                }

            }
            if (fireballDone)
            {
                return true;
            }
        }

        return false;
    }

     bool IsKey(KeyCode keyCode,InputElement[] inputInFrame) 
    {
        foreach (var inputElement in inputInFrame)
        {
            if (inputElement.key==keyCode)
            {
                return true;
            }
        }
        return false;
    }
   

    void ProcessInputs(InputElement[] inputs)
    {
        Vector3 horizontalMovement = new Vector3(0, 0, 0);

        if (isGrounded)
        {
            setCrouch(IsKey(KeyCode.S, inputs));
            if (IsKey(KeyCode.Space, inputs))
            {
                Jump();
            }
        }

        if (!isCrouched)
        {
            if (IsKey(KeyCode.D, inputs))
            {
                horizontalMovement.x = 1;
            }
            if (IsKey(KeyCode.A, inputs))
            {
                horizontalMovement.x = -1;
            }
        }

        //Move in the direction of the player input
        rigidbody2d.transform.position += horizontalMovement /** Time.deltaTime*/ * moveSpeed * 0.01f;
        //However the animation should be fliped if needed
        if (spriteRenderer.flipX)
        {
            horizontalMovement.x = -horizontalMovement.x;
        }

       animator.SetFloat("XSpeed", horizontalMovement.x);

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.LogWarning("Player Landed");
        isGrounded = true;
        animator.SetBool("Jumped", false);
    }


    void setCrouch( bool b) 
    {
        isCrouched = b;
        animator.SetBool("Crouch", b);

    }
    void Jump()
    {
        isGrounded = false;
        rigidbody2d.AddForce(new Vector2(0f, jumpPower), ForceMode2D.Impulse);
        animator.SetBool("Jumped", true);

    }
   
}
