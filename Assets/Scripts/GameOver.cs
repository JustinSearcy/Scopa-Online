using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void Rematch()
    {
        FindObjectOfType<GameManager>().ChangeGameState("InitialDeal");
        FindObjectOfType<GameScorer>().ResetPlayerScore();
        NetworkClient.connection.identity.GetComponent<PlayerManager>().ClearCards();
        gameObject.SetActive(false);
    }
}
