using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    [SerializeField]
    public Animator animator;



     
    public List<float> TimesForComboAdvance;

    private int _hitCount = 0;
    private bool _hitOnTime = true;
    private bool _inHitWindow;
    Coroutine openHitWindow;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // ??? If player is grounded and crouched ???

        //When player hits button start new attack

        

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (_hitCount==3)
            {
                return;
            }
            if (_hitCount == 0)
            {
                _hitCount++;
                //Time the window in which new key press progresses the combo
                openHitWindow= StartCoroutine(OpenHitWindow(TimesForComboAdvance[_hitCount-1]));

            }
            else if (_hitOnTime)
            {
                _hitCount++;
                if (openHitWindow!=null)
                {
                    StopCoroutine(openHitWindow);
                }
                openHitWindow = StartCoroutine(OpenHitWindow(TimesForComboAdvance[_hitCount-1]));

            }
        }

        if (!_hitOnTime)
        {
            _hitOnTime = false;
            _hitCount = 0;
        }


        UpdateAnimation(_hitCount, _hitOnTime);






    }

    private IEnumerator OpenHitWindow( float realTime)
    {
        _hitOnTime = true;
        Debug.Log("Hit Window Started:");
        Debug.Log(realTime);

        yield return new WaitForSeconds(realTime);
        Debug.LogWarning("Hit Window Expired");

        _hitOnTime = false;


    }



  

    void UpdateAnimation(int hCount, bool hit) 
    {
        animator.SetInteger("LHits", hCount);
        animator.SetBool("LHitOnTime", hit);

    }
}
