using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class CardDeck : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] List<GameObject> cardDeck = new List<GameObject>();

    [SerializeField] List<GameObject> fullDeck = null;

    public List<GameObject> getCurrentDeck() { return cardDeck; }

    public List<GameObject> getFullDeck() { return fullDeck; }

    public void removeCard(int cardIndex)
    {
        cardDeck.RemoveAt(cardIndex);
    }

    /*public void ResetDeck()
    {
        foreach (GameObject card in fullDeck)
        {
            cardDeck.Add(card);
        }
    }*/

}
