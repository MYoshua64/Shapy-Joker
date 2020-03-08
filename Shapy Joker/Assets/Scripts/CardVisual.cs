using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    [SerializeField] string attachedCardID;
    public Vector3 originalPos { get; private set; }
    public CardData attachedCard { get; private set; }
    public int attachedCardIndex;
    Collider2D cardCollider2D;
    bool selected = false;
    bool submitted = false;

    private void Init()
    {
        attachedCardIndex = attachedCard.index;
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
        if (!GameManager.isPlayerTurn || GameManager.gamePaused || GameManager.isGameOver) return;
        if (selected)
        {
            Blackboard.gm.activeHand.RemoveFromHand(this);
        }
        else if (Blackboard.gm.activeHand.GetCardAmountInHand() < Blackboard.gm.activeHand.maximumCards)
        {
            Blackboard.gm.activeHand.AddToHand(this);
        }
    }

    bool TouchInColliderBounds(Touch touch)
    {
        Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
        return cardCollider2D.bounds.Contains(touchPos);
    }

    /// <summary>
    /// This function takes card data and attaches it to the view
    /// </summary>
    /// <param name="cardData">The card data to attach to the view</param>
    public void AttachCardData(CardData cardData)
    {
        attachedCard = cardData;
        attachedCard.SetCardView(this);
        attachedCardID = attachedCard.id;
        name = attachedCard.id;
        int spriteIndex = attachedCard.index < 80 ? attachedCard.index : 80 + (attachedCard.index % 80) / 2;
        GetComponent<Image>().sprite = CanvasManager.visibleSprites[spriteIndex];
        Init();
    }

    public void HandleSelected()
    {
        if (selected && !submitted)
        {
            transform.SetParent(Blackboard.tableCardsParent.transform);
            iTween.MoveTo(gameObject, originalPos, 0.75f);
            Blackboard.gm.activeHand.RefreshCardsPositions();
        }
        else if (Blackboard.gm.activeHand.GetCardAmountInHand() < Blackboard.gm.activeHand.maximumCards)
        {
            RectTransform openSlot = Blackboard.gm.activeHand.FindNextOpenSlot();
            if (openSlot)
            {
                transform.SetParent(openSlot);
                iTween.MoveTo(gameObject, openSlot.position, 0.75f);
            }
        }
        else return;
        selected = !selected;
    }

    public void SetSubmitted()
    {
        submitted = true;
    }

    public void SetOriginalPosition(Vector3 originalPos)
    {
        this.originalPos = originalPos;
    }
}
