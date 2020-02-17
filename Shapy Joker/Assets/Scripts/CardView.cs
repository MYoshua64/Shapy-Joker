using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] string attachedCardID;
    GameManager gm;
    public Vector2 originalPos { get; private set; }
    public static Transform tableCardsParent;
    public CardData attachedCard { get; private set; }
    Collider2D cardCollider2D;
    bool selected = false;
    Hand opponentHand;

    private void Init()
    {
        gm = FindObjectOfType<GameManager>();
        tableCardsParent = transform.parent;
        originalPos = transform.localPosition;
        cardCollider2D = GetComponent<Collider2D>();
    }

    //private void Update()
    //{
    //    if (Input.touchCount > 0)
    //    {
    //        Touch playerTouch = Input.GetTouch(0);
    //        selected = playerTouch.phase == TouchPhase.Began && TouchInColliderBounds(playerTouch) && !selected;
    //    }
    //}

    private void OnMouseDown()
    {
        if (!FindObjectOfType<GameManager>().isPlayerTurn) return;
        HandleSelected();
    }

    bool TouchInColliderBounds(Touch touch)
    {
        Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
        return cardCollider2D.bounds.Contains(touchPos);
    }

    public void AttachCardData(CardData cardData)
    {
        attachedCard = cardData;
        attachedCardID = attachedCard.id;
        switch (attachedCard.color)
        {
            case CardColor.Yellow:
                GetComponent<SpriteRenderer>().color = Color.yellow;
                break;
            case CardColor.Blue:
                GetComponent<SpriteRenderer>().color = Color.blue;
                break;
            case CardColor.Red:
                GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case CardColor.Green:
                GetComponent<SpriteRenderer>().color = Color.green;
                break;
        }
        Init();
    }

    void HandleSelected()
    {
        if (selected)
        {
            gm.activeHand.RemoveFromHand(attachedCard);
            transform.SetParent(tableCardsParent);
            transform.localPosition = originalPos;
        }
        else if (gm.activeHand.GetCardAmountInHand() < gm.activeHand.maximumCards)
        {
            gm.activeHand.AddToHand(attachedCard);
            transform.SetParent(gm.activeHand.transform);
        }
        else return;
        selected = !selected;
    }
}
