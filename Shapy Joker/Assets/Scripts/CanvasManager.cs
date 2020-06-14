using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class CanvasManager : MonoBehaviour
{
    [System.Serializable]
    public class BGSettings
    {
        public Image backgroundImage;
        public Sprite playerTurnBG;
        public Sprite opponentTurnBG;
        public Sprite cardBack;
        public Image playerDeckImage;
        public Image opponentDeckImage;
        public Color deckColorDark;
        public Sprite[] deckImages;
        public TextMeshProUGUI playerDeckCount;
        public TextMeshProUGUI opponentDeckCount;
        public RectTransform submitPanel;
        
        public NotificationImage player1BadSetMessage;
        public NotificationImage player2BadSetMessage;

        public Image screenFaderImage;
    }

    [System.Serializable]
    public class OptionsScreen
    {
        public Image optionsScreen;
        public Button optionsScreenButton;
        public Image volumeImage;
        public Sprite volumeLight;
        public Sprite volumeDark;
    }

    [System.Serializable]
    public class TimerSettings
    {
        public Image timerImage;
        public Sprite timerLight;
        public Sprite timerDark;
        public SpriteRenderer playerName;
        public SpriteRenderer opponentName;
        public TextMeshProUGUI playerTimerText;
        public TextMeshProUGUI opponentTimerText;
        public Color normalTimerColor;
        public Color normalOutlineTimerColor;
        public Color redTintTimer;
        public Color redTintOutlineTimer;
        public Image timeUpImage;
        public GameObject timeUpPanel;
    }

    public BGSettings backgroundSettings;
    public OptionsScreen optionsScreenSettings;
    public TimerSettings timerSettings;
    public static List<Sprite> visibleSprites;
    NotificationImage badSetMessage;
    TextMeshProUGUI activeTimer;
    SpriteRenderer nameToShow;
    private bool startedFlashing = false;

    private void Awake()
    {
        Blackboard.cm = this;
        visibleSprites = Resources.LoadAll("Cards", typeof(Sprite)).Cast<Sprite>().ToList();
    }

    public void SetAudioImage(float volume)
    {
        optionsScreenSettings.volumeImage.sprite = volume <= 0 ?optionsScreenSettings.volumeDark : optionsScreenSettings.volumeLight;
    }

    public void ChangeBackgroundImage(bool isPlayerTurn)
    {
        backgroundSettings.backgroundImage.sprite = isPlayerTurn ? backgroundSettings.playerTurnBG : backgroundSettings.opponentTurnBG;
        if (isPlayerTurn)
        {
            backgroundSettings.playerDeckImage.color = Color.white;
            backgroundSettings.opponentDeckImage.color = backgroundSettings.deckColorDark;
            if (Blackboard.gm.playerScore / 10 > 1)
            {
                backgroundSettings.playerDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.playerScore / 10 - 1];
            }
            if (Blackboard.gm.opponentScore / 10 > 1)
            {
                backgroundSettings.opponentDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.opponentScore / 10 - 1];
            }
        }
        else
        {
            backgroundSettings.playerDeckImage.color = backgroundSettings.deckColorDark;
            backgroundSettings.opponentDeckImage.color = Color.white;
            if (Blackboard.gm.playerScore / 10 > 1)
            {
                backgroundSettings.playerDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.playerScore / 10 - 1];
            }
            if (Blackboard.gm.opponentScore / 10 > 1)
            {
                backgroundSettings.opponentDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.opponentScore / 10 - 1];
            }
        }
    }

    public void SetOptionsScreenActive(bool value)
    {
        if (Blackboard.gm.isGameOver) return;
        optionsScreenSettings.optionsScreenButton.interactable = !value;
        Vector3 newPosition = value ? Vector3.zero : new Vector3(0, 9.24f, 0);
        iTween.MoveTo(optionsScreenSettings.optionsScreen.gameObject,  newPosition, 1.5f);
        GameManager.gamePaused = value;
    }

    public void ToggleTimerIcon(bool value)
    {
        timerSettings.timerImage.sprite = value ? timerSettings.timerLight : timerSettings.timerDark;
    }

    public void UpdateTimerText()
    {
        float counter = Mathf.Ceil(Blackboard.timer.timeCountDown);
        string timerText = (int)counter / 60 + ":" + (counter % 60 < 10 ? "0" + (counter % 60).ToString() : (counter % 60).ToString());
        activeTimer = Blackboard.gm.isMainPlayerTurn ? timerSettings.playerTimerText : timerSettings.opponentTimerText;
        if (counter <= 5)
        {
            if (!startedFlashing)
            {
                activeTimer.faceColor = timerSettings.redTintTimer;
                activeTimer.outlineColor = timerSettings.redTintOutlineTimer;
                startedFlashing = true;
                activeTimer.GetComponent<TimerText>().StartBlinking();
            }
        }
        else
        {
            startedFlashing = false;
            activeTimer.faceColor = timerSettings.normalTimerColor;
            activeTimer.outlineColor = timerSettings.normalOutlineTimerColor;
        }
        activeTimer.text = timerText;
    }

    public void ToggleNameActive(bool isPlayerTurn)
    {
        timerSettings.playerName.gameObject.SetActive(!isPlayerTurn);
        timerSettings.opponentName.gameObject.SetActive(isPlayerTurn);
        if (isPlayerTurn) timerSettings.opponentTimerText.text = "";
        else timerSettings.playerTimerText.text = "";

    }

    public void ResetNameActive()
    {
        timerSettings.playerName.gameObject.SetActive(true);
        timerSettings.playerTimerText.text = "";
        timerSettings.opponentName.gameObject.SetActive(true);
        timerSettings.opponentTimerText.text = "";
    }

    public void ShowTimeUpMessage()
    {
        activeTimer.GetComponent<TimerText>().StopBlinking();
        timerSettings.timeUpImage.gameObject.SetActive(true);
        Invoke("TurnOnTimeUpMessages", 20f / 60f);
        Invoke("HideTimeUpMessage", 135f / 60f);
    }

    void TurnOnTimeUpMessages()
    {
        timerSettings.timeUpPanel.SetActive(true);
        Invoke("TurnOffTimeUpMessages", 95f / 60f);
    }

    void TurnOffTimeUpMessages()
    {
        timerSettings.timeUpPanel.SetActive(false);
    }

    void HideTimeUpMessage()
    {
        timerSettings.timeUpImage.gameObject.SetActive(false);
    }

    public void ShowBadSetMessage()
    {
        badSetMessage = Blackboard.gm.isMainPlayerTurn ? backgroundSettings.player1BadSetMessage : backgroundSettings.player2BadSetMessage;
        badSetMessage.Show();
    }
}
