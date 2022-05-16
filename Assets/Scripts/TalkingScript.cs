using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerState
{
    Idle, Listening, Talking
}

[RequireComponent(typeof (AudioSource))]
public class TalkingScript : MonoBehaviour
{
    //[SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerState playerState = PlayerState.Idle;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private float[] clipSampleData;
    
    public const int RecordingFrequency = 44100;
    public const int SampleDataLength = 1024;
    public const float SoundThreshold = 0.025f;



    void Start()
    {
        var playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerAudioSource = GetComponent<AudioSource>();
        playerAudioSource.pitch = 2f;
        clipSampleData = new float[SampleDataLength];
        Idle();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerState == PlayerState.Idle)
        {
            SwitchState();
        }
    }

    bool IsVolumeAboveThresold()
    {
        if (playerAudioSource.clip == null)
        {
            return false;
        }

        playerAudioSource.clip.GetData(clipSampleData, playerAudioSource.timeSamples);
        var clipLoudness = 0f;
        foreach (var item in clipSampleData)
        {
            clipLoudness += Mathf.Abs(item);
        }

        clipLoudness /= SampleDataLength; // int 1024
        return clipLoudness > SoundThreshold; // 0.025f

    }

    void SwitchState()
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                playerState = PlayerState.Listening;
                Listen();
                break;

            case PlayerState.Listening:
                playerState = PlayerState.Talking;
                Talk();
                break;

            case PlayerState.Talking:
                playerState = PlayerState.Idle;
                Idle();
                break;
        }
    }

    void Idle()
    {
        if (playerAudioSource.clip != null)
        {
            playerAudioSource.Stop();
            playerAudioSource.clip = null;
        }
        playerAudioSource.clip = Microphone.Start(null, false, 1, 44100);
    }
    void Listen()
    {
        playerAudioSource.clip = Microphone.Start(null, false, 5, 44100);
        Invoke("SwitchState", 5);
    }

    void Talk()
    {
        Microphone.End(null);
        if (playerAudioSource.clip != null)
        {
            playerAudioSource.Play();
        }
        Invoke("SwitchState", 5);
    }
}
