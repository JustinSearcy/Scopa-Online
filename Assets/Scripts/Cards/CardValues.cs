using UnityEngine;
using System.Collections;

public class CardValues : MonoBehaviour
{

    [SerializeField] int value = 0;
    public int GetValue() { return value; }

    //Spades = 0
    //Clubs = 1
    //Diamonds = 2
    //Hearts = 3
    [SerializeField] int suit = 0; 
    public int GetSuit() { return suit; }
}
