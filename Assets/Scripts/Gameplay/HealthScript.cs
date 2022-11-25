using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public int MaxHealth=100;
    public int MinHealth=0;

    public int CurrentHealth=100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CurrentHealth -= 20;
        }
        if (CurrentHealth<MinHealth)
        {
            Debug.LogWarning("Player Died");
        }
    }
}
