﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct InputElement 
{
    [FieldOffset(0)]
    public KeyCode key;     //4 bytes
}


public class InputFrame 
{
    public InputElement[] _inputInFrame;
    public Int32 TimeStamp { get; set; }
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
            TimeStamp = FrameLimiter.Instance.FramesInPlay + DelayInput;
        }
    }

    public InputFrame(InputElement[] elements, Int32 timestamp)
    {
        this.TimeStamp = timestamp;
        this._inputInFrame = elements;
    }

   

}


public class InputBuffer :MonoBehaviour
{
    //TODO make buffer only accpets keys that do sth in the game

    public Queue<InputFrame> BufferedInput { get; set; }
    [SerializeField]
    public bool CollectInputFromKeyboard;
    [SerializeField]
    public int DelayInput;

    public delegate void InputFrameDelegate(InputFrame inputFrame);  // delegate
    public event InputFrameDelegate OnInputFrameAdded; // event
    public event InputFrameDelegate OnInputFrameDiscarded; // event

    public Queue<KeyCode> PressedKeys;

    public HashSet<KeyCode> KeyDowned;
    [SerializeField]
    public int PressedKeysMaxCount=5;
    [SerializeField]
    public int RefreshKeyPressedAfterFrames=120;
    private int _framesPassedSinceKeyDown=0;


    public InputFrame LastFrame { get; set; }

    public void  Start()
    {
        BufferedInput = new Queue<InputFrame>();
        PressedKeys = new Queue<KeyCode>();
        KeyDowned = new HashSet<KeyCode>();
        OnInputFrameAdded += RecordKeysDown;
    }

    public void Update()
    {
        if (CollectInputFromKeyboard)
        {
            AddNewFrame();
            DebugPrintKeysDown();

        }
        _framesPassedSinceKeyDown++;
        if (_framesPassedSinceKeyDown>=RefreshKeyPressedAfterFrames)
        {
            
            PressedKeys.Clear();
            _framesPassedSinceKeyDown = 0;
        }
    }


    // Update is called once per frame

    public void AddNewFrame(InputFrame inputFrame = null)
    {
        if (inputFrame == null)
        {
            inputFrame = new InputFrame(ClientData.AllowedKeys, DelayInput);
        }
        if (BufferedInput.Count > DelayInput)
        {
            var deqInputFrame = BufferedInput.Dequeue();
            OnInputFrameDiscarded?.Invoke(deqInputFrame);
        }
        BufferedInput.Enqueue(inputFrame);
        LastFrame = inputFrame;

        //Call on add event
        OnInputFrameAdded?.Invoke(inputFrame);


    }



    void RecordKeysDown(InputFrame frame) 
    {
        //Already takes into acount pririty of inputs
        try
        {
            for (int i = 0; i < frame._inputInFrame.Length; i++)
            {
                var input = frame._inputInFrame[i];
                KeyCode inputCheck = ClientData.AllowedKeys[i];

                if (inputCheck == input.key)
                {
                    if (!KeyDowned.Contains(inputCheck))
                    {
                        KeyDowned.Add(inputCheck);
                    }
                }
                else
                {
                    //On Release
                    if (KeyDowned.Contains(inputCheck))
                    {

                        if (PressedKeys.Count > PressedKeysMaxCount)
                        {
                            PressedKeys.Dequeue();

                        }
                        KeyDowned.Remove(inputCheck);
                        PressedKeys.Enqueue(inputCheck);
                        _framesPassedSinceKeyDown = 0;
                        break;
                    }

                }
            }
        }
        catch (IndexOutOfRangeException e)
        {
            int a = 3;
            throw;
        }
        
       
    }
    public InputFrame GetFirstFrame()
    {
        if (this.BufferedInput != null && this.BufferedInput.Count > 0)
        {
            return this.BufferedInput.Peek();
        }
        return new InputFrame(new InputElement[0], -1);
    }



    public void DebugPrintKeysDown() 
    {
        string strKeyDownBuff = "Key Down Buffer";
        foreach (var el in this.PressedKeys)
        {
            strKeyDownBuff += el.ToString();
        }
        Debug.LogError(strKeyDownBuff);

    }
    public void DebugPrint()
    {
        
        string strAllInputBuff = " Input Buffer";
        foreach (var inputFrame in this.BufferedInput)
        {
            foreach (var el in inputFrame._inputInFrame)
            {
                strAllInputBuff += el.key.ToString();
            }
        }
        Debug.LogError(strAllInputBuff);
    }


}
//public class InputBuffer : MonoBehaviour
//{

//    //TODO make buffer only accpets keys that do sth in the game
//    public KeyCode[] AllowedKeys;

//    // public readonly object bufferLock = new object();
//    public InputBufferState BufferState;
    

   
//    [SerializeField]
//    public int DelayInput;

//    public Queue<InputFrame> BufferedInput { get; set; }

//    public float DebugOutputTime;

//    public InputBuffer RedirectDequedInputFramesTo;
//    //private Queue<InputElement> PressedKeys;
//    //Refreshed buffer after seconds if new key has not been received the meantime
//    //public uint MaxKeyDownInputs;

//    public float RefreshBufferAfter;
//    // Start is called before the first frame update
//    public InputFrame LastFrame { get; set; }

//    void Start()
//    {
//        AllowedKeys = new KeyCode[] { KeyCode.Space, KeyCode.S, KeyCode.A, KeyCode.D };

//        StartCoroutine(DebugPrintAfter());
//        StartCoroutine(DeleteBufferAter());
//        BufferedInput = new Queue<InputFrame>();
//        // PressedKeys = new Queue<InputElement>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (ClientData.Pause)
//        {
//            return;
//        }
//        if (BufferState == InputBufferState.InputCollector)
//        {
//            AddNewFrame();
       
//        }
//        //Enemy buffer is updated on another thread when Socket Receive Completes
//    }

  

//    public void AddNewFrame(InputFrame inputFrame=null) 
//    {
//        if (inputFrame==null)
//        {
//            inputFrame = new InputFrame(AllowedKeys,DelayInput);
//        }
//        if (BufferedInput.Count > DelayInput)
//        {
//            var deqInputFrame=BufferedInput.Dequeue();
//            if (RedirectDequedInputFramesTo != null)
//            {
//                RedirectDequedInputFramesTo.AddNewFrame(deqInputFrame);
//            }
//        }
//        BufferedInput.Enqueue(inputFrame);
//        LastFrame = inputFrame;

//    }
//    public InputFrame GetFirstFrame() 
//    {
//        if (this.BufferedInput !=null && this.BufferedInput.Count>0)
//        {
//            return this.BufferedInput.Peek();
//        }
//        return new InputFrame(new InputElement[0],-1);
//    }
    

//    IEnumerator DebugPrintAfter()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(DebugOutputTime);
//           // DebugPrint();
//        }
     
//    }

//    IEnumerator DeleteBufferAter()
//    {
//        yield return new WaitForSeconds(RefreshBufferAfter);
//        //while (true)
//        //{
//        //    yield return new WaitForSeconds(RefreshBufferAfter);
//        //    this.PressedKeys = new Queue<InputElement>();
//        //}

//    }

//    public void DebugPrint() 
//    {
//        //string strKeyDownBuff = "Key Down Buffer";
//        //foreach (var el in this.PressedKeys)
//        //{
//        //    strKeyDownBuff += el.key.ToString();
//        //}

//        string strAllInputBuff="All Input Buffer";
//        foreach (var inputFrame in this.BufferedInput)
//        {
//            foreach (var el in inputFrame._inputInFrame)
//            {
//                strAllInputBuff += el.key.ToString();
//            }
//        }
//        // Debug.LogError(_inputElements.Count);
//        //  Debug.LogError(strKeyDownBuff);
//        Debug.LogError(strAllInputBuff);
//    }

  
//}