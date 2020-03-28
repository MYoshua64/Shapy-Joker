using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] RectTransform playerWinScreen;
    [SerializeField] RectTransform playerLoseScreen;
    [SerializeField] DeckBuilder deck;
    [SerializeField] Hand playerHand;
    [SerializeField] Button submitButton;
    [SerializeField] TextMeshProUGUI playerDeckText;
    [SerializeField] TextMeshProUGUI opponentDeckText;
    public Hand opponentHand;
    public Hand activeHand { get; private set; }
    public bool isPlayerTurn { get; private set; } = true;
    public bool isGameOver = false;
    public int playerScore { get; private set; } = 34;
    public int opponentScore { get; private set; } = 34;
    public static bool gamePaused;
    public bool timerOn { get; private set; } = false;

    //This will contain the positions of the cards before they are submitted
    public List<Vector3> lastPositions { get; private set; } = new List<Vector3>();

    private void Awake()
    {
        Blackboard.gm = this;
        Blackboard.cm.ChangeBackgroundImage(isPlayerTurn);
        activeHand = isPlayerTurn ? playerHand : opponentHand;
        if (!isPlayerTurn) Invoke("ActivateOpponent", 1f);
        playerDeckText.text = opponentDeckText.text = "34";
    }

    public void SetVolume(Slider volumeSlider)
    {
        AudioListener.volume = volumeSlider.value;
        Blackboard.cm.SetAudioImage(volumeSlider.value);
    }

    public void SubmitSet()
    {
        if (gamePaused) return;
        Blackboard.sfxPlayer.PlaySubmitSFX();
        submitButton.interactable = false;
        if (timerOn) Blackboard.timer.Stop();
        isPlayerTurn = !isPlayerTurn;
        foreach (CardData card in activeHand.cardsInHand)
        {
            CardVisual _Card = card.cardView;
            if (!_Card) continue;
            _Card.SetSubmitted();
        }
        CardVisual.currentHighSortingOrder = 10;
        StartCoroutine(MoveCardsOutOfScreen());
    }

    IEnumerator MoveCardsOutOfScreen()
    {
        lastPositions.Clear();
        List<CardData> repCards = new List<CardData>();
        repCards.AddRange(activeHand.cardsInHand);
        foreach (CardData card in repCards)
        {
            CardVisual _Card = card.cardView;
            if (!_Card) continue;
            lastPositions.Add(_Card.originalPos);
            activeHand.RemoveFromHand(_Card, true);
            Blackboard.tableCardsParent.RemoveFromFormation(_Card);
            iTween.MoveTo(_Card.gameObject, new Vector3(12, 0, 0), 2f);
            Destroy(_Card.gameObject, 1f);
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.3f);
        deck.DealNewCards(lastPositions.Count);
    }

    public void HandleTurnEnd(bool timeUp = false)
    {
        if (timeUp)
        {
            submitButton.interactable = false;
            Blackboard.cm.ShowTimeUpMessage();
            ReturnCards();
        }
        if (!isGameOver)
        {
            if (isPlayerTurn)
            {
                activeHand = playerHand;
            }
            else
            {
                activeHand = opponentHand;
                Invoke("ActivateOpponent", 1f);
            }
            Blackboard.cm.ChangeBackgroundImage(isPlayerTurn);
            if (timerOn) Blackboard.timer.Run();
        }
        else
        {
            HandleGameOver();
        }
    }

    private void ReturnCards()
    {
        int cardsCount = activeHand.cardsInHand.Count;
        for (int iter = 0; iter < cardsCount; iter++)
        {
            CardVisual _Card = activeHand.cardsInHand[0].cardView;
            if (!_Card) continue;
            activeHand.RemoveFromHand(_Card, true);
        }
        isPlayerTurn = !isPlayerTurn;
    }

    public void LowerScore()
    {
        if (!isPlayerTurn)
        {
            playerScore--;
            playerDeckText.text = playerScore.ToString();
        }
        else
        {
            opponentScore--;
            opponentDeckText.text = opponentScore.ToString();
        }
        isGameOver = playerScore <= 0 || opponentScore <= 0;
    }

    public void SetSubmitButtonInteractable(bool value)
    {
        submitButton.interactable = value;
    }

    void ActivateOpponent()
    {
        Blackboard.opponent.StartTurn();
    }

    void HandleGameOver()
    {
        if (playerScore <= 0)
        {
            playerWinScreen.gameObject.SetActive(true);
        }
        else if (opponentScore <= 0)
        {
            playerLoseScreen.gameObject.SetActive(true);
        }
    }

    public void SetTimerOnOff()
    {
        timerOn = !timerOn;
        Blackboard.cm.ToggleTimerIcon(timerOn);
        if (timerOn) Blackboard.timer.Run();
        else Blackboard.timer.Stop();
    }
}
