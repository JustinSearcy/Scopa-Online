using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class DragDrop : NetworkBehaviour
{
    public GameObject canvas;
    public GameObject DropZone;
    public GameObject playerArea;
    public PlayerManager playerManager;

    private bool isDragging = false;
    private bool isOverDropZone = false;
    private bool isDraggable = true;
    [SerializeField] bool isSelected = false; //Serialized For Debug

    private GameObject startParent;
    private GameObject dropZone;
    
    private Vector3 startPosition;
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        canvas = GameObject.Find("Main Canvas");
        DropZone = GameObject.Find("Drop Zone");
        playerArea = GameObject.Find("Player Area");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        if (!hasAuthority)
        {
            isDraggable = false;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            transform.SetParent(canvas.transform, true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isOverDropZone = true;
        dropZone = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverDropZone = false;
        dropZone = null;
    }

    public void StartDrag()
    {
        if (!isDraggable) { return; }
        else if (!playerManager.isMyTurn) { return; }
        startPosition = transform.position;
        startParent = transform.parent.gameObject;
        isDragging = true;
    }

    public void EndDrag()
    {
        if (!isDraggable) { return; }
        else if (!playerManager.isMyTurn) { return; }
        isDragging = false;
        if (isOverDropZone && playerManager.isMyTurn)
        {
            transform.SetParent(dropZone.transform, false);
            isDraggable = false;
            resetColor();
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.PlayCard(gameObject);
            playerManager.ClearSelectedCards();
            playerManager.CheckRound();
            DeselectAll();
        }
        else
        {
            transform.position = startPosition;
            transform.SetParent(startParent.transform, false);
        }
    }

    public void DeselectAll()
    {
        GameObject dropArea = GameObject.Find("Drop Zone");
        for (int i = 0; i < dropArea.transform.childCount; i++)
        {
            GameObject card = dropArea.transform.GetChild(i).gameObject;
            if (card.GetComponent<DragDrop>().isSelected)
            {
                card.GetComponent<DragDrop>().Deselect();
            }
        }
    }

    public void NotDraggable() { isDraggable = false; }

    public void MouseClick()
    {
        if (!isSelected && playerManager.isMyTurn)
        {
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            if (gameObject.transform.IsChildOf(playerArea.transform))
            {
                isSelected = true;
                playerManager.SetSelectedCard(gameObject);
                ChangeColor();
            }
            else if (gameObject.transform.IsChildOf(dropZone.transform))
            {
                if (playerManager.HasSelectCard())
                {
                    isSelected = true;
                    ChangeColor();
                    playerManager.PickUpCard(gameObject);
                }
            }
            else { return; } 
        }
    }

    private void ChangeColor()
    {
        for (int i = 0; i < playerArea.transform.childCount; i++)
        {
            GameObject card = playerArea.transform.GetChild(i).gameObject;
            if (!card.GetComponent<DragDrop>().IsSelected())
            {
                card.transform.GetChild(0).GetComponent<Image>().color = new Color32(138, 138, 138, 255);
            }
            else
            {
                card.transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
        }

        GameObject dropArea = GameObject.Find("Drop Zone");
        for (int i = 0; i < dropArea.transform.childCount; i++)
        {
            GameObject centerCard = dropArea.transform.GetChild(i).gameObject;
            if (!centerCard.GetComponent<DragDrop>().IsSelected())
            {
                centerCard.transform.GetChild(0).GetComponent<Image>().color = new Color32(138, 138, 138, 255);
            }
            else
            {
                centerCard.transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
        }
    }

    public void resetColor()
    {
        for (int i = 0; i < playerArea.transform.childCount; i++)
        {
            if (playerArea.transform.GetChild(i) != null)
            {
                GameObject card = playerArea.transform.GetChild(i).gameObject;
                card.transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
        }
        GameObject dropArea = GameObject.Find("Drop Zone");
        for (int i = 0; i < dropArea.transform.childCount; i++)
        {
            if(dropArea.transform.GetChild(i) != null)
            {
                GameObject centerCard = dropArea.transform.GetChild(i).gameObject;
                centerCard.transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }   
        }
    }

    public void Deselect() { isSelected = false; }

    public bool IsSelected() { return isSelected; }
}
