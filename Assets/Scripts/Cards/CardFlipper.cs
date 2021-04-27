using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardFlipper : MonoBehaviour
{
    public Sprite cardFront;
    public Sprite cardBack;

    public void Flip()
    {
        GameObject cardChild = gameObject.transform.GetChild(7).gameObject;
        Sprite currentSprite = cardChild.GetComponent<Image>().sprite;

        if(currentSprite == cardFront)
        {
            cardChild.GetComponent<Image>().sprite = cardBack;
        }
        else
        {
            cardChild.GetComponent<Image>().sprite = cardFront;
        }
    }

    
}
