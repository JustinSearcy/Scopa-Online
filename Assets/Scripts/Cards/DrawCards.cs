using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DrawCards : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager = null;
    [SerializeField] GameManager gameManager = null;

    private void Start() => gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();

    public void OnClick()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        if(gameManager.gameState == "InitialDeal")
        {
            playerManager.CmdDealCards();
        }
        else if(gameManager.gameState == "PlayerDeal")
        {
            if (!playerManager.hasCards())
            {
                playerManager.CmdDrawCards();
                gameManager.PlayerReady();
            }
        }
    }
}
