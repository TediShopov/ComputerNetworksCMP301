using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealth : MonoBehaviour
{
    public HealthScript health;
    private Slider slider; 
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        float fillValue = (float)health.CurrentHealth / (float)health.MaxHealth;
        slider.value = fillValue;
    }
}
