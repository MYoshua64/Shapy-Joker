using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<RectTransform> cardSlots = new List<RectTransform>();
    List<CardData> cardsInHand = new List<CardData>();
    public int maximumCards { get; private set; } = 5;
    Set attachedSet;

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

    public void RemoveFromHandAt(int index, bool clear = false)
    {
        if (clear)
        {
            //Clears the currently held set and returns cards to their places
            foreach (CardData card in cardsInHand)
            {
                card.cardView.HandleSelected();
            }
            cardsInHand.Clear();
            return;
        }
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

    public void RemoveFromHand(CardVisual cardToRemove)
    {
        if (!cardsInHand.Contains(cardToRemove.attachedCard)) return;
        cardToRemove.HandleSelected();
        cardsInHand.Remove(cardToRemove.attachedCard);
        CheckAttachedSetValidity();
    }

    public int GetCardAmountInHand()
    {
        return cardsInHand.Count;
    }

    void CheckAttachedSetValidity()
    {
        attachedSet = new Set(cardsInHand);
        attachedSet.CheckSetValidityBySequence();
        if (FindObjectOfType<GameManager>().isPlayerTurn)
            FindObjectOfType<GameManager>().SetSubmitButtonInteractable(attachedSet.isSetValid);
        else
            FindObjectOfType<Opponent>().IsSetValid(attachedSet.isSetValid);
    }

    public void Print()
    {
        foreach (CardData card in cardsInHand)
        {
            card.Print();
        }
    }
}
