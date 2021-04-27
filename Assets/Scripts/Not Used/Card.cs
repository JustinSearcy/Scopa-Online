using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card")]
public class Card : ScriptableObject
{
    public string rank;

    public int rankValue;

    //Spades = 0
    //Clubs = 1
    //Diamonds = 2
    //Hearts = 3
    public int suitvalue; 

    public Sprite suit;
}
