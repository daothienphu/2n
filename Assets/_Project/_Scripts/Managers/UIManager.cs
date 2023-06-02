using System.Collections;
using _Project._Scripts.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace _Project._Scripts.Managers
{
    public class UIManager : MonoBehaviour {

        [SerializeField] private Animator _mainUIAnimator;
        [FormerlySerializedAs("_BGScroller")] [SerializeField] private BgScroller _bgScroller;
        private static readonly int LoadOut = Animator.StringToHash("LoadOut");

        public void OnStartButtonClicked(){
            _bgScroller.LoadOutBg();
            _mainUIAnimator.SetTrigger(LoadOut);
            StartCoroutine(LoadSceneDelay(1f, "MainScene"));
        }

        public void OnQuitButtonClicked(){
            Application.Quit();
        }

        IEnumerator LoadSceneDelay(float t, string sceneName){
            yield return new WaitForSeconds(t);
            SceneManager.LoadScene(sceneName);
        }
    }
}
