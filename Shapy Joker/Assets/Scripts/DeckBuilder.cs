using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    [SerializeField] RectTransform playerDeck;
    [SerializeField] RectTransform opponentDeck;
    [SerializeField] CardVisual cardViewPF;
    List<string> preDeck = new List<string>();
    List<CardData> deck = new List<CardData>();
    CardVisual[] cardsOnScreen;
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
        cardsOnScreen = FindObjectsOfType<CardVisual>();
        for (; currentCardIndex < cardsOnScreen.Length; currentCardIndex++)
        {
            cardsOnScreen[currentCardIndex].AttachCardData(deck[currentCardIndex]);
        }
    }

    void AttachCardToView(CardVisual cardView)
    {
        cardView.AttachCardData(deck[currentCardIndex]);
        currentCardIndex++;
    }

    public void DealNewCards(int numberToDeal)
    {
        StartCoroutine(StartDealing(numberToDeal));
    }

    IEnumerator StartDealing(int numberToDeal)
    {
        RectTransform activeDeck = gm.isPlayerTurn ? playerDeck : opponentDeck;
        while (numberToDeal > 0 && !gm.isGameOver)
        {
            CardVisual newCard = Instantiate(cardViewPF, activeDeck.position, Quaternion.identity, CardVisual.tableCardsParent.transform);
            CardVisual.tableCardsParent.AddToFormation(newCard);
            newCard.SetOriginalPosition(gm.lastPositions[0]);
            iTween.MoveTo(newCard.gameObject, gm.lastPositions[0], 0.75f);
            gm.lastPositions.RemoveAt(0);
            AttachCardToView(newCard);
            numberToDeal--;
            gm.LowerScore();
            yield return new WaitForSeconds(0.3f);
        }
        gm.HandleTurnEnd();
    }
}
