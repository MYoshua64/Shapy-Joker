using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    List<CardData> cardsInHand = new List<CardData>();
    public int maximumCards { get; private set; } = 5;
    Set attachedSet;

    public void AddToHand(CardData newCard)
    {
        cardsInHand.Add(newCard);
        CheckAttachedSetValidity();
    }

    public void RemoveFromHand(CardData cardToRemove)
    {
        cardsInHand.Remove(cardToRemove);
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
        FindObjectOfType<GameManager>().SetSubmitButtonInteractable(attachedSet.setValid);
    }
}
