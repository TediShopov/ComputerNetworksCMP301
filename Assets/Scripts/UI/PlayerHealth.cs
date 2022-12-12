using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealth : MonoBehaviour
{
    private Slider slider; 

    public bool isLeft;

    private bool isPlayerHp => isLeft == !ClientData.CharacterIndex;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
       
    }

    // Update is called once per frame
    void Update()
    {
        HealthScript health;
        if (isPlayerHp)
        {
            health = StaticBuffers.Instance.Player.GetComponent<HealthScript>();
        }
        else
        { 
            health = StaticBuffers.Instance.Enemy.GetComponent<HealthScript>();

        }

        if (Input.GetKeyDown(KeyCode.Y) && this.isPlayerHp)
        {
            health.TakeDamage(20);
        }

        if (Input.GetKeyDown(KeyCode.U) && !this.isPlayerHp)
        {
            health.TakeDamage(20);
        }
        
        float fillValue = (float)health.CurrentHealth / (float)health.MaxHealth;
        slider.value = fillValue;
    }
}
