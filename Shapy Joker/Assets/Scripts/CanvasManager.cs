using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CanvasManager : MonoBehaviour
{
    public static List<Sprite> visibleSprites;
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite playerTurnBG;
    [SerializeField] Sprite opponentTurnBG;
    [SerializeField] Image playerDeckImage;
    [SerializeField] Image opponentDeckImage;
    [SerializeField] Sprite[] deckLightImages;
    [SerializeField] Sprite[] deckDarkImages;
    [SerializeField] Image optionsScreen;
    [SerializeField] Button optionsScreenButton;
    GameManager gm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        visibleSprites = Resources.LoadAll("Cards", typeof(Sprite)).Cast<Sprite>().ToList();
    }

    public void ChangeBackgroundImage(bool isPlayerTurn)
    {
        backgroundImage.sprite = isPlayerTurn ? playerTurnBG : opponentTurnBG;
        if (isPlayerTurn)
        {
            if (gm.playerScore / 10 > 1)
            {
                playerDeckImage.sprite = deckLightImages[gm.playerScore / 10 - 1];
            }
            if (gm.opponentScore / 10 > 1)
            {
                opponentDeckImage.sprite = deckDarkImages[gm.opponentScore / 10 - 1];
            }
        }
        else
        {
            if (gm.playerScore / 10 > 1)
            {
                playerDeckImage.sprite = deckDarkImages[gm.playerScore / 10 - 1];
            }
            if (gm.opponentScore / 10 > 1)
            {
                opponentDeckImage.sprite = deckLightImages[gm.opponentScore / 10 - 1];
            }
        }
    }

    public void SetOptionsScreenActive(bool value)
    {
        optionsScreenButton.interactable = !value;
        Vector3 newPosition = value ? Vector3.zero : new Vector3(0, 9.24f, 0);
        iTween.MoveTo(optionsScreen.gameObject,  newPosition, 1.5f);
        GameManager.gamePaused = value;
    }
}
