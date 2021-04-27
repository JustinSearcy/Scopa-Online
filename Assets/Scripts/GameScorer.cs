using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Mirror;

public class GameScorer : MonoBehaviour
{
    [SerializeField] GameObject playerTakenCards = null;
    [SerializeField] GameObject opponentTakenCards = null;
    [SerializeField] TextMeshProUGUI playerScoreText = null;
    [SerializeField] TextMeshProUGUI opponentScoreText = null;
    [SerializeField] TextMeshProUGUI playerCardsText = null;
    [SerializeField] TextMeshProUGUI playerDiamondsText = null;
    [SerializeField] TextMeshProUGUI playerSettebelloText = null;
    [SerializeField] TextMeshProUGUI playerSevensText = null;
    [SerializeField] TextMeshProUGUI playerScopasText = null;
    [SerializeField] TextMeshProUGUI opponentCardsText = null;
    [SerializeField] TextMeshProUGUI opponentDiamondsText = null;
    [SerializeField] TextMeshProUGUI opponentSettebelloText = null;
    [SerializeField] TextMeshProUGUI opponentSevensText = null;
    [SerializeField] TextMeshProUGUI opponentScopasText = null;
    [SerializeField] TextMeshProUGUI playerRoundScoresText = null;
    [SerializeField] TextMeshProUGUI opponentRoundScoresText = null;
    [SerializeField] TextMeshProUGUI playerNameText = null;
    [SerializeField] TextMeshProUGUI opponentNameText = null;

    [SerializeField] List<GameObject> playerCards = null;
    [SerializeField] List<GameObject> opponentCards = null;

    [SerializeField] GameObject scoringUI = null;
    [SerializeField] GameObject gameOverUI = null;

    //[SerializeField] float transitionTime = 5f;

    [SerializeField] int winScore = 21;

    [SerializeField] int playerScore = 0;
    [SerializeField] int opponentScore = 0;

    int playerScopas = 0;
    int opponentScopas = 0;

    int playerTargetCards = 0;
    int opponentTargetCards = 0;

    int playerDiamonds = 0;
    int opponentDiamonds = 0;

    bool playerHas = false;
    bool playerWon;

    TextMeshProUGUI gameOverText;

    Animator anim;
    GameObject dropZone = null;

    //Score from Scopas, 7D, 7's, cards, diamonds

    public void ScoreGame()
    {
        GetPlayerCards();
        CountCards();
        CountDiamonds();
        CountSevenOfDiamonds();
        CountSevens(7);
        CountScopas();
        DisplayScores();

        anim = scoringUI.GetComponent<Animator>();
        anim.SetTrigger("DropIn");

        playerScopas = 0;
        opponentScopas = 0;
        ResetPlayerCards();
        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        pm.ClearCards();
        pm.ChangeGameState("Initial Deal");
    }

    public void RemoveScoreMenu(Animator anim)
    {
        anim.SetTrigger("DropOut");
        if (PlayerWin())
        {
            gameOverUI.SetActive(true);
            gameOverText = GameObject.Find("Game Result Text").GetComponent<TextMeshProUGUI>();
            PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            if (playerWon)
            {
                gameOverText.text = pm.DisplayName + " Won";
            }
            else
            {
                if(pm.DisplayName != pm.HostName())
                {
                    gameOverText.text = pm.HostName() + " Won";
                }
                else
                {
                    gameOverText.text = pm.ClientName() + " Won";
                }
                
            }
        }
    }

    private bool PlayerWin()
    {
        playerWon = false;
        if(playerScore >= winScore || opponentScore >= winScore)
        {
            if(playerScore > opponentScore)
            {
                playerWon = true;
                return true;
            }
            else if(opponentScore > playerScore)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void GetPlayerCards()
    {
        for (int i = 0; i < playerTakenCards.transform.childCount; i++)
        {
            playerCards.Add(playerTakenCards.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < opponentTakenCards.transform.childCount; i++)
        {
            opponentCards.Add(opponentTakenCards.transform.GetChild(i).gameObject);
        }
        dropZone = GameObject.Find("Drop Zone").gameObject;
        if (NetworkClient.connection.identity.GetComponent<PlayerManager>().tookLastCard)
        {
            for (int i = 0; i < dropZone.transform.childCount; i++)
            {
                GameObject card = dropZone.transform.GetChild(i).gameObject;
                playerCards.Add(card);
            }
        }
        else
        {
            for (int i = 0; i < dropZone.transform.childCount; i++)
            {
                GameObject card = dropZone.transform.GetChild(i).gameObject;
                opponentCards.Add(card);
            }
        }
    }

    private void CountCards()
    {
        if(playerCards.Count > opponentCards.Count)
        {
            playerScore++;
            playerCardsText.color = Color.green;
            opponentCardsText.color = Color.white;
        }
        else if(playerCards.Count < opponentCards.Count)
        {
            opponentScore++;
            playerCardsText.color = Color.white;
            opponentCardsText.color = Color.green;
        }
        else 
        {
            playerCardsText.color = Color.white;
            opponentCardsText.color = Color.white;
            return; 
        }
    }

    private void CountDiamonds()
    {
        playerDiamonds = 0;
        opponentDiamonds = 0;
        foreach (GameObject card in playerCards)
        {
            if(card.GetComponent<CardValues>().GetSuit() == 2)
            {
                playerDiamonds++;
            }
        }

        foreach (GameObject card in opponentCards)
        {
            if (card.GetComponent<CardValues>().GetSuit() == 2)
            {
                opponentDiamonds++;
            }
        }

        if (playerDiamonds > opponentDiamonds)
        {
            playerScore++;
            playerDiamondsText.color = Color.green;
            opponentDiamondsText.color = Color.white;
        }
        else if(playerDiamonds < opponentDiamonds)
        {
            opponentScore++;
            playerDiamondsText.color = Color.white;
            opponentDiamondsText.color = Color.green;
        }
        else 
        {
            playerDiamondsText.color = Color.white;
            opponentDiamondsText.color = Color.white;
            return; 
        }
    }

    private void CountSevenOfDiamonds()
    {
        playerHas = false;
        foreach (GameObject card in playerCards)
        {
            if (card.GetComponent<CardValues>().GetSuit() == 2 && card.GetComponent<CardValues>().GetValue() == 7)
            {
                playerScore++;
                playerHas = true;
                playerSettebelloText.color = Color.green;
                opponentSettebelloText.color = Color.white;
            }
        }
        if (!playerHas)
        {
            opponentScore++;
            playerSettebelloText.color = Color.white;
            opponentSettebelloText.color = Color.green;
        }
    }

    private void CountSevens(int targetValue)
    {
        playerTargetCards = 0;
        opponentTargetCards = 0;
        if (targetValue == 0) 
        {
            playerSevensText.color = Color.white;
            opponentSevensText.color = Color.white;
            return; 
        }

        foreach (GameObject card in playerCards)
        {
            if (card.GetComponent<CardValues>().GetValue() == targetValue)
            {
                playerTargetCards++;
            }
        }

        foreach (GameObject card in opponentCards)
        {
            if (card.GetComponent<CardValues>().GetValue() == targetValue)
            {
                opponentTargetCards++;
            }
        }

        if (playerTargetCards > opponentTargetCards)
        {
            playerScore++;
            playerSevensText.color = Color.green;
            opponentSevensText.color = Color.white;
        }
        else if (playerTargetCards < opponentTargetCards)
        {
            opponentScore++;
            playerSevensText.color = Color.white;
            opponentSevensText.color = Color.green;
        }
        else { CountSevens(targetValue - 1); }
    }

    private void CountScopas()
    {
        playerScore += playerScopas;
        opponentScore += opponentScopas;
    }

    public void playerScopa()
    {
        playerScopas++;
    }

    public void opponentScopa()
    {
        opponentScopas++;
    }

    private void DisplayScores()
    {
        playerScoreText.text = "Score: " + playerScore;
        opponentScoreText.text = "Score: " + opponentScore;
        playerScopasText.text = "" + playerScopas;
        opponentScopasText.text = "" + opponentScopas;
        playerCardsText.text = "" + playerCards.Count;
        opponentCardsText.text = "" + opponentCards.Count;
        playerDiamondsText.text = "" + playerDiamonds;
        opponentDiamondsText.text = "" + opponentDiamonds;
        playerSevensText.text = "" + playerTargetCards;
        opponentSevensText.text = "" + opponentTargetCards;
        playerRoundScoresText.text = "" + playerScore;
        opponentRoundScoresText.text = "" + opponentScore;

        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        if (pm.IsClientOnly())
        {
            playerNameText.text = pm.ClientName();
            opponentNameText.text = pm.HostName();
        }
        else
        {
            playerNameText.text = pm.HostName();
            opponentNameText.text = pm.ClientName();
        }


        if (playerHas)
        {
            playerSettebelloText.text = "1";
            opponentSettebelloText.text = "0";
        }
        else
        {
            playerSettebelloText.text = "0";
            opponentSettebelloText.text = "1";
        }

    }

    public void ResetPlayerScore()
    {
        playerScore = 0;
        opponentScore = 0;
        playerScoreText.text = "Score: " + playerScore;
        opponentScoreText.text = "Score: " + opponentScore;
    }

    private void ResetPlayerCards()
    {
        playerCards.Clear();
        opponentCards.Clear();
    }
}
