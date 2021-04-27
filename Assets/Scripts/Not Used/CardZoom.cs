using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardZoom : MonoBehaviour
{
    [SerializeField] GameObject canvas;

    private GameObject zoomCard;

    public void Awake()
    {
        canvas = GameObject.Find("Main Canvas");
    }

    public void OnHoverEnter()
    {
        zoomCard = Instantiate(gameObject, new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + 325), Quaternion.identity);
        zoomCard.transform.SetParent(canvas.transform, true);

        zoomCard.GetComponent<BoxCollider2D>().enabled = false;
        RectTransform rect = zoomCard.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 412);
    }

    public void OnHoverExit()
    {
        Destroy(zoomCard);
    }
}
