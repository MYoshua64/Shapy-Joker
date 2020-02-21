using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardColor
{
    Yellow,
    Blue,
    Red,
    Green
}

public enum CardShape
{
    Joker,
    Star,
    Rectangle,
    Triangle,
    Circle
}

public class CardData
{
    public CardVisual cardView { get; private set; }
    public string id { get; private set; }
    public CardColor color { get; private set; }
    public CardShape shape { get; private set; }
    public int number { get; private set; } = 0;
    public bool jokerCard { get; private set; }
    public int index { get; private set; }

    public CardData(string cardID)
    {
        //Determining color attribute
        id = cardID.ToUpper();
        switch (id[0])
        {
            case 'Y':
                color = CardColor.Yellow;
                break;
            case 'B':
                color = CardColor.Blue;
                break;
            case 'R':
                color = CardColor.Red;
                break;
            case 'G':
                color = CardColor.Green;
                break;
        }
        //If the string length is more than 2, we're assuming it a regular card
        if (id.Length > 2)
        {
            //Determining shape attribute
            switch (id[1])
            {
                case 'S':
                    shape = CardShape.Star;
                    break;
                case 'R':
                    shape = CardShape.Rectangle;
                    break;
                case 'T':
                    shape = CardShape.Triangle;
                    break;
                case 'C':
                    shape = CardShape.Circle;
                    break;
            }
            //Determining number attribute
            if (id[2] >= 49 && id[2] <= 53) number = id[2] - 48;
        }
        //Otherwise, it's probably a joker card, so it makes one final check before flagging it as a joker
        else if (id.Length == 2 && id[1] == 'J') jokerCard = true;
    }

    public void SetCardView(CardVisual view)
    {
        cardView = view;
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }

    public void Print()
    {
        Debug.Log(id);
    }
}
