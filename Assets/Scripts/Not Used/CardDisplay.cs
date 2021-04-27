using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card card;

    public TextMeshProUGUI valueTL;
    public TextMeshProUGUI valueBR;

    public Image suit;
    public Image suitTL;
    public Image suitBR;

    void Start()
    {
        valueTL.text = card.rank;
        valueBR.text = card.rank;

        suit.sprite = card.suit;
        suitTL.sprite = card.suit;
        suitBR.sprite = card.suit;
    }  
}
