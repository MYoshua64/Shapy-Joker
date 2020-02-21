using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    [SerializeField] string attachedCardID;
    GameManager gm;
    public Vector3 originalPos { get; private set; }
    public static TableFormation tableCardsParent;
    public CardData attachedCard { get; private set; }
    Collider2D cardCollider2D;
    bool selected = false;

    private void Init()
    {
        gm = FindObjectOfType<GameManager>();
        tableCardsParent = GetComponentInParent<TableFormation>();
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

    /// <summary>
    /// What happens when the player taps on a card
    /// </summary>
    private void OnMouseDown()
    {
        if (!FindObjectOfType<GameManager>().isPlayerTurn) return;
        if (selected)
        {
            gm.activeHand.RemoveFromHand(this);
        }
        else if (gm.activeHand.GetCardAmountInHand() < gm.activeHand.maximumCards)
        {
            gm.activeHand.AddToHand(this);
        }
    }

    bool TouchInColliderBounds(Touch touch)
    {
        Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
        return cardCollider2D.bounds.Contains(touchPos);
    }

    public void AttachCardData(CardData cardData)
    {
        attachedCard = cardData;
        attachedCard.SetCardView(this);
        attachedCardID = attachedCard.id;
        name = attachedCard.id;
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

    public void HandleSelected()
    {
        if (selected)
        {
            transform.SetParent(tableCardsParent.transform);
            iTween.MoveTo(gameObject, originalPos, 0.75f);
        }
        else if (gm.activeHand.GetCardAmountInHand() < gm.activeHand.maximumCards)
        {
            originalPos = transform.localPosition;
            RectTransform openSlot = gm.activeHand.FindNextOpenSlot();
            if (openSlot)
            {
                transform.SetParent(openSlot);
                iTween.MoveTo(gameObject, openSlot.position, 0.75f);
            }
        }
        else return;
        selected = !selected;
    }

    public void SetOriginalPosition(Vector3 originalPos)
    {
        this.originalPos = originalPos;
    }
}
