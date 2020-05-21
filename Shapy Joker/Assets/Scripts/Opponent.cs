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
        isMySetValid = false;
        int cardIndex = 0, firstIndex = 0;
        do
        {
            myHand.ClearHand();
            List<CardVisual> potentialCards = new List<CardVisual>();
            if (cardIndex >= cardsOnScreen.Count)
            {
                Debug.LogError("No sets found! I might be blind though, because my programmer is too dumb to make me right!");
                Blackboard.gm.HandleTurnEnd();
                yield break;
            }
            CardVisual inspectedCard = cardsOnScreen[cardIndex];
            if (inspectedCard.attachedCard.jokerCard)
            {
                cardIndex++;
                continue;
            }
            inspectedCard.Print();
            cardIndex++;
            potentialCards = FindMatchesIn(inspectedCard.attachedCard, cardsOnScreen);
            myHand.AddToHand(inspectedCard);
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
                inspectedCard.Print();
                myHand.AddToHand(inspectedCard);
                string neededCard = CalculateNeededCard();
                SearchForCardWithID(neededCard);
                firstIndex++;
            } while (!isMySetValid);
        } while (!isMySetValid);
        string extraCard = CalculateNeededCard(myHand.GetHandGroupType());
        SearchForCardWithID(extraCard);
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
        Debug.LogWarning("Submitted");
        Blackboard.gm.SubmitSet(myHand);
    }

    private void SearchForCardWithID(string neededCard)
    {
        string neededJoker = neededCard[0] + "J";
        for (int i = 0; i < cardsOnScreen.Count; i++)
        {
            if (cardsOnScreen[i].attachedCard.id == neededCard || (!myHand.ContainsJoker() && cardsOnScreen[i].attachedCard.id == neededJoker))
            {
                if (cardsOnScreen[i].attachedCard.jokerCard)
                {
                    CardData virtualCard = new CardData(neededCard);
                    cardsOnScreen[i].AttachCardData(virtualCard, true);
                }
                myHand.AddToHand(cardsOnScreen[i]);
                if (!isMySetValid) myHand.RemoveFromHand(cardsOnScreen[i], default, true);
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
                Compare(comparedCard.attachedCard.shape, inspectedCard.shape);
            if ((matches >= 1 && Compare(comparedCard.attachedCard.number, inspectedCard.number) == 1) || IsThereJoker(comparedCard.attachedCard, inspectedCard)) potentialCards.Add(comparedCard);
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
        return Mathf.Abs(a - b) <= 1 ? 1 : 0;
    }

    public void ConfirmIfSetValid(bool value)
    {
        isMySetValid = value;
    }

    string CalculateNeededCard(GroupType groupType = GroupType.None)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        switch (groupType)
        {
            case GroupType.None:
                sb.Append(CalculateNeededColor(myHand.cardsInHand[0].color != myHand.cardsInHand[1].color));
                sb.Append(CalculateNeededShape(myHand.cardsInHand[0].shape != myHand.cardsInHand[1].shape));
                sb.Append(CalculateNeededNumber(myHand.cardsInHand[0].number != myHand.cardsInHand[1].number));
                break;
            case GroupType.NumberColor:
                sb.Append(CalculateNeededColor());
                sb.Append(CalculateNeededShape(true));
                sb.Append(CalculateNeededNumber());
                break;
            case GroupType.ShapeNumber:
                sb.Append(CalculateNeededColor(true));
                sb.Append(CalculateNeededShape());
                sb.Append(CalculateNeededNumber());
                break;
            case GroupType.ShapeColorCons:
                sb.Append(CalculateNeededColor());
                sb.Append(CalculateNeededShape());
                sb.Append(CalculateNeededNumber(true));
                break;
            case GroupType.ColorCons:
                sb.Append(CalculateNeededColor());
                sb.Append(CalculateNeededShape(true));
                sb.Append(CalculateNeededNumber(true));
                break;
            case GroupType.ShapeCons:
                sb.Append(CalculateNeededColor(true));
                sb.Append(CalculateNeededShape());
                sb.Append(CalculateNeededNumber(true));
                break;
        }
        return sb.ToString();
    }

    string CalculateNeededShape(bool different = false)
    {
        string shapeStr = "";
        if (!different)
        {
            shapeStr = ConvertToString(myHand.cardsInHand[0].shape);
        }
        else
        {
            int index = 0;
            bool match = true;
            bool uniqueShapeFound = true;
            CardShape _shape = CardShape.Circle;
            for (; index < 4 && !uniqueShapeFound; index++)
            {
                uniqueShapeFound = true;
                _shape = (CardShape)index;
                for (int i = 0; i < myHand.cardsInHand.Count; i++)
                {
                    match = myHand.cardsInHand[i].shape == _shape;
                    uniqueShapeFound = uniqueShapeFound && !match;
                }
            }
            shapeStr = ConvertToString(_shape);
        }
        return shapeStr;
    }

    string CalculateNeededColor(bool different = false)
    {
        string colorStr = "";
        if (!different)
        {
            colorStr = ConvertToString(myHand.cardsInHand[0].color);
        }
        else
        {
            int index = 0;
            bool match = true;
            bool uniqueColorFound = true;
            CardColor _color = CardColor.Blue;
            for (; index < 4 && match; index++)
            {
                uniqueColorFound = true;
                _color = (CardColor)index;
                for (int i = 0; i < myHand.cardsInHand.Count && match; i++)
                {
                    match = myHand.cardsInHand[i].color == _color;
                    uniqueColorFound = uniqueColorFound && !match;
                }
            }
            colorStr = ConvertToString(_color);
        }
        return colorStr;
    }

    string CalculateNeededNumber(bool different = false)
    {
        string numStr = "";
        if (!different)
        {
            numStr = myHand.cardsInHand[0].number.ToString();
        }
        else
        {
            int cardIndex = 0;
            if (FindSmallestNumber(out cardIndex) > 1)
            {
                numStr = (myHand.cardsInHand[cardIndex].number - 1).ToString();
            }
            else if (FindGreatestNumber(out cardIndex) < 5)
            {
                numStr = (myHand.cardsInHand[cardIndex].number + 1).ToString();
            }
        }
        return numStr;
    }

    string ConvertToString(CardShape shape)
    {
        string str = "";
        switch (shape)
        {
            case CardShape.Joker:
                str = "J";
                break;
            case CardShape.Star:
                str = "S";
                break;
            case CardShape.Rectangle:
                str = "R";
                break;
            case CardShape.Triangle:
                str = "T";
                break;
            case CardShape.Circle:
                str = "C";
                break;
        }
        return str;
    }

    string ConvertToString(CardColor color)
    {
        string str = "";
        switch (color)
        {
            case CardColor.Yellow:
                str = "Y";
                break;
            case CardColor.Blue:
                str = "B";
                break;
            case CardColor.Red:
                str = "R";
                break;
            case CardColor.Green:
                str = "G";
                break;
        }
        return str;
    }

    int FindSmallestNumber(out int index)
    {
        int number = 6;
        index = 0;
        for (int i = 0; i < myHand.cardsInHand.Count; i++)
        {
            CardData comparedCard = myHand.cardsInHand[i];
            if (comparedCard.number < number)
            {
                number = comparedCard.number;
                index = i;
            }
        }
        return number;
    }

    int FindGreatestNumber(out int index)
    {
        int number = 0;
        index = 0;
        for (int i = 0; i < myHand.cardsInHand.Count; i++)
        {
            CardData comparedCard = myHand.cardsInHand[i];
            if (comparedCard.number > number)
            {
                number = comparedCard.number;
                index = i;
            }
        }
        return number;
    }
}
