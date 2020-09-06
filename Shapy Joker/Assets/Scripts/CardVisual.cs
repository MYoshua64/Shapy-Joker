using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    [SerializeField] string attachedCardID;
    public Vector3 originalPos;
    public CardData attachedCard { get; private set; }
    public int attachedCardIndex;
    Collider2D cardCollider2D;
    public bool selected { get; private set; } = false;
    bool pickable = true;
    bool submitted = false;
    RectTransform slot;
    Image cardSprite;

    private void Init()
    {
        attachedCardIndex = attachedCard.index;
        cardCollider2D = GetComponent<Collider2D>();
        cardSprite = GetComponent<Image>();
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
    public void HandleTouch()
    {
        if (!Blackboard.gm.allowCardPickUp || GameManager.gamePaused || Blackboard.gm.isGameOver || !pickable) return;
        Blackboard.gm.ResetSubmitButtonSprite();
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
    public void AttachCardData(CardData cardData, bool fromJoker = false)
    {
        attachedCard = cardData;
        attachedCard.SetCardView(this);
        attachedCardID = attachedCard.id;
        Init();
        if (!fromJoker)
        {
            name = attachedCard.id;
            int spriteIndex = attachedCard.index < 80 ? attachedCard.index : 80 + (attachedCard.index % 80) / 2;
            cardSprite.sprite = CanvasManager.visibleSprites[spriteIndex];
        }
    }

    public void HandleSelected()
    {
        if (selected)
        {
            slot.GetComponent<CardSlot>().open = true;
            if (!submitted)
            {
                transform.SetParent(Blackboard.tableCardsParent.transform);
                iTween.MoveTo(gameObject, iTween.Hash("position", originalPos, "time", 0.75f, "islocal", true,
                    "oncompletetarget", gameObject, "oncomplete", "ResetClickable"));
                slot.SetParent(null);
                Blackboard.sfxPlayer.PlaySFX(SFXType.CardPlace);
            }
            slot = null;
        }
        else if (Blackboard.gm.activeHand.GetCardAmountInHand() <= Blackboard.gm.activeHand.maximumCards)
        {
            slot = Blackboard.gm.activeHand.FindNextOpenSlot();
            if (slot)
            {
                slot.SetParent(Blackboard.gm.activeHand.transform);
                transform.SetAsLastSibling();
                UpdatePosition();
                slot.GetComponent<CardSlot>().open = false;
                Blackboard.sfxPlayer.PlaySFX(SFXType.CardTake);
            }
        }
        else return;
        pickable = false;
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

    public void UpdatePosition(iTween.EaseType easeType = iTween.EaseType.easeOutExpo)
    {
        StartCoroutine(MoveToSlot(Time.fixedDeltaTime, easeType));
    }

    IEnumerator MoveToSlot(float delayTime, iTween.EaseType easeType)
    {
        if (!slot) yield break;
        yield return new WaitForSeconds(delayTime);
        if (transform.position != slot.position) iTween.MoveTo(gameObject, iTween.Hash("position", slot.position, "time", 0.75f, "easetype", easeType));
        yield return new WaitForSeconds(0.75f);
        ResetClickable();
    }

    public void Print()
    {
        Debug.Log(attachedCard.id);
    }

    public void SetSprite(Sprite sprite)
    {
        cardSprite.sprite = sprite;
    }

    void ResetClickable()
    {
        pickable = true;
    }
}
