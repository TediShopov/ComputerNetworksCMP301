using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

//[StructLayout(LayoutKind.Explicit, Size = 4)]
//public struct InputElement 
//{
//    [FieldOffset(0)]
//    public KeyCode key;     //4 bytes
//}


public class InputFrame 
{
    //public InputElement[] _inputInFrame;
    public byte[] Inputs;
    public Int32 TimeStamp { get; set; }


    //int DelayInput = 0;

    public InputFrame()
    {
        this.TimeStamp = -1;
        this.Inputs = new byte[ClientData.AllowedKeys.Length];
    }

   public static InputFrame CaptureInput(int delay) 
    {
        InputFrame captureInputs= new InputFrame();
        captureInputs.Inputs = new byte[ClientData.AllowedKeys.Length];
        //this.DelayInput = delay;
        for (int i = 0; i < ClientData.AllowedKeys.Length; i++)
        {
            if (Input.GetKey(ClientData.AllowedKeys[i]))
            {
                captureInputs.Inputs[i] = 255;
            }
            captureInputs.TimeStamp = FrameLimiter.Instance.FramesInPlay + delay;
        }
        return captureInputs;
    }

    public bool IsKey(KeyCode keyCode)
    {
        int index = ClientData.AllowedKeysIndex[keyCode];
        return Inputs[index] != 0;
        
    }


    public void SetKey(KeyCode code) 
    {
        int index = ClientData.AllowedKeysIndex[code];
        Inputs[index] = 255;
    }

    public InputFrame(byte[] inputs,Int32 timestamp)
    {

        this.TimeStamp = timestamp;
        this.Inputs = inputs;
        
    }

    public InputFrame(InputFramePacket packet)
    {

        this.TimeStamp = packet.TimeStamp;
        this.Inputs = packet.InputElements;
  
    }

}


public class InputBuffer
{
    //TODO make buffer only accpets keys that do sth in the game

    public Queue<InputFrame> BufferedInput { get; set; }
    public int DelayInput;

    public delegate void InputFrameDelegate(InputFrame inputFrame);  // delegate
    public event InputFrameDelegate OnInputFrameAdded; // event
    public event InputFrameDelegate OnInputFrameDiscarded; // event

    public Queue<KeyCode> PressedKeys;
    public HashSet<KeyCode> KeyDowned;
    public int PressedKeysMaxCount = 5;
    public int RefreshKeyPressedAfterFrames = 120;
    private int _framesPassedSinceKeyDown = 0;

    public bool IsEmpty => this.BufferedInput.Count <= 0;

    public bool IsOverflow => BufferedInput.Count > DelayInput;

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
        BufferedInput = new Queue<InputFrame>(inputBuffer.BufferedInput);
        PressedKeys = new Queue<KeyCode>(inputBuffer.PressedKeys);
        KeyDowned = new HashSet<KeyCode>(inputBuffer.KeyDowned);
        LastFrame = inputBuffer.LastFrame;
        this.PressedKeysMaxCount = inputBuffer.PressedKeysMaxCount;
        this.RefreshKeyPressedAfterFrames = inputBuffer.RefreshKeyPressedAfterFrames;
        this._framesPassedSinceKeyDown = inputBuffer._framesPassedSinceKeyDown;
        this.DelayInput = inputBuffer.DelayInput;
        //this.OnInputFrameAdded = inputBuffer.OnInputFrameAdded;
        //this.OnInputFrameDiscarded = inputBuffer.OnInputFrameDiscarded;
    }
    public void Clear() 
    {
        BufferedInput.Clear();
        PressedKeys.Clear();
        KeyDowned.Clear();
        LastFrame = null;
        this._framesPassedSinceKeyDown = 0;

    }




    public void Enqueue(InputFrame inputFrame = null)
    {
        if (inputFrame == null)
        {
            inputFrame = InputFrame.CaptureInput(DelayInput);
        }

        BufferedInput.Enqueue(inputFrame);
     
        LastFrame = inputFrame;

        ////Call on add event
        //RecordKeysDown(inputFrame);

        OnInputFrameAdded?.Invoke(inputFrame);
    }


    public InputFrame Dequeue() 
    {
        if (this.BufferedInput?.Count<=0)
        {
            return null;
        }
        OnInputFrameDiscarded?.Invoke(this.BufferedInput.Peek());
        //Call on add event
        RecordKeysDown(this.BufferedInput.Peek());
        return this.BufferedInput.Dequeue();
    }



    void RecordKeysDown(InputFrame frame) 
    {
        //Already takes into acount pririty of inputs
       
            for (int i = 0; i < ClientData.AllowedKeys.Length; i++)
            {
                var key = ClientData.AllowedKeys[i];
                var inputDown = frame.IsKey(key);
                if (inputDown)
                {
                    if (!KeyDowned.Contains(key))
                    {
                        KeyDowned.Add(key);
                    }
                }
                //K == 0
                else
                {
                    //On Release
                    if (KeyDowned.Contains(key))
                    {
                        if (PressedKeys.Count > PressedKeysMaxCount)
                        {
                            //Test out what is the error dequing empty q
                            PressedKeys.Dequeue();

                        }
                        KeyDowned.Remove(key);
                        PressedKeys.Enqueue(key);
                        _framesPassedSinceKeyDown = 0;
                        break;
                    }

                }
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
            foreach (var key in ClientData.AllowedKeys)
            {
                if (inputFrame.IsKey(key))
                {
                    strAllInputBuff += key;
                }
               
            }
        }
        Debug.LogError(strAllInputBuff);
    }


}
