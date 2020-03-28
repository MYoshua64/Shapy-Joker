using System.Collections.Generic;
using System;

public enum GroupType
{
    None,
    NumberColor,
    ShapeNumber,
    ShapeColorCons,
    ColorCons,
    ShapeCons
}

public class Group
{
    List<CardData> cardsInSet = new List<CardData>();
    int minimumCardsInSet = 3;
    int maximumCardsInSet = 5;
    public bool isSetValid { get; private set; }
    public GroupType groupType { get; private set; }

    public Group(List<CardData> setCards)
    {
        cardsInSet = setCards;
    }

    public bool IsFull()
    {
        return cardsInSet.Count >= maximumCardsInSet;
    }

    public void CheckSetValidityBySequence()
    {
        if (cardsInSet.Count < minimumCardsInSet)
        {
            isSetValid = false;
            return;
        }
        CheckSequence();
        isSetValid = groupType != GroupType.None && JokerCountInSet() < 2;
    }

    int JokerCountInSet()
    {
        int jokerCount = 0;
        for (int i = 0; i < cardsInSet.Count; i++)
        {
            if (cardsInSet[i].jokerCard) jokerCount++;
        }
        return jokerCount;
    }

    void RearrangeCardsInSetByNumber()
    {
        int offset = FindSmallestNumberInSet();
        for (int index = 0; index < cardsInSet.Count;)
        {
            CardData card = cardsInSet[index];
            //skipping checking the card if it's a joker
            if (card.jokerCard)
            {
                index++;
                continue;
            }

            //determines where in the list we want the card to be based on number
            int desiredIndex = card.number - offset;

            //condition for exiting the process - desired index out of range
            if (desiredIndex >= cardsInSet.Count) return;
            if (index != desiredIndex && card.number != cardsInSet[desiredIndex].number)
                ReplaceCards(index, desiredIndex);
            else index++;
        }
    }

    int FindSmallestNumberInSet()
    {
        int offset = 6;
        for (int i = 0; i < cardsInSet.Count; i++)
        {
            if (!cardsInSet[i].jokerCard && cardsInSet[i].number < offset) offset = cardsInSet[i].number;
        }
        return offset;
    }

    void ReplaceCards(int indexA, int indexB)
    {
        CardData temp = cardsInSet[indexB];
        cardsInSet[indexB] = cardsInSet[indexA];
        cardsInSet[indexA] = temp;
    }

    /// <summary>
    /// Checks a set's validity by way of string reference. The string is built
    /// according to the matched attributes between the cards in the set.
    /// </summary>
    void CheckSequence()
    {
        string setDeter = "";
        bool shape = true, number = true, color = true, numberCon = false;
        //comparing each card to the others in the set to check for any matches
        for (int index = 0; index < cardsInSet.Count; index++)
        {
            for (int checkedIndex = index + 1; checkedIndex < cardsInSet.Count; checkedIndex++)
            {
                bool jokerInPair = cardsInSet[index].jokerCard || cardsInSet[checkedIndex].jokerCard;
                if (shape)
                    shape = cardsInSet[index].shape == cardsInSet[checkedIndex].shape || jokerInPair;
                if (number)
                    number = cardsInSet[index].number == cardsInSet[checkedIndex].number || jokerInPair;
                if (color)
                    color = cardsInSet[index].color == cardsInSet[checkedIndex].color;
            }
        }
        if (!number)
        {
            //Checking for consecutive numbers in the set
            RearrangeCardsInSetByNumber();
            numberCon = true;
            for (int index = 0; index < cardsInSet.Count - 1 && numberCon; index++)
            {
                for (int checkedIndex = index + 1; checkedIndex < cardsInSet.Count && numberCon; checkedIndex++)
                {
                    bool jokerInPair = cardsInSet[index].jokerCard || cardsInSet[checkedIndex].jokerCard;
                    numberCon = Math.Abs(cardsInSet[index].number - cardsInSet[checkedIndex].number) == Math.Abs(index - checkedIndex) || jokerInPair;
                }
            }
        }
        //Building the string reference according to the matches found
        if (shape) setDeter += "s";
        if (color) setDeter += "c";
        if (number) setDeter += "n";
        if (numberCon) setDeter += "N";
        bool unique = true;
        //And starts analyzing the string
        switch (setDeter)
        {
            case "cn":
                //This makes sure there are no two cards of the same shape
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = index + 1; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        unique = cardsInSet[index].shape != cardsInSet[checkedIndex].shape;
                    }
                }
                if (unique && cardsInSet.Count < 5) groupType = GroupType.NumberColor;
                break;
            case "sn":
                //This makes sure there are no two cards of the same color
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = index + 1; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        unique = cardsInSet[index].color != cardsInSet[checkedIndex].color;
                    }
                }
                if (unique) groupType = GroupType.ShapeNumber;
                break;
            case "cN":
                //This makes sure there are no two cards of the same shape
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = index + 1 ; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        unique = cardsInSet[index].shape != cardsInSet[checkedIndex].shape;
                    }
                }
                if (unique) groupType = GroupType.ColorCons;
                break;
            case "sN":
                //This makes sure there are no two cards with the same color
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = index + 1; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        unique = cardsInSet[index].color != cardsInSet[checkedIndex].color;
                    }
                }
                if (unique) groupType = GroupType.ShapeCons;
                break;
            case "scN":
                groupType = GroupType.ShapeColorCons;
                break;
        }
    }
}
