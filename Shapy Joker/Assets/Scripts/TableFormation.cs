using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableFormation : MonoBehaviour
{
    public List<CardVisual> cardsOnTable { get; private set; } = new List<CardVisual>();

    private void Awake()
    {
        Blackboard.tableCardsParent = this;
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
