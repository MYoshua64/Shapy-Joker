using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    Hand myHand;
    CardView[] cardsOnScreen;
    bool isMySetValid = false;

    public void StartTurn()
    {
        myHand = FindObjectOfType<GameManager>().opponentHand;
        cardsOnScreen = FindObjectsOfType<CardView>();
        SearchForSets();
    }

    void SearchForSets()
    {
        myHand.RemoveFromHandAt(0);
        List<CardView> potentialCards = new List<CardView>();
        CardView inspectedCard;
        int randIndex = 0;
        do
        {
            randIndex = Random.Range(0, cardsOnScreen.Length);
            inspectedCard = cardsOnScreen[randIndex];
            potentialCards = FindMatchesIn(inspectedCard.attachedCard, cardsOnScreen);
        } while (potentialCards.Count < 2);
        int index = -1;
        myHand.AddToHand(inspectedCard);
        CardView checkedCard;
        do
        {
            index++;
            checkedCard = potentialCards[index];
            potentialCards = FindMatchesIn(checkedCard.attachedCard, potentialCards);
        } while (potentialCards.Count < 1 && index < potentialCards.Count);
        myHand.AddToHand(checkedCard);
        for (index = 0; index < potentialCards.Count && !isMySetValid; index++)
        {
            myHand.RemoveFromHandAt(2);
            myHand.AddToHand(potentialCards[index]);

        }
        myHand.Print();
    }

    List<CardView> FindMatchesIn(CardData inspectedCard, ICollection collection)
    {
        List<CardView> potentialCards = new List<CardView>();
        foreach (CardView comparedCard in collection)
        {
            if (comparedCard.attachedCard == inspectedCard) continue;
            int matches = 0;
            matches += Compare(comparedCard.attachedCard.color, inspectedCard.color) + Compare(comparedCard.attachedCard.shape, inspectedCard.shape) +
                Compare(comparedCard.attachedCard.number, inspectedCard.number);
            if (matches >= 2 || IsThereJoker(comparedCard.attachedCard, inspectedCard)) potentialCards.Add(comparedCard);
        }
        return potentialCards;
    }

    int Compare(object a, object b)
    {
        return a.Equals(b) ? 1 : 0;
    }

    bool IsThereJoker(CardData a, CardData b)
    {
        return a.jokerCard || b.jokerCard;
    }

    int Compare(int a, int b)
    {
        return Mathf.Abs(a - b) <= 2 ? 1 : 0;
    }

    public void IsSetValid(bool value)
    {
        isMySetValid = value;
    }
}
