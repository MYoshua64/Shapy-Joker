using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    [SerializeField] CardView cardViewPF;
    List<string> preDeck = new List<string>();
    List<CardData> deck = new List<CardData>();
    CardView[] cardsOnScreen;
    GameManager gm;
    int currentCardIndex = 0;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        FillDeck();
        ShuffleDeck();
        AttachCardsToView();
    }

    void FillDeck()
    {
        for (int index = 0; index < 88; index++)
        {
            if (index < 80)
            {
                int color = index / 5 % 4;
                int number = index % 5 + 1;
                int shape = index / 20 % 4;
                DetermineStringID(color, shape, number);
            }
            else
            {
                int color = (index - 80) / 2 % 4;
                DetermineStringID(color);
            }
            CreateCard(preDeck[index]);
        }
        
    }

    void DetermineStringID(int color, int shape, int number)
    {
        string id = "";
        switch (color)
        {
            case 0:
                id += "Y";
                break;
            case 1:
                id += "B";
                break;
            case 2:
                id += "R";
                break;
            case 3:
                id += "G";
                break;
        }
        switch (shape)
        {
            case 0:
                id += "S";
                break;
            case 1:
                id += "R";
                break;
            case 2:
                id += "T";
                break;
            case 3:
                id += "C";
                break;
        }
        id += number.ToString();
        preDeck.Add(id);
    }

    void DetermineStringID(int color)
    {
        string id = "";
        switch (color)
        {
            case 0:
                id += "Y";
                break;
            case 1:
                id += "B";
                break;
            case 2:
                id += "R";
                break;
            case 3:
                id += "G";
                break;
        }
        id += "J";
        preDeck.Add(id);
    }

    void CreateCard(string cardID)
    {
        CardData newCard = new CardData(cardID);
        deck.Add(newCard);
    }

    void ShuffleDeck()
    {
        deck.Clear();
        List<string> tempID = new List<string>();
        foreach (string id in preDeck)
        {
            tempID.Add(id);
        }
        preDeck.Clear();
        while (tempID.Count > 0)
        {
            int index = Random.Range(0, tempID.Count);
            preDeck.Add(tempID[index]);
            tempID.RemoveAt(index);
            CreateCard(preDeck[preDeck.Count - 1]);
        }
        currentCardIndex = 0;
    }

    void AttachCardsToView()
    {
        cardsOnScreen = FindObjectsOfType<CardView>();
        for (; currentCardIndex < cardsOnScreen.Length; currentCardIndex++)
        {
            cardsOnScreen[currentCardIndex].AttachCardData(deck[currentCardIndex]);
        }
    }

    void AttachCardToView(CardView cardView)
    {
        cardView.AttachCardData(deck[currentCardIndex]);
        currentCardIndex++;
    }

    public void DealNewCards(int numberToDeal)
    {
        while (numberToDeal > 0)
        {
            CardView newCard = Instantiate(cardViewPF, gm.lastPositions[0], Quaternion.identity);
            gm.lastPositions.RemoveAt(0);
            newCard.transform.SetParent(CardView.tableCardsParent);
            AttachCardToView(newCard);
            numberToDeal--;
        }
    }
}
