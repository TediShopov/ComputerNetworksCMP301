using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterController : MonoBehaviour
{

    //public FighterController enemy;

    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float jumpPower = 10f;


    public float waitAfterJump;
    public BoxCollider2D groundCollider;
    public GameObject enemy;


    private bool isGrounded = true;
    private bool isCrouched = false;
    private Vector3 horizontalMovement;

    private Rigidbody2D rigidbody2d;
    public InputBuffer InputBuffer;
    private int _lastProcessedTs;
    private HashSet<KeyCode> _keysPressedThisFrame;

    [SerializeField]
    Animator animator;
    [SerializeField]
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _lastProcessedTs = 0;
      
        rigidbody2d = GetComponent<Rigidbody2D>();
        _keysPressedThisFrame = new HashSet<KeyCode>();
        Debug.LogError($"InputBuffer {this.GetInstanceID()} is an active object ");
        InputBuffer = GetComponent<InputBuffer>();
        //StartCoroutine(RefreshTimestamp());
    }

    // Update is called once per frame
    void Update()
    {
        if (ClientData.IsPaused)
        {
            return;
        }
        horizontalMovement = new Vector3(0, 0, 0);

        _keysPressedThisFrame.Clear();
        OrientToEnemy(enemy.transform);
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _lastProcessedTs=0;
        }
       
        ProcessInputBuffer(InputBuffer.GetFirstFrame());
       
        animator.SetFloat("XSpeed", horizontalMovement.x);
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
    int GetGameFrame() 
    {
        return FrameLimiter.Instance.FramesInPlay + OffsetGameFrame;
    }
    void ProcessInputBuffer(InputFrame inputFrame) 
    {
      
        //var keyDownBuff = this.InputBuffer.GetKeyDownBuffer();

        //if (CheckFireball(keyDownBuff.ToArray()))
        //{
        //    Debug.LogWarning("Fireball Input Detected");
        //}

        //If combos/special attack is not found 
        //Todo this will not aacount for multiple pressed keys at once
        if (inputFrame==null)
        {
            return;
        }

        if (OffsetGameFrame!=0)
        {
            int mismtach = inputFrame.TimeStamp - GetGameFrame();
            Debug.LogError($"Mismatch Between Current Frame and timestamp = {mismtach}");
        }
       
        if (inputFrame.TimeStamp == GetGameFrame())
        {
            foreach (var el in inputFrame._inputInFrame)
            {
                if (!_keysPressedThisFrame.Contains(el.key))
                {
                    _keysPressedThisFrame.Add(el.key);
                    ProcessAsSingleInput(el);
                }
            }
            _lastProcessedTs = inputFrame.TimeStamp;
        }

    }

    bool CheckFireball(InputElement[] inputElements) 
    {
        
        KeyCode[] consecKey = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D };
        bool fireballDone = true;


        for (int index = 0; index < inputElements.Length-3; index++)
        {
            fireballDone = true;
            for (int i = 0; i < 2; i++)
            {
                if (inputElements[index + i].key != consecKey[index + i])
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

     bool IsKey(InputElement el, KeyCode key) { return el.key == key; }
    void ProcessAsSingleInput(InputElement e) 
    {
       
        if (isGrounded)
        {
            setCrouch(IsKey(e,KeyCode.S));
            if (IsKey(e, KeyCode.Space))
            {
                Jump();
            }
        }



        if (!isCrouched)
        {
            //TODO refacttor to input
            if (IsKey(e, KeyCode.D))
            {
                horizontalMovement.x = 1;
                // renderer.sprite = MoveForward;
            }
            if (IsKey(e, KeyCode.A))
            {
                horizontalMovement.x = -1;
                //  renderer.sprite = MoveBackward;
            }
        }

        //Move in the direction of the player input
        rigidbody2d.transform.position += horizontalMovement * Time.deltaTime * moveSpeed;
        //However the animation should be fliped if needed
        if (spriteRenderer.flipX)
        {
            horizontalMovement.x = -horizontalMovement.x;
        }


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
