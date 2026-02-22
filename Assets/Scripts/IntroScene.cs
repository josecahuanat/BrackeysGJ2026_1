using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroScene : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    
    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    
    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene("Main Gameplay");
    }
}
