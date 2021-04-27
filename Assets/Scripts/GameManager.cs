using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public int playerScore = 0;
    public int opponentScore = 0;
    public string gameState = "InitialDeal";

    public int round = 0;
    [SerializeField] TextMeshProUGUI roundText = null;

    public int readyPlayers = 0; //public for debug

    [SerializeField] TextMeshProUGUI playerTurn = null;

    [SerializeField] GameObject pauseMenu = null;

    private void Start()
    {
        Application.targetFrameRate = 300;
        UpdateRoundText();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
        }
    }
    

    public void ChangeGameState(string stateRequest)
    {
        switch (stateRequest)
        {
            case "InitialDeal":
                round = 0;
                UpdateRoundText();
                playerTurn.text = "Deal Phase";
                gameState = "InitialDeal";
                break;

            case "PlayerDeal":
                playerTurn.text = "Draw Phase";
                gameState = "PlayerDeal";
                readyPlayers = 0;
                IncreaseRound();
                break;

            case "Play":
                gameState = "Play";
                break;

            case "Evaluate":
                gameState = "Evaluate";
                break;

            default:
                break;
        }
    }

    public void IncreaseRound()
    {
        round++;
        UpdateRoundText();
    }

    private void UpdateRoundText() => roundText.text = "Round: " + round + "/6";

    public void PlayerReady()
    {
        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        readyPlayers++;
        if(readyPlayers >= 3)
        {
            readyPlayers = 0;
            ChangeGameState("Play");
            if (pm.isMyTurn)
            {
                playerTurn.text = "Your Turn";
            }
            else
            {
                playerTurn.text = "Opponent's Turn";
            }
        }
    }
}
