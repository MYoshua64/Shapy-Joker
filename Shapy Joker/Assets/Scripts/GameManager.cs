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
    [SerializeField] Button badCheckButton;
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
        for (int i = 0; i < repCards.Count; i++)
        {
            CardVisual _Card = repCards[i].cardView;
            if (!_Card) continue;
            lastPositions.Add(_Card.originalPos);
            activeHand.RemoveFromHand(_Card, true);
            Blackboard.tableCardsParent.RemoveFromFormation(_Card);
        }
        yield return new WaitForSeconds(0.1f);
        for (int j = 0; j < repCards.Count; j++)
        {
            CardVisual _Card = repCards[j].cardView;
            Transform selectedSlot = Blackboard.cm.backgroundSettings.submitPanel.GetChild(repCards.Count - 1 - j);
            float zAngle = repCards.Count % 2 == 1 ? 15 * (repCards.Count / 2) - 15 * j : 22.5f - 15 * j;
            float yPos = repCards.Count % 2 == 1 ? -0.16f * Mathf.Pow(j - repCards.Count / 2, 2) : -0.3f * (0.5f * Mathf.Pow(j, 2) - 1.5f * j + 1);
            Vector3 cardPosition = new Vector3(selectedSlot.position.x, yPos, selectedSlot.position.z);
            iTween.MoveTo(_Card.gameObject, iTween.Hash("position", cardPosition, "time", 0.5f, "easetype", iTween.EaseType.spring));
            iTween.ScaleTo(_Card.gameObject, iTween.Hash("scale", 2 * Vector3.one, "time", 0.5f));
            iTween.RotateTo(_Card.gameObject, iTween.Hash("rotation", zAngle * Vector3.forward, "time", 0.5f));
        }
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < repCards.Count; i++)
        {
            CardVisual _Card = repCards[repCards.Count - 1 - i].cardView;
            iTween.MoveTo(_Card.gameObject, iTween.Hash("position", 12 * Vector3.right, "time", 1.5f));
            Destroy(_Card.gameObject, 1.5f);
            activeHand.cardSlots[i].transform.SetParent(Blackboard.cm.transform);
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.3f);
        deck.DealNewCards(lastPositions.Count);
    }

    public void HandleTurnEnd(bool timeUp = false)
    {
        if (timerOn) Blackboard.cm.ToggleNameActive(isPlayerTurn);
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
                badCheckButton.gameObject.SetActive(true);
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
        badCheckButton.gameObject.SetActive(!value);
    }

    public void ReportBadSetSubmission()
    {
        Debug.Log("This is a bad set!");
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
        if (timerOn)
            Blackboard.cm.ToggleNameActive(isPlayerTurn);
        else
            Blackboard.cm.ResetNameActive();
        Blackboard.cm.ToggleTimerIcon(timerOn);
        if (timerOn) Blackboard.timer.Run();
        else Blackboard.timer.Stop();
    }
}
