using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] List<AudioClip> cardTakeSFX;
    [SerializeField] List<AudioClip> cardPlaceSFX;
    [SerializeField] AudioClip submitSFX;

    private void Awake()
    {
        Blackboard.sfxPlayer = this;
    }

    /// <summary>
    /// Plays the SFX for card movement
    /// </summary>
    /// <param name="takeCard">Decides whether to play a card take SFX or card place SFX</param>
    public void PlayCardSFX(bool takeCard)
    {
        //Take the correct list based off the boolean
        List<AudioClip> usedSFX = new List<AudioClip>();
        usedSFX.AddRange(takeCard ? cardTakeSFX : cardPlaceSFX);

        //Then picks a sound randomly from the list
        AudioClip selectedClip = usedSFX[Random.Range(0, usedSFX.Count)];
        AudioSource.PlayClipAtPoint(selectedClip, Camera.main.transform.position);
    }

    public void PlaySubmitSFX()
    {
        AudioSource.PlayClipAtPoint(submitSFX, Camera.main.transform.position);
    }
}
