using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SetType
{
    None,
    NumberColor,
    ShapeNumber,
    ShapeColorCons,
    ColorCons,
    ShapeCon
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
        CheckIfNumberColor();
        if (setType == SetType.NumberColor)
        {
            setValid = JokerCountInSet() < 2;
            return;
        }
        CheckIfShapeNumber();
        if (setType == SetType.ShapeNumber)
        {
            setValid = JokerCountInSet() < 2;
            return;
        }
        CheckIfShapeColorCons();
        if (setType == SetType.ShapeColorCons)
        {
            setValid = JokerCountInSet() < 2;
            return;
        }
        CheckIfColorCons();
        if (setType == SetType.ColorCons)
        {
            setValid = JokerCountInSet() < 2;
            return;
        }
        CheckIfShapeCons();
        if (setType == SetType.ShapeCon)
        {
            setValid = JokerCountInSet() < 2;
            return;
        }
        setValid = false;
    }

    void CheckIfNumberColor()
    {
        bool valid = true;
        for (int index = 0; index < cardsInSet.Count && valid; index++)
        {
            for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && valid; checkedIndex++)
            {
                if (index == checkedIndex) continue;
                bool jokerInPair = cardsInSet[index].jokerCard || cardsInSet[checkedIndex].jokerCard;
                valid = cardsInSet[index].color == cardsInSet[checkedIndex].color;
                valid = valid && (cardsInSet[index].number == cardsInSet[checkedIndex].number || jokerInPair) &&
                    cardsInSet[index].shape != cardsInSet[checkedIndex].shape;
            }
        }
        if (valid)
        {
            setType = SetType.NumberColor;
        }
    }

    void CheckIfShapeNumber()
    {
        bool valid = true;
        for (int index = 0; index < cardsInSet.Count && valid; index++)
        {
            for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && valid; checkedIndex++)
            {
                if (index == checkedIndex) continue;
                bool pairWithJoker = cardsInSet[index].jokerCard || cardsInSet[checkedIndex].jokerCard;
                valid = cardsInSet[index].color != cardsInSet[checkedIndex].color;
                valid = valid && (cardsInSet[index].number == cardsInSet[checkedIndex].number || pairWithJoker) &&
                    (cardsInSet[index].shape == cardsInSet[checkedIndex].shape || pairWithJoker);
            }
        }
        if (valid)
        {
            setType = SetType.ShapeNumber;
        }
    }

    void CheckIfShapeColorCons()
    {
        bool valid = true;
        int offset = FindSmallestNumberInSet();
        RearrangeCardsInSetBy(offset);
        for (int index = 0; index < cardsInSet.Count - 1 && valid; index++)
        {
            bool pairWithJoker = cardsInSet[index].jokerCard || cardsInSet[index + 1].jokerCard;
            valid = (cardsInSet[index].shape == cardsInSet[index + 1].shape || pairWithJoker) && 
                (cardsInSet[index].number == cardsInSet[index + 1].number - 1 || pairWithJoker);
            valid = valid && cardsInSet[index].color == cardsInSet[index + 1].color;
        }
        if (valid)
        {
            setType = SetType.ShapeColorCons;
        }
    }

    void CheckIfColorCons()
    {
        bool valid = true;
        int offset = FindSmallestNumberInSet();
        RearrangeCardsInSetBy(offset);
        for (int index = 0; index < cardsInSet.Count - 1 && valid; index++)
        {
            bool pairWithJoker = cardsInSet[index].jokerCard || cardsInSet[index + 1].jokerCard;
            valid = cardsInSet[index].number == cardsInSet[index + 1].number - 1 || pairWithJoker;
            valid = valid && cardsInSet[index].color == cardsInSet[index + 1].color;
        }
        for (int index = 0; index < cardsInSet.Count && valid; index++)
        {
            for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && valid; checkedIndex++)
            {
                if (index == checkedIndex) continue;
                valid = cardsInSet[index].shape != cardsInSet[checkedIndex].shape;
            }
        }
        if (valid)
        {
            setType = SetType.ColorCons;
        }
    }

    void CheckIfShapeCons()
    {
        bool valid = true;
        int offset = FindSmallestNumberInSet();
        RearrangeCardsInSetBy(offset);
        for (int index = 0; index < cardsInSet.Count - 1 && valid; index++)
        {
            bool pairWithJoker = cardsInSet[index].jokerCard || cardsInSet[index + 1].jokerCard;
            valid = (cardsInSet[index].shape == cardsInSet[index + 1].shape || pairWithJoker) &&
                (cardsInSet[index].number == cardsInSet[index + 1].number - 1 || pairWithJoker);
        }
        for (int index = 0; index < cardsInSet.Count && valid; index++)
        {
            for (int checkedIndex = 0; checkedIndex < cardsInSet.Count && valid; checkedIndex++)
            {
                if (index == checkedIndex) continue;
                valid = cardsInSet[index].color != cardsInSet[checkedIndex].color;
            }
        }
        if (valid)
        {
            setType = SetType.ShapeCon;
        }
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
        switch (setDeter)
        {
            case "scN":
                break;
            
        }
    }
}
