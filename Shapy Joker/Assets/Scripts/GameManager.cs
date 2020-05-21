using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool allowCardPickUp { get; private set; } = true;
    public bool hotseatMode;
    [SerializeField] RectTransform playerWinScreen;
    [SerializeField] RectTransform playerLoseScreen;
    [SerializeField] DeckBuilder deck;
    [SerializeField] Hand player1Hand;
    [Tooltip("For hotseat mode only!!")] [SerializeField] Hand player2Hand;
    [SerializeField] Button submitButtonDefault;
    [SerializeField] Button submitButtonCorrect;
    [SerializeField] TextMeshProUGUI playerDeckText;
    [SerializeField] TextMeshProUGUI opponentDeckText;
    [SerializeField] Sprite buttonDefault;
    [SerializeField] Sprite buttonCorrect;
    [SerializeField] Sprite buttonWrong;
    [SerializeField] Toggle[] timerToggles;
    public Hand opponentHand;
    public Hand activeHand { get; private set; }
    public bool isMainPlayerTurn { get; private set; } = true;
    public bool isGameOver { get; set; } = false;
    public int playerScore { get; private set; } = 34;
    public int opponentScore { get; private set; } = 34;
    public static bool gamePaused;
    public bool timerOn { get; private set; } = false;
    bool playerSubmitted = false;
    Toggle currentToggle;

    //This will contain the positions of the cards before they are submitted
    public List<Vector3> lastPositions { get; private set; } = new List<Vector3>();

    private void Awake()
    {
        Blackboard.gm = this;
        Blackboard.cm.ChangeBackgroundImage(isMainPlayerTurn);
        activeHand = isMainPlayerTurn ? player1Hand : player2Hand;
        if (!isMainPlayerTurn && !hotseatMode) Invoke("ActivateOpponent", 1f);
        playerDeckText.text = opponentDeckText.text = "34";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPeriod) && isMainPlayerTurn)
            HandleTurnEnd();
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        Blackboard.cm.SetAudioImage(value);
    }

    public void SubmitSetWrong(Hand submittingHand)
    {
        if (gamePaused || submittingHand.submitted || activeHand != submittingHand) return;
        CancelInvoke("ResetSubmitButtonSprite");
        submittingHand.submitButtonWrong.GetComponent<Image>().sprite = buttonWrong;
        Invoke("ResetSubmitButtonSprite", 2f);
        ReportBadSetSubmission();
    }

    public void SubmitSet(Hand submittingHand)
    {
        if (gamePaused || submittingHand.submitted || activeHand != submittingHand) return;
        submittingHand.submitted = true;
        allowCardPickUp = false;
        Blackboard.sfxPlayer.PlaySubmitSFX();
        SetSubmitButtonsTrue(submittingHand.submitButton, submittingHand.submitButtonWrong, false);
        if (timerOn) Blackboard.timer.Stop();
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
        for (int j = 0; j < Blackboard.cm.backgroundSettings.submitPanel.childCount; j++)
        {
            CardVisual _Card = repCards[j].cardView;
            Transform selectedSlot = Blackboard.cm.backgroundSettings.submitPanel.GetChild(repCards.Count - 1 - j);
            float zAngle = repCards.Count % 2 == 1 ? 15 * (repCards.Count / 2) - 15 * j : 22.5f - 15 * j;
            float yPos = repCards.Count % 2 == 1 ? -0.16f * Mathf.Pow(j - repCards.Count / 2, 2) : -0.3f * (0.5f * Mathf.Pow(j, 2) - 1.5f * j + 1);
            Vector3 cardPosition = new Vector3(selectedSlot.position.x, yPos, selectedSlot.position.z);
            _Card.GetComponent<SpriteRenderer>().sortingOrder = 15 + j;
            iTween.MoveTo(_Card.gameObject, iTween.Hash("position", cardPosition, "time", 0.7f, "easetype", iTween.EaseType.easeOutBounce));
            iTween.ScaleTo(_Card.gameObject, iTween.Hash("scale", 2 * Vector3.one, "time", 0.7f));
            iTween.RotateTo(_Card.gameObject, iTween.Hash("rotation", zAngle * Vector3.forward, "time", 0.7f));
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
        isMainPlayerTurn = !isMainPlayerTurn;
        if (timerOn) Blackboard.cm.ToggleNameActive(isMainPlayerTurn);
        if (timeUp)
        {
            Blackboard.cm.ShowTimeUpMessage();
            ReturnCards();
        }
        if (!isGameOver)
        {
            if (isMainPlayerTurn)
            {
                allowCardPickUp = true;
                player1Hand.submitted = false;
                activeHand = player1Hand;
            }
            else
            {
                if (hotseatMode)
                {
                    allowCardPickUp = true;
                    player2Hand.submitted = false;
                    activeHand = player2Hand;
                }
                else
                {
                    activeHand = opponentHand;
                    opponentHand.submitted = false;
                    Invoke("ActivateOpponent", 1f);
                }
            }
            Blackboard.cm.ChangeBackgroundImage(isMainPlayerTurn);
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
    }

    public void LowerScore()
    {
        if (isMainPlayerTurn)
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

    public void ResetSubmitButtonSprite()
    {
        Button buttonToReset = isMainPlayerTurn ? player1Hand.submitButtonWrong : player2Hand.submitButtonWrong;
        buttonToReset.GetComponent<Image>().sprite = buttonDefault;
    }

    public void SetSubmitButtonsTrue(Button defaultButton, Button wrongButton, bool value)
    {
        if (wrongButton && defaultButton)
        {
            wrongButton.gameObject.SetActive(!value);
            defaultButton.gameObject.SetActive(value);
        }
    }

    public void ReportBadSetSubmission()
    {
        Blackboard.cm.ShowBadSetMessage();
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
            if (!hotseatMode)
            {
                Blackboard.cm.backgroundSettings.screenFaderImage.gameObject.SetActive(true);
            }
            playerLoseScreen.gameObject.SetActive(true);
        }
    }

    public void SetTimerOnOff()
    {
        timerOn = !timerOn;
        foreach (Toggle toggle in timerToggles)
        {
            toggle.gameObject.SetActive(timerOn);
        }
        if (timerOn)
        {
            Blackboard.cm.ToggleNameActive(isMainPlayerTurn);
            timerToggles[0].isOn = true;
        }
        else
        {
            Blackboard.cm.ResetNameActive();
        }
        Blackboard.cm.ToggleTimerIcon(timerOn);
        if (!timerOn) Blackboard.timer.Stop();
    }

    public void SetTimerValue(float time)
    {
        if (!timerOn) return;
        Blackboard.timer.SetTurnTime(time);
        Blackboard.timer.Run();
    }

    public void SetCurrentToggle(Toggle toggle)
    {
        if (!timerOn || !toggle.isOn) return;
        if (currentToggle)
        {
            currentToggle.isOn = toggle == currentToggle;
        }
        currentToggle = toggle;
        float time = float.Parse(currentToggle.name);
        SetTimerValue(time);
    }
}
