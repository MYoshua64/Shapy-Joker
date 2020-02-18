using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    List<CardData> cardsInHand = new List<CardData>();
    public int maximumCards { get; private set; } = 5;
    Set attachedSet;

    public void AddToHand(CardView newCard)
    {
        newCard.HandleSelected();
        cardsInHand.Add(newCard.attachedCard);
        CheckAttachedSetValidity();
    }

    public void RemoveFromHandAt(int index)
    {
        if (index < cardsInHand.Count)
        {
            cardsInHand.RemoveAt(index);
            CheckAttachedSetValidity();
        }
    }

    public void RemoveFromHand(CardView cardToRemove)
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
