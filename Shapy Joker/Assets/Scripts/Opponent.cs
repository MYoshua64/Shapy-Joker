using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    Hand myHand;
    List<CardVisual> cardsOnScreen = new List<CardVisual>();
    List<List<CardData>> possibleGroups;
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
        isMySetValid = false;
        possibleGroups = new List<List<CardData>>();
        int cardIndex = -1;
        int cap = 0;
        do
        {
            cardIndex++;
            myHand.ClearHand();
            if (cardIndex >= cardsOnScreen.Count) break;
            List<CardVisual> potentialCards = new List<CardVisual>();
            CardVisual inspectedCard;
            do
            {
                //Picks a card from all 20 cards on the table
                inspectedCard = cardsOnScreen[cardIndex];
                //Tries to find cards that have at least TWO shared attributes
                potentialCards = FindMatchesIn(inspectedCard.attachedCard, cardsOnScreen);
            } while (potentialCards.Count < 2);
            myHand.AddToHand(inspectedCard);
            while (GameManager.gamePaused)
            {
                yield return null;
            }
            List<CardVisual> matches = new List<CardVisual>();
            matches.AddRange(potentialCards);
            int index = -1;
            do
            {
                index++;
                //Goes through the generated list of matching cards
                inspectedCard = matches[index];
                //Tries to find more matching cards to create a set
                potentialCards = FindMatchesIn(inspectedCard.attachedCard, matches);
            } while (potentialCards.Count < 1 && index < potentialCards.Count);
            myHand.AddToHand(inspectedCard);
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
            cap++;
            if (cap > cardsOnScreen.Count) break;
        } while (!isMySetValid);
        if (!isMySetValid)
        {
            myHand.ClearHand();
            Blackboard.gm.HandleTurnEnd();
            yield break;
        }
        string neededCard = CalculateNeededCard();
        SearchForCardWithID(neededCard);
        if (myHand.cardsInHand.Count > 3)
        {
            neededCard = CalculateNeededCard();
            SearchForCardWithID(neededCard);
        }
        foreach (CardData card in myHand.cardsInHand)
        {
            myHand.AddToHand(card.cardView, true);
            yield return new WaitForSeconds(0.75f);
        }
        yield return new WaitForSeconds(0.5f);
        while (GameManager.gamePaused)
        {
            yield return null;
        }
        Blackboard.gm.SubmitSet();
    }

    private void SearchForCardWithID(string neededCard)
    {
        string neededJoker = neededCard[0] + "J";
        for (int i = 0; i < cardsOnScreen.Count; i++)
        {
            if (cardsOnScreen[i].attachedCard.id == neededCard || (!myHand.ContainsJoker() && cardsOnScreen[i].attachedCard.id == neededJoker))
            {
                myHand.AddToHand(cardsOnScreen[i]);
                return;
            }
        }
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

    string CalculateNeededCard()
    {
        GroupType myHandGroupType = myHand.GetHandGroupType();
        string neededCardString = "";
        switch (myHandGroupType)
        {
            case GroupType.NumberColor:
                neededCardString += ChooseNeededCardColor(false) + ChooseNeededCardShape() + ChooseNeededCardNumber(false);
                break;
            case GroupType.ShapeNumber:
                neededCardString += ChooseNeededCardColor() + ChooseNeededCardShape(false) + ChooseNeededCardNumber(false);
                break;
            case GroupType.ShapeColorCons:
                neededCardString += ChooseNeededCardColor(false) + ChooseNeededCardShape(false) + ChooseNeededCardNumber();
                break;
            case GroupType.ColorCons:
                neededCardString += ChooseNeededCardColor(false) + ChooseNeededCardShape() + ChooseNeededCardNumber();
                break;
            case GroupType.ShapeCons:
                neededCardString += ChooseNeededCardColor() + ChooseNeededCardShape(false) + ChooseNeededCardNumber();
                break;
        }
        return neededCardString;
    }

    private string ChooseNeededCardNumber(bool different = true)
    {
        if (!different)
        {
            if (!myHand.cardsInHand[myHand.cardsInHand.Count - 1].jokerCard)
                return myHand.cardsInHand[myHand.cardsInHand.Count - 1].id[2].ToString();
            else
                return myHand.cardsInHand[myHand.cardsInHand.Count - 2].id[2].ToString();
        }
        if (!myHand.cardsInHand[myHand.cardsInHand.Count - 1].jokerCard)
        {
            if (myHand.cardsInHand[myHand.cardsInHand.Count - 1].number < 5)
                return (myHand.cardsInHand[myHand.cardsInHand.Count - 1].number + 1).ToString();
            else if (myHand.cardsInHand[0].number > 1)
                return (myHand.cardsInHand[0].number - 1).ToString();
        }
        else
        {
            if (myHand.cardsInHand[myHand.cardsInHand.Count - 2].number < 4)
                return (myHand.cardsInHand[myHand.cardsInHand.Count - 2].number + 2).ToString();
            else if (myHand.cardsInHand[0].number > 1)
                return (myHand.cardsInHand[0].number - 1).ToString();
        }
        return "";
    }

    private string ChooseNeededCardShape(bool different = true)
    {
        if (!different)
        {
            if (!myHand.cardsInHand[myHand.cardsInHand.Count - 1].jokerCard)
                return myHand.cardsInHand[myHand.cardsInHand.Count - 1].id[1].ToString();
            else
                return myHand.cardsInHand[myHand.cardsInHand.Count - 2].id[1].ToString();
        }
        CardShape selectedShape = CardShape.Joker;
        bool match = true;
        for (int i = 0; i < 4 && match; i++)
        {
            match = true;
            selectedShape = (CardShape)i;
            foreach (CardData card in myHand.cardsInHand)
            {
                match = card.shape == selectedShape;
                if (match) break;
            }
        }
        switch (selectedShape)
        {
            case CardShape.Star:
                return "S";
            case CardShape.Rectangle:
                return "R";
            case CardShape.Triangle:
                return "T";
            case CardShape.Circle:
                return "C";
            case CardShape.Joker:
                return "J";
            default:
                return "";
        }
    }

    private string ChooseNeededCardColor(bool different = true)
    {
        if (!different)
        {
            return myHand.cardsInHand[myHand.cardsInHand.Count - 1].id[0].ToString();
        }
        CardColor selectedColor = CardColor.Blue;
        bool match = true;
        for (int i = 0; i < 4 && match; i++)
        {
            match = true;
            selectedColor = (CardColor)i;
            foreach (CardData card in myHand.cardsInHand)
            {
                match = card.color == selectedColor;
                if (match) break;
            }
        }
        switch (selectedColor)
        {
            case CardColor.Yellow:
                return "Y";
            case CardColor.Blue:
                return "B";
            case CardColor.Red:
                return "R";
            case CardColor.Green:
                return "G";
            default:
                return "";
        }
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
