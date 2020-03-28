using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardVisual : MonoBehaviour
{
    [SerializeField] string attachedCardID;
    public Vector3 originalPos { get; private set; }
    public CardData attachedCard { get; private set; }
    public int attachedCardIndex;
    Collider2D cardCollider2D;
    public bool selected { get; private set; } = false;
    bool submitted = false;
    RectTransform slot;
    public static int currentHighSortingOrder = 10;

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
        if (!Blackboard.gm.isPlayerTurn || GameManager.gamePaused || Blackboard.gm.isGameOver) return;
        if (selected)
        {
            Blackboard.gm.activeHand.RemoveFromHand(this);
        }
        else if (Blackboard.gm.activeHand.GetCardAmountInHand() < Blackboard.gm.activeHand.maximumCards)
        {
            Blackboard.gm.activeHand.AddToHand(this, true);
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
        GetComponent<SpriteRenderer>().sprite = CanvasManager.visibleSprites[spriteIndex];
        Init();
    }

    public void HandleSelected()
    {
        if (selected)
        {
            slot.GetComponent<CardSlot>().open = true;
            if (!submitted)
            {
                transform.SetParent(Blackboard.tableCardsParent.transform);
                iTween.MoveTo(gameObject, iTween.Hash("position", originalPos, "time", 0.75f, "oncompletetarget", gameObject, "oncomplete", "SetSortingOrder",
                    "oncompleteparams", 5));
                slot.SetParent(null);
                Blackboard.sfxPlayer.PlayCardSFX(false);
            }
        }
        else if (Blackboard.gm.activeHand.GetCardAmountInHand() <= Blackboard.gm.activeHand.maximumCards)
        {
            slot = Blackboard.gm.activeHand.FindNextOpenSlot();
            if (slot)
            {
                slot.SetParent(Blackboard.gm.activeHand.transform);
                SetSortingOrder(currentHighSortingOrder);
                currentHighSortingOrder++;
                UpdatePosition();
                slot.GetComponent<CardSlot>().open = false;
                Blackboard.sfxPlayer.PlayCardSFX(true);
            }
        }
        else return;
        selected = !selected;
    }

    void SetSortingOrder(int order)
    {
        GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    public void SetSubmitted()
    {
        submitted = true;
        slot.SetParent(null);
    }

    public void SetOriginalPosition(Vector3 originalPos)
    {
        this.originalPos = originalPos;
    }

    public void UpdatePosition()
    {
        Invoke("MoveToSlot", Time.fixedDeltaTime);
    }

    void MoveToSlot()
    {
        if (!slot) return;
        if (transform.position != slot.position) iTween.MoveTo(gameObject, slot.position, 0.75f);
    }
}
