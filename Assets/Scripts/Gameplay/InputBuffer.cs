using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct InputElement 
{
    [FieldOffset(0)]
    public Int32 timeStamp; //4 bytes
    [FieldOffset(4)]
    public KeyCode key;     //4 bytes
}

public enum InputBufferState
{
    InputCollector, InputReceiver
}


public class InputFrame 
{
    public InputElement[] _inputInFrame;
    int DelayInput = 0;
    public InputFrame(KeyCode[] allowedKeys,int delay=0)
    {
        _inputInFrame = new InputElement[allowedKeys.Length];
        this.DelayInput = delay;
        for (int i = 0; i < allowedKeys.Length; i++)
        {
            
            if (Input.GetKey(allowedKeys[i]))
            {
                KeyCode keyCode = allowedKeys[i];
                _inputInFrame[i].key = keyCode;
            }
            _inputInFrame[i].timeStamp = FrameLimiter.Instance.FramesInPlay + DelayInput;
        }
    }

    public InputFrame(InputElement[] elements)
    {
        this._inputInFrame = elements;
    }

   

}
public class InputBuffer : MonoBehaviour
{

    //TODO make buffer only accpets keys that do sth in the game
    public KeyCode[] allowedKeysArr;

    // public readonly object bufferLock = new object();
    public InputBufferState bufferState;
    

   
    [SerializeField]
    public int DelayInput;

    public Queue<InputFrame> BufferedInput { get; set; }

    public float DebugOutputTime;

    //private Queue<InputElement> _keyDownBuffer;
    //Refreshed buffer after seconds if new key has not been received the meantime
    //public uint MaxKeyDownInputs;

    public float RefreshBufferAfter;
    // Start is called before the first frame update
    public InputFrame LastFrame { get; set; }

    void Start()
    {
        allowedKeysArr = new KeyCode[] { KeyCode.Space, KeyCode.S, KeyCode.A, KeyCode.D };
        StartCoroutine(DebugPrintAfter());
        StartCoroutine(DeleteBufferAter());
        //_inputElements = new InputElement[5];
        BufferedInput = new Queue<InputFrame>();
        // _keyDownBuffer = new Queue<InputElement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ClientData.IsPaused)
        {
            return;
        }
        if (bufferState == InputBufferState.InputCollector)
        {
            AddNewFrame();
       
        }
        //Enemy buffer is updated on another thread when Socket Receive Completes
    }

  

    public void AddNewFrame(InputFrame inputFrame=null) 
    {
        if (inputFrame==null)
        {
            inputFrame = new InputFrame(allowedKeysArr,DelayInput);
        }
        if (BufferedInput.Count > DelayInput)
        {
            BufferedInput.Dequeue();
        }
        BufferedInput.Enqueue(inputFrame);
        LastFrame = inputFrame;

    }
    public InputElement[] GetFirstFrame() 
    {
        if (this.BufferedInput !=null && this.BufferedInput.Count>0)
        {
            return this.BufferedInput.Peek()._inputInFrame;
        }
        return new InputElement[0];
    }
    

    IEnumerator DebugPrintAfter()
    {
        while (true)
        {
            yield return new WaitForSeconds(DebugOutputTime);
           // DebugPrint();
        }
     
    }

    IEnumerator DeleteBufferAter()
    {
        yield return new WaitForSeconds(RefreshBufferAfter);
        //while (true)
        //{
        //    yield return new WaitForSeconds(RefreshBufferAfter);
        //    this._keyDownBuffer = new Queue<InputElement>();
        //}

    }

    public void DebugPrint() 
    {
        //string strKeyDownBuff = "Key Down Buffer";
        //foreach (var el in this._keyDownBuffer)
        //{
        //    strKeyDownBuff += el.key.ToString();
        //}

        string strAllInputBuff="All Input Buffer";
        foreach (var inputFrame in this.BufferedInput)
        {
            foreach (var el in inputFrame._inputInFrame)
            {
                strAllInputBuff += el.key.ToString();
            }
        }
        // Debug.LogError(_inputElements.Count);
        //  Debug.LogError(strKeyDownBuff);
        Debug.LogError(strAllInputBuff);
    }

  
}
