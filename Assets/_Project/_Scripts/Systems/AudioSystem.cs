using UnityEngine;

public class AudioSystem : Singleton<AudioSystem>
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _soundsSource;
    [SerializeField] private bool canPlaySound = true;
    public void PlayMusic(AudioClip clip){
        if (canPlaySound){
            _musicSource.clip = clip;
            _musicSource.Play();
        }
    }

    public void PlaySound(AudioClip clip, Vector3 pos, float vol = 1) {
        if (canPlaySound){
            _soundsSource.transform.position = pos;
            PlaySound(clip, vol);
        }
    }

    public void PlaySound(AudioClip clip, float vol = 1){
        if (canPlaySound){
            _soundsSource.PlayOneShot(clip, vol);
        }
    }

    public void Mute(){
        canPlaySound = false;
    }

    public void Unmute(){
        canPlaySound = true;
    }

    public bool IsMuted(){
        return !canPlaySound;
    }
}