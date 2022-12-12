using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatePhysics : MonoBehaviour
{
    private void Update()
    {
        if (!ClientData.Pause)
        {
            Physics2D.Simulate(0.016667f);
        }

    }
}
