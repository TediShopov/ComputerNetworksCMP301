using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterRBControlller : FighterController
{
    public override void Update()
    {

        Debug.LogError($"Call From RB Controller");
        base.Update();
    }


}
