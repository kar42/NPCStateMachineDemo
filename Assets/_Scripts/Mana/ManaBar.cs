using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    private Image barImage;
    public Mana mana;
    public PlayerCombat playerCombat;
    public PlayerController playerController;

    // Start is called before the first frame update
    void Awake()
    {
        barImage = transform.Find("Bar").GetComponent<Image>();
        barImage.fillAmount = .1f;

        mana = new Mana();

        //CMDebug.ButtonUI(new Vector2(0, -50), "spend mana", () => { mana.TrySpendMana(30); });
    }



    // Update is called once per frame
    void Update()
    {
            mana.Update();
            barImage.fillAmount = mana.GetManaNormalized();
    }

    public bool TryUseMana(int amount)
    {
        Debug.Log("Trying to use mana");
        return mana.TrySpendMana(amount);
    }
    
    public bool CanUseMana(int amount)
    {
        return mana.CanSpendMana(amount);
    }

    public void RegenMana(int amount)
    {
        mana.RegenMana(amount);
    }
}
