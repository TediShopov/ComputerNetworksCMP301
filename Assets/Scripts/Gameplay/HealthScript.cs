using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{

    public int MaxHealth=100;
    public int MinHealth=0;


    public bool IsEnemy;
    public int CurrentHealth=100;
  
    // Update is called once per frame
    void Update()
    {
      

        float fillValue = (float)CurrentHealth / (float)MaxHealth;
    }
}
