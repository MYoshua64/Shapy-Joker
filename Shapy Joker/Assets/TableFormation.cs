using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableFormation : MonoBehaviour
{
    List<CardVisual> cardsOnTable = new List<CardVisual>();

    private void Awake()
    {
        cardsOnTable.AddRange(GetComponentsInChildren<CardVisual>());
    }

    public void AddToFormation(CardVisual newCard)
    {
        cardsOnTable.Add(newCard);
    }

    public void RemoveFromFormation(CardVisual cardToRemove)
    {
        if (!cardsOnTable.Contains(cardToRemove)) return;
        cardsOnTable.Remove(cardToRemove);
    }

    public List<CardVisual> GetCardsOnTable()
    {
        return cardsOnTable;
    }
}
