using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterBufferMono : MonoBehaviour
{
    //TODO make buffer only accpets keys that do sth in the game
    [SerializeField]
    public bool CollectInputFromKeyboard;
    [SerializeField]
    public int DelayInput;
    [SerializeField]
    public int PressedKeysMaxCount = 5;
    [SerializeField]
    public int RefreshKeyPressedAfterFrames = 120;

    public InputBuffer InputBuffer { get; set; }
    public void Awake()
    {
        InputBuffer = new InputBuffer();
        InputBuffer.DelayInput = DelayInput;
        InputBuffer.PressedKeysMaxCount = PressedKeysMaxCount;
        InputBuffer.RefreshKeyPressedAfterFrames = RefreshKeyPressedAfterFrames;
        this.GetComponent<FighterController>().InputBuffer = InputBuffer;
        StaticBuffers.Instance.RenewBuffers();
    }


    
    public void Update()
    {
        if (CollectInputFromKeyboard && !ClientData.Pause)
        {
            InputBuffer.Enqueue();
            Debug.LogError($"Player Fighter Buffer Updated");
            //DebugPrintKeysDown();

        }
       
    }


    

   
   

}
