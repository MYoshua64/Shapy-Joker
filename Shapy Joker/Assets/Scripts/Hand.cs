using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<RectTransform> cardSlots = new List<RectTransform>();
    List<CardData> cardsInHand = new List<CardData>();
    public int maximumCards { get; private set; } = 5;
    Group attachedSet;

    private void Awake()
    {
        GetChildSlots();
    }

    private void GetChildSlots()
    {
        RectTransform[] rawComponentArray = GetComponentsInChildren<RectTransform>();
        cardSlots.AddRange(rawComponentArray);
        cardSlots.Remove(GetComponent<RectTransform>());
    }

    public RectTransform FindNextOpenSlot()
    {
        RectTransform openSlot = null;
        foreach (RectTransform slot in cardSlots)
        {
            if (slot.childCount == 0)
            {
                openSlot = slot;
                break;
            }
        }
        return openSlot;
    }

    public void AddToHand(CardVisual newCard)
    {
        newCard.HandleSelected();
        cardsInHand.Add(newCard.attachedCard);
        CheckAttachedSetValidity();
    }

    public void RemoveFromHandAt(int index)
    {
        //if the index is in the set's bound...
        if (index < cardsInHand.Count)
        {
            //Remove the card in given index and returns it to its place
            cardsInHand[index].cardView.HandleSelected();
            cardsInHand.RemoveAt(index);
            //And checkes the validity of the set without it
            CheckAttachedSetValidity();
        }
    }

    public void ClearHand()
    {
        //Clears the currently held set and returns cards to their places
        foreach (CardData card in cardsInHand)
        {
            card.cardView.HandleSelected();
        }
        cardsInHand.Clear();
    }

    public void RemoveFromHand(CardVisual cardToRemove, bool submitting = false)
    {
        if (!cardsInHand.Contains(cardToRemove.attachedCard)) return;
        if (!submitting) cardToRemove.HandleSelected();
        cardsInHand.Remove(cardToRemove.attachedCard);
        CheckAttachedSetValidity();
    }

    public int GetCardAmountInHand()
    {
        return cardsInHand.Count;
    }

    void CheckAttachedSetValidity()
    {
        attachedSet = new Group(cardsInHand);
        attachedSet.CheckSetValidityBySequence();
        if (GameManager.isPlayerTurn)
            Blackboard.gm.SetSubmitButtonInteractable(attachedSet.isSetValid);
        else
            Blackboard.opponent.IsSetValid(attachedSet.isSetValid);
    }

    public void RefreshCardsPositions()
    {
        for (int i = 1; i < cardSlots.Count; i++)
        {
            if (cardSlots[i].transform.childCount == 1)
            {
                if (cardSlots[i - 1].transform.childCount == 0)
                {
                    iTween.MoveTo(cardSlots[i].transform.GetChild(0).gameObject, cardSlots[i - 1].transform.position, 0.75f);
                    cardSlots[i].transform.GetChild(0).SetParent(cardSlots[i - 1].transform);
                }
            }
        }
    }

    public void Print()
    {
        foreach (CardData card in cardsInHand)
        {
            card.Print();
        }
    }
}
