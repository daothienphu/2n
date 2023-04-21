using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {

    [SerializeField] private Animator _mainUIAnimator;
    [SerializeField] private BGScroller _BGScroller;
    public void OnStartButtonClicked(){
        _BGScroller.LoadOutBG();
        _mainUIAnimator.SetTrigger("LoadOut");
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
