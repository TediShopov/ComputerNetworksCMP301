using System;
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

   
    public InputFrame(InputElement[] elements=null, Int32 timestamp=-1)
    {
        if (elements == null)
        {
            elements = new InputElement[ClientData.AllowedKeys.Length];
        }
        this.TimeStamp = timestamp;
        this._inputInFrame = elements;
    }

   

}


public class InputBuffer 
{
    //TODO make buffer only accpets keys that do sth in the game

    public Queue<InputFrame> BufferedInput { get; set; }
    public int DelayInput;

    public  delegate void InputFrameDelegate(InputFrame inputFrame);  // delegate
    public event InputFrameDelegate OnInputFrameAdded; // event
    public event InputFrameDelegate OnInputFrameDiscarded; // event

    public Queue<KeyCode> PressedKeys;
    public HashSet<KeyCode> KeyDowned;
    public int PressedKeysMaxCount=5;
    public int RefreshKeyPressedAfterFrames=120;
    private int _framesPassedSinceKeyDown=0;

    //The buffer is ready when there is enough buffered input
    //to match the delay
    public bool IsReady { get {return this.BufferedInput.Count > DelayInput; } }
    public InputFrame LastFrame { get; set; }

    public InputBuffer()
    {
        BufferedInput = new Queue<InputFrame>();
        PressedKeys = new Queue<KeyCode>();
        KeyDowned = new HashSet<KeyCode>();
    }

    public InputBuffer(InputBuffer inputBuffer)
    {
        this.SetTo(inputBuffer);
    }
   

    public void SetTo(InputBuffer inputBuffer) 
    {
        BufferedInput = new Queue<InputFrame>( inputBuffer.BufferedInput);
        PressedKeys = new Queue<KeyCode>( inputBuffer.PressedKeys);
        KeyDowned = new HashSet<KeyCode>( inputBuffer.KeyDowned);
        LastFrame = inputBuffer.LastFrame;
        this.PressedKeysMaxCount = inputBuffer.PressedKeysMaxCount;
        this.RefreshKeyPressedAfterFrames = inputBuffer.RefreshKeyPressedAfterFrames;
        this._framesPassedSinceKeyDown = inputBuffer._framesPassedSinceKeyDown;
        //this.OnInputFrameAdded = inputBuffer.OnInputFrameAdded;
        //this.OnInputFrameDiscarded = inputBuffer.OnInputFrameDiscarded;
    }

   


    public bool IsOverflow { get {return BufferedInput.Count > DelayInput;}}
    public void Enqueue(InputFrame inputFrame = null)
    {
        if (inputFrame == null)
        {
            inputFrame = new InputFrame(ClientData.AllowedKeys, DelayInput);
        }
       
        BufferedInput.Enqueue(inputFrame);
        //if (BufferedInput.Count > DelayInput)
        //{
        //    var deqInputFrame = BufferedInput.Dequeue();
        //    OnInputFrameDiscarded?.Invoke(deqInputFrame);
        //}
        LastFrame = inputFrame;

        //Call on add event
        RecordKeysDown(inputFrame);

        OnInputFrameAdded?.Invoke(inputFrame);


    }

    

    public InputFrame Dequeue() 
    {
        if (this.BufferedInput?.Count<=0)
        {
            return null;
        }
        OnInputFrameDiscarded?.Invoke(this.BufferedInput.Peek());
        return this.BufferedInput.Dequeue();
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

                // K == 1
               
                if (inputCheck == input.key)
                {
                    if (!KeyDowned.Contains(inputCheck))
                    {
                        KeyDowned.Add(inputCheck);
                    }
                }
                //K == 0
                else
                {
                    //On Release
                    if (KeyDowned.Contains(inputCheck))
                    {
                        // Held Down: A
                        // Pressed Keys: S D
                        // Pressed Keys: 

                        //DSpASD
                        if (PressedKeys.Count > PressedKeysMaxCount)
                        {
                            //Test out what is the error dequing empty q
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
    public InputFrame Peek()
    {
        if (this.BufferedInput != null && this.BufferedInput.Count > 0)
        {
            return this.BufferedInput.Peek();
        }
        return new InputFrame();
    }

    public void OnUpdate() 
    
    {
        _framesPassedSinceKeyDown++;
        if (_framesPassedSinceKeyDown >= RefreshKeyPressedAfterFrames)
        {

            PressedKeys.Clear();
            _framesPassedSinceKeyDown = 0;
        }
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
