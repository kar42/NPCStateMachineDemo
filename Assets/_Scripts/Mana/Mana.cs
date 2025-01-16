using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mana
{
    public const int MANA_MAX = 100;

    private float manaAmount;
    private float manaRegenAmount;


    public Mana()
    {
        manaAmount = 0;
        manaRegenAmount = 1f;
    }

    // Update is called once per frame
    public void Update()
    {
        manaAmount += manaRegenAmount * Time.deltaTime;
        manaAmount = Mathf.Clamp(manaAmount, 0f, MANA_MAX);

        
    }

    public bool TrySpendMana(int amount)
    {
        if(manaAmount >= amount)
        {
            manaAmount -= amount;
            return true;
        }

        return false;
    }
    
    public bool CanSpendMana(int amount)
    {
        if(manaAmount >= amount)
        {
            return true;
        }

        return false;
    }
    
    

    public float GetManaNormalized()
    {
        return manaAmount / MANA_MAX;
    }

    public void RegenMana(int amount)
    {
        Debug.Log(amount + " mana regenerated.");
        manaAmount += amount;
        manaAmount = Mathf.Clamp(manaAmount, 0f, MANA_MAX);
    }
}
