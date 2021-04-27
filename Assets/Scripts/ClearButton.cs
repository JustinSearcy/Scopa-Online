using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearButton : MonoBehaviour
{
    public void ClearSelectedCards()
    {
        NetworkClient.connection.identity.GetComponent<PlayerManager>().ClearSelectedCards();
    }
}
