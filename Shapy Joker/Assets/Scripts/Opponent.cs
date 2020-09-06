using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    Hand myHand;
    List<CardVisual> cardsOnScreen = new List<CardVisual>();
    public bool isMySetValid { get; private set; } = false;

    private void Awake()
    {
        Blackboard.opponent = this;
        myHand = Blackboard.gm.opponentHand;
    }

    public void StartTurn()
    {
        cardsOnScreen = Blackboard.tableCardsParent.GetCardsOnTable();
        StartCoroutine(SearchForSets());
    }

    IEnumerator SearchForSets()
    {
        while (GameManager.gamePaused)
        {
            yield return null;
        }
        isMySetValid = false;
        int cardIndex = 0, firstIndex = 0;
        do
        {
            myHand.ClearHand();
            List<CardVisual> potentialCards = new List<CardVisual>();
            if (cardIndex >= cardsOnScreen.Count)
            {
                Debug.LogError("No sets found! I might be blind though, because my programmer is too dumb to make me right!");
                Blackboard.deckBuilder.ReshuffleDeck();
                cardIndex = 0;
            }
            CardVisual inspectedCard = cardsOnScreen[cardIndex];
            if (inspectedCard.attachedCard.jokerCard)
            {
                cardIndex++;
                continue;
            }
            cardIndex++;
            potentialCards = Blackboard.gm.FindMatchesIn(inspectedCard.attachedCard, cardsOnScreen);
            myHand.AddToHand(inspectedCard);
            while (GameManager.gamePaused)
            {
                yield return null;
            }
            firstIndex = 0;
            do
            {
                if (firstIndex >= potentialCards.Count) break;
                if (myHand.cardsInHand.Count > 1)
                {
                    myHand.RemoveFromHand(inspectedCard, default, true);
                }
                inspectedCard = potentialCards[firstIndex];
                if (inspectedCard.attachedCard.jokerCard)
                {
                    firstIndex++;
                    continue;
                }
                myHand.AddToHand(inspectedCard);
                while (GameManager.gamePaused)
                {
                    yield return null;
                }
                string[] neededCard = Blackboard.gm.CalculateNeededCard();
                SearchForCardWithID(neededCard);
                firstIndex++;
            } while (!isMySetValid);
        } while (!isMySetValid);
        while (GameManager.gamePaused)
        {
            yield return null;
        }
        string[] extraCard = Blackboard.gm.CalculateNeededCard(myHand.GetHandGroupType());
        SearchForCardWithID(extraCard);
        foreach (CardData card in myHand.cardsInHand)
        {
            myHand.AddToHand(card.cardView, true);
            while (GameManager.gamePaused)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.75f);
        }
        yield return new WaitForSeconds(0.5f);
        while (GameManager.gamePaused)
        {
            yield return null;
        }
        Debug.LogWarning("Submitted");
        while (GameManager.gamePaused)
        {
            yield return null;
        }
        Blackboard.gm.SubmitSet(myHand);
    }

    void SearchForCardWithID(string[] neededCard)
    {
        string neededJoker = neededCard[0] + "J";
        for (int i = 0; i < cardsOnScreen.Count; i++)
        {
            if (ThisCardIs(cardsOnScreen[i], neededCard) || (!myHand.ContainsJoker() && ThisCardIs(cardsOnScreen[i], neededJoker)))
            {
                myHand.AddToHand(cardsOnScreen[i]);
                if (!isMySetValid) myHand.RemoveFromHand(cardsOnScreen[i], default, true);
                return;
            }
        }
    }

    private bool ThisCardIs(CardVisual cardVisual, string[] neededCard)
    {
        string cardId = cardVisual.attachedCard.id;
        return neededCard[0].Contains(cardId[0].ToString()) && neededCard[1].Contains(cardId[1].ToString()) && neededCard[2].Contains(cardId[2].ToString());
    }

    private bool ThisCardIs(CardVisual cardVisual, string neededCard)
    {
        string cardId = cardVisual.attachedCard.id;
        return neededCard.Contains(cardId[0].ToString()) && neededCard[neededCard.Length - 1] == cardId[1];
    }
    
    public void ConfirmIfSetValid(bool value)
    {
        isMySetValid = value;
    }
}
