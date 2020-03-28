using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<CardSlot> cardSlots = new List<CardSlot>();
    public List<CardData> cardsInHand { get; private set; } = new List<CardData>();
    public int maximumCards { get; private set; } = 5;
    Group attachedSet;

    public RectTransform FindNextOpenSlot()
    {
        foreach (CardSlot slot in cardSlots)
        {
            if (slot.open)
            {
                return slot.GetComponent<RectTransform>();
            }
        }
        return null;
    }

    public void AddToHand(CardVisual newCard, bool picking = false)
    {
        if (!cardsInHand.Contains(newCard.attachedCard))
        {
            cardsInHand.Add(newCard.attachedCard);
            CheckAttachedSetValidity();
        }
        if (!Blackboard.gm.isPlayerTurn)
        {
            if (cardsInHand.Count > 2)
            {
                if (!Blackboard.opponent.isMySetValid) return;
            }
        }
        foreach (CardData card in cardsInHand)
        {
            card.cardView.UpdatePosition();
        }
        if (picking)
            newCard.HandleSelected();
    }

    public void RemoveFromHandAt(int index)
    {
        //if the index is in the set's bound...
        if (index < cardsInHand.Count)
        {
            //Remove the card in given index and returns it to its place
            if (cardsInHand[index].cardView.selected && Blackboard.gm.isPlayerTurn)
                cardsInHand[index].cardView.HandleSelected();
            cardsInHand.RemoveAt(index);
            //And checkes the validity of the set without it
            CheckAttachedSetValidity();
            foreach (CardData card in cardsInHand)
            {
                card.cardView.UpdatePosition();
            }
        }
    }

    public void ClearHand()
    {
        //Clears the currently held set and returns cards to their places
        foreach (CardData card in cardsInHand)
        {
            if (card.cardView.selected)
                card.cardView.HandleSelected();
        }
        cardsInHand.Clear();
    }

    public void RemoveFromHand(CardVisual cardToRemove, bool submitting = false)
    {
        if (!cardsInHand.Contains(cardToRemove.attachedCard)) return;
        cardsInHand.Remove(cardToRemove.attachedCard);
        cardToRemove.HandleSelected();
        if (!submitting)
        {
            foreach (CardData card in cardsInHand)
            {
                card.cardView.UpdatePosition();
            }
        }
        CheckAttachedSetValidity(submitting);
    }

    public int GetCardAmountInHand()
    {
        return cardsInHand.Count;
    }

    void CheckAttachedSetValidity(bool submitting = false)
    {
        attachedSet = new Group(cardsInHand);
        attachedSet.CheckSetValidityBySequence();
        if (Blackboard.gm.isPlayerTurn && !submitting)
            Blackboard.gm.SetSubmitButtonInteractable(attachedSet.isSetValid);
        else
            Blackboard.opponent.ConfirmIfSetValid(attachedSet.isSetValid);
    }

    public GroupType GetHandGroupType()
    {
        return attachedSet.groupType;
    }

    public bool ContainsJoker()
    {
        foreach (CardData card in cardsInHand)
        {
            if (card.jokerCard) return true;
        }
        return false;
    }
}
