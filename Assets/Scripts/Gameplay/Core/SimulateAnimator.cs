using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateAnimator : MonoBehaviour
{
    Animator animator;
    private void Awake()
    {
        animator = this.gameObject.GetComponent<Animator>();
        
    }
    // Update is called once per frame

    public void ManualUpdateFrame() 
    {
        animator.Update(0.016667f);
    } 
    void Update()
    {
        if (!ClientData.Pause)
        {
            ManualUpdateFrame();
        }
    }
}
