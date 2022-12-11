using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{

    private const int _HIGH_DMG_THRESHOLD = 15;

    public int MaxHealth = 100;
    public int MinHealth = 0;


    public bool IsEnemy;
    public int CurrentHealth { get;  private set; }

    public Animator Animator;
    private void Awake()
    {
        CurrentHealth = MaxHealth;
        Animator = this.gameObject.GetComponent<Animator>();
    }

    public void TakeDamage(int dmgAmount) 
    {
        this.CurrentHealth -= dmgAmount;
        if (this.CurrentHealth<0)
        {
            this.CurrentHealth = 0;
            Animator.SetBool("IsDead", true);
        }

        if (dmgAmount > _HIGH_DMG_THRESHOLD)
        {
            Animator.SetTrigger("HighDamage");
        }
        else 
        {
            Animator.SetTrigger("LowDamage");
        }
    }

    
  
    // Update is called once per frame
    void Update()
    {
      

        float fillValue = (float)CurrentHealth / (float)MaxHealth;
    }
}
