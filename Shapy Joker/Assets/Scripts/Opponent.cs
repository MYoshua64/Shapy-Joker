using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    Hand myHand;
    CardView[] cardsOnScreen;

    public void StartTurn()
    {
        myHand = FindObjectOfType<GameManager>().opponentHand;
        cardsOnScreen = FindObjectsOfType<CardView>();
        SearchForSets();
    }

    void SearchForSets()
    {
        for (int index = 0; index < cardsOnScreen.Length; index++)
        {
            for (int checkedIndex = 0; checkedIndex < cardsOnScreen.Length; checkedIndex++)
            {
                if (index.Equals(checkedIndex)) continue;
                
            }
        }
    }
}
