using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AttackScript : MonoBehaviour
{
    private Animator animator;
    private FighterController fighter; 

    public bool IsHitting { get; set; }
     
    public List<float> TimesForComboAdvance;

    private int _hitCount = 0;
    private bool _hitOnTime = true;
    //Coroutine openHitWindow;
    // Start is called before the first frame update
    [SerializeField]
    public GameObject[] HurtBoxes;


    private void Awake()
    {
        animator = this.GetComponent<Animator>();
        fighter = this.GetComponent<FighterController>();
    }
  

    bool waitForRelease = false;

    public void SetTo(AttackScript attackScript) 
    {
        //this.openHitWindow = attackScript.openHitWindow;
        this._hitCount = attackScript._hitCount;
        this._hitOnTime = attackScript._hitOnTime;


        _hitWindow = attackScript._hitWindow;
        _hitTimePassed = attackScript._hitTimePassed;
        //UpdateAnimation(_hitCount, _hitOnTime);
        waitForRelease = attackScript.waitForRelease;
        this.IsHitting = attackScript.IsHitting;

        for (int i = 0; i < attackScript.HurtBoxes.Length; i++)
        {
          
            this.HurtBoxes[i].SetActive(attackScript.HurtBoxes[i].activeSelf);
        }
    }
    public static int Activated = 0;
    public void ActiveHurtBox() 
    {
        Activated++;
        this.HurtBoxes[_hitCount - 1].SetActive(true);
    }

    public void ProcessInput(InputFrame inputFrame) 
    {
      
        bool keyPressed = inputFrame.IsKey(KeyCode.J);
        if (waitForRelease)
        {
            waitForRelease = keyPressed;
            return;
        }

        if (keyPressed)
        {
            waitForRelease = true;
            if (_hitCount == 3)
            {
                ActiveHurtBox();
                return;
            }
            if (_hitCount == 0)
            {

                _hitCount++;
                _hitOnTime = true;
                //Time the window in which new key press progresses the combo
                ActiveHurtBox();
                StartHitWindow(TimesForComboAdvance[_hitCount - 1]);

                //openHitWindow = StartCoroutine(OpenHitWindow(TimesForComboAdvance[_hitCount - 1]));

            }
            else if (_hitOnTime)
            {
                _hitCount++;
                ActiveHurtBox();
                StartHitWindow(TimesForComboAdvance[_hitCount - 1]);
                //if (openHitWindow != null)
                //{
                //    StopCoroutine(openHitWindow);
                //}
                //openHitWindow = StartCoroutine(OpenHitWindow(TimesForComboAdvance[_hitCount - 1]));

            }
        }

        //if (!_hitOnTime)
        //{
        //    _hitOnTime = false;
        //    _hitCount = 0;
        //}


        //UpdateAnimation(_hitCount, _hitOnTime);
    }

    //private void Update()
    //{
    //    if (!_hitOnTime)
    //    {
    //        _hitOnTime = false;
    //        _hitCount = 0;
    //    }

    //    UpdateAnimation(_hitCount, _hitOnTime);
    //}

    public float _hitWindow = 0;
    public float _hitTimePassed = 0;

    void StartHitWindow(float windowTime) 
    {
        _hitWindow=windowTime;
        _hitTimePassed = 0;
    }


    void ProgressHitWindowTimer(float delta) 
    {
        _hitTimePassed += delta;
        if (_hitTimePassed>_hitWindow)
        {
            _hitOnTime = false;
        }
        
    }
   

    public void OnUpdate(float delta= 0.01666666667f) 
    {
        ProgressHitWindowTimer(delta);
        if (!_hitOnTime)
        {
            _hitOnTime = false;
            _hitCount = 0;
        }

        UpdateAnimation(_hitCount, _hitOnTime);
    }

    //private IEnumerator OpenHitWindow( float realTime)
    //{
    //    _hitOnTime = true;
    //    Debug.Log("Hit Window Started:");
    //    Debug.Log(realTime);

    //    yield return new WaitForSeconds(realTime);
    //    Debug.LogWarning("Hit Window Expired");

    //    _hitOnTime = false;


    //}



  

    void UpdateAnimation(int hCount, bool hit) 
    {
        animator.SetInteger("LHits", hCount);
        animator.SetBool("LHitOnTime", hit);
    }
}
