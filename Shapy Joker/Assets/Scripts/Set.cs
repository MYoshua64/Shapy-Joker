using System.Collections.Generic;

public enum SetType
{
    None,
    NumberColor,
    ShapeNumber,
    ShapeColorCons,
    ColorCons,
    ShapeCons
}

public class Set
{
    List<CardData> cardsInSet = new List<CardData>();
    int minimumCardsInSet = 3;
    int maximumCardsInSet = 5;
    public bool setValid { get; private set; }
    public SetType setType { get; private set; }

    public Set(List<CardData> setCards)
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
            setValid = false;
            return;
        }
        CheckSequence();
        setValid = setType != SetType.None && JokerCountInSet() < 2;
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

    int FindSmallestNumberInSet()
    {
        int offset = 6;
        for (int index = 0; index < cardsInSet.Count; index++)
        {
            if (cardsInSet[index].number < offset) offset = cardsInSet[index].number;
        }
        return offset;
    }

    void RearrangeCardsInSetBy(int offset)
    {
        for (int checkedNumber = offset; checkedNumber < 6; checkedNumber++)
        {
            for (int index = 0; index < cardsInSet.Count; index++)
            {
                if (cardsInSet[index].number == checkedNumber && cardsInSet[index].number != index + offset)
                {
                    CardData pulledCard = cardsInSet[index];
                    cardsInSet.RemoveAt(index);
                    cardsInSet.Add(pulledCard);
                    break;
                }
            }
        }
    }

    void CheckSequence()
    {
        string setDeter = "";
        bool shape = true, number = true, color = true, numberCon = true;
        for (int index = 0; index < cardsInSet.Count; index++)
        {
            for (int checkedIndex = 0; checkedIndex < cardsInSet.Count; checkedIndex++)
            {
                if (index == checkedIndex) continue;
                bool jokerInPair = cardsInSet[index].jokerCard || cardsInSet[checkedIndex].jokerCard;
                if (shape)
                    shape = cardsInSet[index].shape == cardsInSet[checkedIndex].shape || jokerInPair;
                if (number)
                    number = cardsInSet[index].number == cardsInSet[checkedIndex].number || jokerInPair;
                if (color)
                    color = cardsInSet[index].color == cardsInSet[checkedIndex].color;
            }
        }
        for (int index = 0; index < cardsInSet.Count - 1 && numberCon; index++)
        {
            bool jokerInPair = cardsInSet[index].jokerCard || cardsInSet[index + 1].jokerCard;
            numberCon = cardsInSet[index].number == cardsInSet[index + 1].number - 1 || jokerInPair;
        }
        if (shape) setDeter += "s";
        if (color) setDeter += "c";
        if (number) setDeter += "n";
        if (numberCon) setDeter += "N";
        bool unique = true;
        switch (setDeter)
        {
            case "cn":
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        if (index == checkedIndex) continue;
                        unique = cardsInSet[index].shape != cardsInSet[checkedIndex].shape;
                    }
                }
                if (unique) setType = SetType.NumberColor;
                break;

            case "sn":
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        if (index == checkedIndex) continue;
                        unique = cardsInSet[index].color != cardsInSet[checkedIndex].color;
                    }
                }
                if (unique) setType = SetType.ShapeNumber;
                break;
            case "cN":
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        if (index == checkedIndex) continue;
                        unique = cardsInSet[index].shape != cardsInSet[checkedIndex].shape;
                    }
                }
                if (unique) setType = SetType.ColorCons;
                break;
            case "sN":
                for (int index = 0; index < cardsInSet.Count && unique; index++)
                {
                    for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && unique; checkedIndex++)
                    {
                        if (index == checkedIndex) continue;
                        unique = cardsInSet[index].color != cardsInSet[checkedIndex].color;
                    }
                }
                if (unique) setType = SetType.ShapeCons;
                break;
            case "scN":
                setType = SetType.ShapeColorCons;
                break;
        }
    }
}
