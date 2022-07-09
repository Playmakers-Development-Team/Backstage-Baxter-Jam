using UnityEngine;

namespace Pluggables
{
    public class PlugCableAudio : MonoBehaviour
    {
        // TODO: These still need to be hooked up according to the tooltips.
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Clips to be played when a connection would be audible through a speaker.")]
        [SerializeField] private AudioClip[] audioConnectOn;
        [Tooltip("Clips to be played when a disconnection would be audible through a speaker.")]
        [SerializeField] private AudioClip[] audioDisconnectOn;
        [Tooltip("Clips to be played when a connection would not be audible through a speaker.")]
        [SerializeField] private AudioClip[] audioConnectOff;
        [Tooltip("Clips to be played when a disconnection would not be audible through a speaker.")]
        [SerializeField] private AudioClip[] audioDisconnectOff;
        [SerializeField] private AudioClip[] audioElecFailure;

        private void OnEnable()
        {
            GameEvents.onDisconnect += OnDisconnect;
            GameEvents.onConnectionStarted += OnConnectionStarted;
            GameEvents.onConnect += OnConnect;
            GameEvents.onConnectionAbandoned += OnConnectionAbandoned;
            GameEvents.onConnectionFailure += OnConnectionFailure;
        }
        
        private void OnDisable()
        {
            GameEvents.onDisconnect -= OnDisconnect;
            GameEvents.onConnectionStarted -= OnConnectionStarted;
            GameEvents.onConnect -= OnConnect;
            GameEvents.onConnectionAbandoned -= OnConnectionAbandoned;
            GameEvents.onConnectionFailure -= OnConnectionFailure;
        }

        private void OnConnectionFailure(Connection obj)
        {
            PlayRandomSound(audioElecFailure);
        }

        private void OnConnectionAbandoned(Connection obj)
        {
            PlayRandomSound(audioDisconnectOn);
        }

        private void OnConnect(Connection connection, PlugCable arg2)
        {
            PlayRandomSound(audioConnectOff);
        }

        private void OnConnectionStarted(Connection obj)
        {
            PlayRandomSound(audioConnectOn);
        }

        private void OnDisconnect(Connection arg1, PlugCable arg2)
        {
            PlayRandomSound(audioDisconnectOff);
        }

        private void PlayRandomSound(AudioClip[] array)
        {
            if (array.Length == 0)
            {
                Debug.LogWarning("Could not play random sound: No sounds in array.");

                return;
            }
            
            int randIndex = Random.Range(0, array.Length - 1);
            
            AudioClip randClip = array[randIndex];
            
            audioSource.time = 0f;
            audioSource.clip = randClip;
            audioSource.Play();
        }
    }
}