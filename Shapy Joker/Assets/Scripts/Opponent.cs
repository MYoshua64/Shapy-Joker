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
        do
        {
            myHand.ClearHand();
            List<CardVisual> potentialCards = new List<CardVisual>();
            CardVisual inspectedCard;
            int randIndex = 0;
            do
            {
                //Picks a card at random from all 20 cards on the table
                randIndex = Random.Range(0, cardsOnScreen.Count);
                inspectedCard = cardsOnScreen[randIndex];
                //Tries to find cards that have at least TWO shared attributes
                potentialCards = FindMatchesIn(inspectedCard.attachedCard, cardsOnScreen);
            } while (potentialCards.Count < 2);
            myHand.AddToHand(inspectedCard);
            yield return new WaitForSeconds(0.75f);
            while (GameManager.gamePaused)
            {
                yield return null;
            }
            int index = -1;
            CardVisual checkedCard;
            do
            {
                index++;
                //Goes through the generated list of matching cards
                checkedCard = potentialCards[index];
                //Tries to find more matching cards to create a set
                potentialCards = FindMatchesIn(checkedCard.attachedCard, potentialCards);
            } while (potentialCards.Count < 1 && index < potentialCards.Count);
            myHand.AddToHand(checkedCard);
            yield return new WaitForSeconds(0.75f);
            while (GameManager.gamePaused)
            {
                yield return null;
            }
            for (index = 0; index < potentialCards.Count && !isMySetValid; index++)
            {
                //This goes through "trial and error" until it finds a set
                myHand.RemoveFromHandAt(2);
                myHand.AddToHand(potentialCards[index]);
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
        } while (!isMySetValid);
        myHand.Print();
        yield return new WaitForSeconds(1.25f);
        while (GameManager.gamePaused)
        {
            yield return null;
        }
        Blackboard.gm.SubmitSet();
    }


    List<CardVisual> FindMatchesIn(CardData inspectedCard, ICollection collection)
    {
        List<CardVisual> potentialCards = new List<CardVisual>();
        foreach (CardVisual comparedCard in collection)
        {
            if (comparedCard.attachedCard == inspectedCard) continue;
            int matches = 0;
            //Whenever there is a match in an attribute, the counter goes up by 1
            matches += Compare(comparedCard.attachedCard.color, inspectedCard.color) + 
                Compare(comparedCard.attachedCard.shape, inspectedCard.shape) +
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

    public void ConfirmIfSetValid(bool value)
    {
        isMySetValid = value;
    }
}
