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

    private bool dying = false;
    private bool castingFireball = false;
    private bool isGrounded = true;
    private bool isCrouched = false;
    private bool isBlocking = false;

    //private Vector3 horizontalMovement;

    private Rigidbody2D rigidbody2d;

    //[SerializeField]
    public InputBuffer InputBuffer { get; set; }
    private int LastFrameProcessed;

    //[SerializeField]
    private Animator animator;
    private AttackScript attack;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update

    
    public void SetInnerStateTo(FighterController fc) 
    {
        this.isCrouched = fc.isCrouched;
        this.isEnemy = fc.isEnemy;
        this.isGrounded = fc.isGrounded;
        this.dying = fc.dying;
        this.isFlipped = fc.isFlipped;
        this.castingFireball = fc.castingFireball;
    }

    private void Awake()
    {
        LastFrameProcessed = 0;

        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        attack = GetComponent<AttackScript>();

    }

public void ResimulateInput(InputBuffer inputBuffer,int frames) 
    {
        for (int i = 0; i < frames; i++)
        {
            OrientToEnemy(GetEnemy()?.transform);

            //Simulate the input buffer as the frame it was send in
            // this will always pass
            ProcessInputBuffer(inputBuffer, inputBuffer.BufferedInput.Peek().TimeStamp,false);
        }


       
    }

    public bool isEnemy;
    private GameObject GetEnemy() 
    {
        //If in rollback testing layer
        if (this.gameObject.layer==9)
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
      
        if (ClientData.Pause || this.dying)
        {
            return;
        }


        if (groundCollider.enabled==false)
        {
            groundCollider.enabled = true;
        }
        OrientToEnemy(GetEnemy()?.transform);
        if (FrameLimiter.Instance.FramesInPlay >=  InputBuffer.DelayInput)
        {
            ProcessInputBuffer(InputBuffer, GetGameFrame());
        }
        attack.OnUpdate();
    }


    bool isFlipped=false;
  
    void OrientToEnemy(Transform enemyTransform) 
    {
      
       bool isRight= (enemyTransform.position.x - this.transform.position.x)<0;

        if (isRight)
        {
            isFlipped = true;
            if (this.transform.localScale.x>0)
            {
                Vector3 theScale = transform.localScale;
                theScale.x *= -1;
                this.transform.localScale = theScale;
            }
           // spriteRenderer.flipX = true;
        }
        else
        {
            isFlipped = false;
            if (this.transform.localScale.x < 0)
            {
                Vector3 theScale = transform.localScale;
                theScale.x *= -1;
                this.transform.localScale = theScale;
            }
            //  spriteRenderer.flipX = false;
        }

    }
    public int OffsetGameFrame=0;

    public bool ExecuteOnlyOnOverflow;
    public int GetGameFrame() 
    {
        return FrameLimiter.Instance.FramesInPlay + OffsetGameFrame;
    }

    public Vector3 GetDirToEnemy() 
    {
        return Vector3.Normalize( GetEnemy().transform.position - this.transform.position);
    }


    
    public int TimeStampDifference=>
        InputBuffer.LastFrame.TimeStamp - GetGameFrame();
       
    InputFrame GetPredictedInput(InputFrame lastReceived) 
    {
        //Debug.LogError($"Predicted input from {lastReceived.TimeStamp}");

        return new InputFrame(lastReceived.Inputs,
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
                AttempCastFireball();
            }


        }

        //If combos/special attack is not found 
        //Mode for the RB to simulate delay
        if (ExecuteOnlyOnOverflow)
        {
            if (inputBuffer.IsOverflow)
            {
                InputFrame input = inputBuffer.Dequeue();
                int diff = GetGameFrame() - input.TimeStamp;
               // Debug.LogError($" RB Timestamp difference is {diff}");
                ProcessInputs(input);
                LastFrameProcessed = input.TimeStamp;
            }
          
        }
        else
        {
            InputFrame input;
            if (inputBuffer.BufferedInput.Count<=0)
            {
                InputFrame predictedFrame = inputBuffer.LastFrame;
                if (predictedFrame == null)
                {
                    predictedFrame = new InputFrame();
                    //predictedFrame.SetKey(KeyCode.K);
                    predictedFrame.TimeStamp = FrameLimiter.Instance.FramesInPlay;
                }
               
                inputBuffer.Enqueue(GetPredictedInput(predictedFrame));

            }
            input = inputBuffer.Dequeue();
            ProcessInputs(input);
            LastFrameProcessed = input.TimeStamp;
            

        }




    }

    bool CheckFireball(KeyCode[] inputElements) 
    {
        
        KeyCode[] consecKey = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D };
        if (isFlipped)
        {
            consecKey = new KeyCode[] { KeyCode.D, KeyCode.S, KeyCode.A };
        }

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

    void ProcessInputs(InputFrame inputs)
    {

        if (castingFireball)
        {
            return;
        }
        Vector3 horizontalMovement = new Vector3(0, 0, 0);

        if (inputs.IsKey(KeyCode.K) && isGrounded)
        {
            this.SetBlocking(true);
            animator.SetFloat("XSpeed", horizontalMovement.x);
            return;
        }
        SetBlocking(false);


        attack.ProcessInput(inputs);
        if (attack.IsHitting)
        {
            return;
        }


        if (isGrounded)
        {
            SetCrouch(inputs.IsKey(KeyCode.S));
            if (inputs.IsKey(KeyCode.Space))
            {
                Jump();
            }
        }

        if (!isCrouched)
        {
            if (inputs.IsKey(KeyCode.D))
            {
                horizontalMovement.x = 1;
            }
            if (inputs.IsKey(KeyCode.A))
            {
                horizontalMovement.x = -1;
            }
        }

        //Move in the direction of the player input
        rigidbody2d.transform.position += horizontalMovement /** Time.deltaTime*/ * moveSpeed * 0.01f;
       
        if (isFlipped)
        {
            horizontalMovement.x = -horizontalMovement.x;
        }

        animator.SetFloat("XSpeed", horizontalMovement.x);

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.LogWarning("Player Landed");
        isGrounded = true;
        animator.SetBool("IsGrounded", true);
        animator.SetBool("Jumped", false);
    }


    public void AttempCastFireball() 
    {
        animator.SetTrigger("CastFireball");
    }
    public void SetCastingFireball(bool b)
    {
        castingFireball = b;
        animator.SetBool("CastingFireball", b);
        if (isCrouched)
        {
            SetCrouch(false);
        }

    }

    public bool GetCastingFireball()
    {
        return castingFireball;
    }


    public void setDamaged( bool isLow) 
    {
        if (isLow)
        {
            animator.SetTrigger("LowDamage");
        }
        else
        {
            animator.SetTrigger("HighDamage");
        }
    }
    public bool GetBlocking()
    {
        return this.isBlocking;
    }
    public void SetBlocking(bool b) 
    {
        this.isBlocking = b;
        animator.SetBool("IsBlocking", b);
    }
    public void SetDying(bool b) 
    {
        this.dying = b;
        animator.SetBool("IsDead", true);
        animator.SetTrigger("Dying");

    }
    public void SetCrouch( bool b) 
    {
        isCrouched = b;
        animator.SetBool("Crouch", b);

    }
    void Jump()
    
    {
        groundCollider.enabled = false;
        isGrounded = false;
        animator.SetBool("IsGrounded", false);
        
        rigidbody2d.AddForce(new Vector2(0f, jumpPower), ForceMode2D.Impulse);
        animator.SetBool("Jumped", true);
       


    }

}
