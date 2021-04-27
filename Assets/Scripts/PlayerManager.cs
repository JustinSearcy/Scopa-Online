using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    //All serialized for debug
    [SerializeField] GameObject playerArea = null;
    [SerializeField] GameObject opponentArea = null;
    [SerializeField] GameObject dropZone = null;
    [SerializeField] GameObject playerTakenCards = null;
    [SerializeField] GameObject opponentTakenCards = null;
    [SerializeField] TextMeshProUGUI playerTurn = null;
    [SerializeField] TextMeshProUGUI playerScore = null;
    [SerializeField] TextMeshProUGUI opponentScore = null;
    [SerializeField] TextMeshProUGUI playerName = null;
    [SerializeField] TextMeshProUGUI opponentName = null;

    [SerializeField] GameObject selectedCard = null; //Serialize for debug
    [SerializeField] Button clearButton = null;
    //[SerializeField] Button dealCards = null;

    [SerializeField] List<GameObject> selectedCards = null; //Serialize for debug
    [SerializeField] List<GameObject> takenCards = null; //Serialize for debug

    [SerializeField] int selectedValue = 0; //Serialize for debug
    [SerializeField] public bool tookLastCard = false;

    GameManager gameManager;
    GameScorer gameScorer;

    public bool isMyTurn = false;

    [SyncVar]
    public string DisplayName = "Loading...";

    private NetworkManagerScopa room;

    private NetworkManagerScopa Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerScopa;
        }
    }

    public override void OnStartClient()
    {
        //base.OnStartClient();

        DontDestroyOnLoad(gameObject);

        Room.PlayerManagers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.PlayerManagers.Remove(this);
    }

    public bool IsClientOnly()
    {
        return isClientOnly;
    }

    public String ClientName()
    {
        return Room.PlayerManagers[0].DisplayName;
    }

    public String HostName()
    {
        return Room.PlayerManagers[1].DisplayName;
    }

    public void GetObjects() //Add coroutine/wait for both players to be loaded??????
    {
        StartCoroutine(WaitToLoad());
    }

    IEnumerator WaitToLoad()
    {
        yield return new WaitForSeconds(3f);
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        gameScorer = GameObject.Find("Game Manager").GetComponent<GameScorer>();
        playerArea = GameObject.Find("Player Area");
        opponentArea = GameObject.Find("Opponent Area");
        dropZone = GameObject.Find("Drop Zone");
        playerTakenCards = GameObject.Find("Player Taken Cards");
        opponentTakenCards = GameObject.Find("Opponent Taken Cards");
        playerTurn = GameObject.Find("Player Turn").GetComponent<TextMeshProUGUI>();
        playerTurn.text = "Deal Phase";
        playerScore = GameObject.Find("Player Score").GetComponent<TextMeshProUGUI>();
        playerScore.text = "Score: 0";
        opponentScore = GameObject.Find("Opponent Score").GetComponent<TextMeshProUGUI>();
        opponentScore.text = "Score: 0";
        playerName = GameObject.Find("Player Name").GetComponent<TextMeshProUGUI>();
        opponentName = GameObject.Find("Opponent Name").GetComponent<TextMeshProUGUI>();
        clearButton = GameObject.Find("Clear Button").GetComponent<Button>();

        //dealCards = GameObject.Find("Deal Cards").GetComponent<Button>();

        if (isClientOnly)
        {
            isMyTurn = true;
            //playerName.text = Room.PlayerManagers[0].DisplayName;
            //opponentName.text = Room.PlayerManagers[1].DisplayName;
            playerName.text = ClientName();
            opponentName.text = HostName();
            clearButton.interactable = true;
        }
        else
        {
            isMyTurn = false;
            //playerName.text = Room.PlayerManagers[1].DisplayName;
            //opponentName.text = Room.PlayerManagers[0].DisplayName;
            playerName.text = HostName();
            opponentName.text = ClientName();
            clearButton.interactable = false;
        }
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.DisplayName = displayName;
    }

    //Return true if player has cards left
    public bool hasCards()
    {
        if(playerArea.transform.childCount > 0) { return true; }
        else { return false; }
    }

    //Deselects current cards and selects a new one
    public void SetSelectedCard(GameObject card)
    {
        if(selectedCard != null)
        {
            selectedCard.GetComponent<DragDrop>().Deselect();
            selectedCards.Clear();
        }
        selectedCard = card;
    }

    //Returns true if the player has a selected card from their player area
    public bool HasSelectCard()
    {
        if(selectedCard != null) { return true; }
        else { return false; }
    }

    //Clears the list of selected cards
    public void ClearSelectedCards()
    {
        selectedCards.Clear();
        if(selectedCard != null)
        {
            DragDrop dragDrop = selectedCard.GetComponent<DragDrop>();
            dragDrop.Deselect();
            dragDrop.DeselectAll();
            dragDrop.resetColor();
            selectedCard = null;
        }
    }

    //Picks up cards if able, checks for end of round
    public void PickUpCard(GameObject card)
    {
        if (selectedCard != null)
        {
            selectedCards.Add(card);
            selectedValue = 0;
            for (int i = 0; i < selectedCards.Count; i++)
            {
                selectedValue += selectedCards[i].GetComponent<CardValues>().GetValue();
            }
            if (selectedValue == selectedCard.GetComponent<CardValues>().GetValue())
            {
                if (FaceCardRule())
                {
                    for (int i = 0; i < selectedCards.Count; i++)
                    {
                        CmdTakeCard(selectedCards[i], "Taken");
                        takenCards.Add(selectedCards[i]);
                    }
                    if (dropZone.transform.childCount == selectedCards.Count)
                    {
                        CmdTakeCard(selectedCard, "Scopa");
                        takenCards.Add(selectedCard);
                    }
                    else
                    {
                        CmdTakeCard(selectedCard, "Used");
                        takenCards.Add(selectedCard);
                    }
                    ClearSelectedCards();
                    CmdChangeTurn();
                    CmdTookCardLast(); //Sets everyone to false
                    StartCoroutine(TookLastCard());
                    StartCoroutine(CheckRoundEnd());
                }
                else
                {
                    FailedPickUp(card);
                }
            }
            else if(selectedValue > selectedCard.GetComponent<CardValues>().GetValue())
            {
                FailedPickUp(card);
            }
        }  
    }

    private void FailedPickUp(GameObject card)
    {
        foreach (GameObject selectedCard in selectedCards)
        {
            selectedCard.GetComponent<DragDrop>().Deselect();
        }
        ClearSelectedCards();
        card.GetComponent<DragDrop>().resetColor();
    }

    private bool FaceCardRule()
    {
        bool followsRule = true;
        int cardValue = selectedCard.GetComponent<CardValues>().GetValue();
        if (selectedCards.Count == 1)
        {
            followsRule = true;
        }
        else if(cardValue == 8 || cardValue == 9 || cardValue == 10)
        {
            for (int i = 0; i < dropZone.transform.childCount; i++)
            {
                if (cardValue == dropZone.transform.GetChild(i).GetComponent<CardValues>().GetValue())
                {
                    followsRule = false;
                }
            }
        }
        return followsRule;
    }

    IEnumerator TookLastCard()
    {
        yield return new WaitForSeconds(0.4f);
        tookLastCard = true;
    }

    [Command]
    private void CmdTookCardLast() => RpcTookCardLast();

    //Resets everybody to false
    [ClientRpc]
    private void RpcTookCardLast()
    {
        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        pm.tookLastCard = false;
    }

    public void CheckRound() => StartCoroutine(CheckRoundEnd());

    //If no player has cards call to deal new hands
    IEnumerator CheckRoundEnd()
    {
        playerTurn.text = "";
        yield return new WaitForSeconds(0.5f);
        if(playerArea.transform.childCount == 0 && opponentArea.transform.childCount == 0)
        { 
            if (gameManager.round == 6)
            {
                CmdResetDeck();
                CmdEndGame();
            }
            else 
            { 
                CmdNextRound();  
            }
        }
        else { yield break; }
    }
    
    [Command]
    private void CmdResetDeck()
    {
        List<GameObject> cardDeck = FindObjectOfType<CardDeck>().getCurrentDeck();
        List<GameObject> fullDeck = FindObjectOfType<CardDeck>().getFullDeck();
        foreach (GameObject card in fullDeck)
        {
            cardDeck.Add(card);
        }
    }

    public void ChangeGameState(string state)
    {
        CmdChangeGameState(state);
    }

    [Command]
    private void CmdChangeGameState(string state)
    {
        RpcChangeGameState(state);
    }

    [Command] //NEED TO FIX THIS (Cleared Cards before they were scored?)
    private void CmdEndGame()
    {
        //RpcTakeFinalCards();
        RpcChangeGameState("Evaluate");
        RpcScoreGame();
        RpcChangeTurn();
        RpcChangeGameState("InitialDeal");
    }

    public void ClearCards()
    {
        for (int i = 0; i < playerTakenCards.transform.childCount; i++)
        {
            GameObject card = playerTakenCards.transform.GetChild(i).gameObject;
            Destroy(card);
        }
        for (int i = 0; i < opponentTakenCards.transform.childCount; i++)
        {
            GameObject card = opponentTakenCards.transform.GetChild(i).gameObject;
            Destroy(card);
        }
        for(int i = 0; i < dropZone.transform.childCount; i++)
        {
            GameObject card = dropZone.transform.GetChild(i).gameObject;
            Destroy(card);
        }
    }

    /*[ClientRpc]
    private void RpcTakeFinalCards()
    {
        StartCoroutine(TakeFinalCards());
    }*/

   /* IEnumerator TakeFinalCards()
    {
        yield return new WaitForSeconds(1f);
        if (tookLastCard)
        {
            for (int i = dropZone.transform.childCount - 1; i >= 0; i--)
            {
                GameObject card = dropZone.transform.GetChild(i).gameObject;
                card.transform.SetParent(playerTakenCards.transform, false);
                card.GetComponent<CardFlipper>().Flip();
            }
        }
        else
        {
            for (int i = dropZone.transform.childCount - 1; i >= 0; i--)
            {
                GameObject card = dropZone.transform.GetChild(i).gameObject;
                card.transform.SetParent(opponentTakenCards.transform, false);
                card.GetComponent<CardFlipper>().Flip();
            }
        }
    }*/

    [ClientRpc]
    private void RpcScoreGame() => StartCoroutine(ScoreGame());

    IEnumerator ScoreGame()
    {
        yield return new WaitForSeconds(2f);
        gameScorer.ScoreGame();
    }

    [Command]
    private void CmdNextRound() => RpcChangeGameState("PlayerDeal");

    //Calls to change all players turns
    [Command]
    void CmdChangeTurn()
    {
        RpcChangeTurn();
    }

    //Calls to show taken card
    [Command]
    void CmdTakeCard(GameObject card, string type) => RpcShowCard(card, type);

    [Server] //Server does something
    public override void OnStartServer()
    {
        //List<GameObject> fullCardDeck = cardDeck;
    }

    //Calls to draw cards and signals a player is ready
    [Command] 
    public void CmdDrawCards() //Method must start with Cmd
    {
        CardDeck cardDeck = FindObjectOfType<CardDeck>();
        for (int i = 0; i < 3; i++)
        {
            List<GameObject> currentDeck = cardDeck.getCurrentDeck();
            int cardDrawn = UnityEngine.Random.Range(0, currentDeck.Count);
            GameObject playerCard = Instantiate(currentDeck[cardDrawn], new Vector2(0, 0), Quaternion.identity);
            cardDeck.removeCard(cardDrawn);
            NetworkServer.Spawn(playerCard, connectionToClient);
            RpcShowCard(playerCard, "Draw");
        }
        RpcPlayerReady();
    }

    //Signals to all clients that a player is ready
    [ClientRpc]
    void RpcPlayerReady() => gameManager.PlayerReady();

    //Calls to deal cards to the center, changes the game state
    [Command]
    public void CmdDealCards()
    {
        CardDeck cardDeck = FindObjectOfType<CardDeck>();
        for (int i = 0; i < 4; i++)
        {
            List<GameObject> currentDeck = cardDeck.getCurrentDeck();
            int cardDrawn = UnityEngine.Random.Range(0, currentDeck.Count);
            GameObject playerCard = Instantiate(currentDeck[cardDrawn], new Vector2(0, 0), Quaternion.identity);
            cardDeck.removeCard(cardDrawn);
            NetworkServer.Spawn(playerCard, connectionToClient);
            RpcShowCard(playerCard, "Deal");
        }
        RpcChangeGameState("PlayerDeal");     
    }

    //Signals the game state to all clients
    [ClientRpc]
    void RpcChangeGameState(string state) => gameManager.ChangeGameState(state);

    public void PlayCard(GameObject card)
    {
        selectedCard = null;
        CmdPlayCard(card);
    }

    [Command]
    void CmdPlayCard(GameObject card)
    {
        RpcShowCard(card, "Play");
        RpcChangeTurn();
    }

    [ClientRpc]
    void RpcChangeTurn()
    {
        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        pm.isMyTurn = !pm.isMyTurn;
        pm.clearButton.interactable = !pm.clearButton.interactable;
        if (pm.isMyTurn)
        {
            playerTurn.text = "Your Turn";
        }
        else
        {
            playerTurn.text = "Opponent's Turn";
        }
    }

    [ClientRpc] //Rpc = remote procedure call
    void RpcShowCard(GameObject card, string type) //Function must start with Rpc
    {
        switch (type)
        {
            case "Draw":
                if (hasAuthority)
                {
                    card.transform.SetParent(playerArea.transform, false);
                }
                else
                {
                    card.transform.SetParent(opponentArea.transform, false);
                    card.GetComponent<CardFlipper>().Flip();
                }
                break;

            case "Deal":
                card.transform.SetParent(dropZone.transform, false);
                card.GetComponent<DragDrop>().NotDraggable();
                break;

            case "Play":
                card.transform.SetParent(dropZone.transform, false);
                card.GetComponent<DragDrop>().Deselect();
                if (!hasAuthority)
                {
                    card.GetComponent<CardFlipper>().Flip();
                }
                
                break;

            case "Taken":
                if (hasAuthority)
                {
                    card.transform.SetParent(playerTakenCards.transform, false);
                    card.GetComponent<CardFlipper>().Flip();
                }
                else
                {
                    card.transform.SetParent(opponentTakenCards.transform, false);
                    card.GetComponent<CardFlipper>().Flip();
                }
                break;
            case "Scopa":
                if (hasAuthority)
                {
                    card.transform.SetParent(playerTakenCards.transform, false);
                    gameScorer.playerScopa();
                }
                else
                {
                    card.transform.SetParent(opponentTakenCards.transform, false);
                    card.GetComponent<CardFlipper>().Flip();
                    gameScorer.opponentScopa();
                }
                break;

            case "Used":
                if (hasAuthority)
                {
                    card.transform.SetParent(playerTakenCards.transform, false);
                    card.GetComponent<CardFlipper>().Flip();
                }
                else
                {
                    card.transform.SetParent(opponentTakenCards.transform, false);
                }
                break;

            default:
                break;
        }
    }
}
