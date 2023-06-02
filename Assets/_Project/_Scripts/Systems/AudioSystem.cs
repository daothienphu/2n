using _Project._Scripts.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project._Scripts.Systems
{
    public class AudioSystem : Singleton<AudioSystem>
    {
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _soundsSource;
        [FormerlySerializedAs("canPlaySound")] [SerializeField] private bool _canPlaySound = true;
        public void PlayMusic(AudioClip clip){
            if (_canPlaySound){
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }

        public void PlaySound(AudioClip clip, Vector3 pos, float vol = 1) {
            if (_canPlaySound){
                _soundsSource.transform.position = pos;
                PlaySound(clip, vol);
            }
        }

        public void PlaySound(AudioClip clip, float vol = 1){
            if (_canPlaySound){
                _soundsSource.PlayOneShot(clip, vol);
            }
        }

        public void Mute(){
            _canPlaySound = false;
        }

        public void Unmute(){
            _canPlaySound = true;
        }

        public bool IsMuted(){
            return !_canPlaySound;
        }
    }
}