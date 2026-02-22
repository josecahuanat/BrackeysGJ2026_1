using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Interact Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Puzzle Progress")]
    [SerializeField] private GameObject progressUI;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Puzzle Progress - Fade")]
    [SerializeField] private float tiempoVisibleAntesDeFade = 3f;
    [SerializeField] private float duracionFade = 1f;

    [Header("Scene Fade")]
    [SerializeField] private Image SceneFadeImage;
    [SerializeField] private float fadeDuration = 1f;


    private CanvasGroup progressCanvasGroup;
    private Coroutine fadeCoroutine;
    private Coroutine feedbackCoroutine;
    private Coroutine currentFade;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (progressUI) progressCanvasGroup = progressUI.GetComponent<CanvasGroup>();
    }
    void Start()
    {
        HidePuzzleObjectiveUI();
        SceneFadeImage.color = Color.black;
        FadeOut();
    }

    // Mensaje temporal en el prompt (reemplaza los Debug.Log del inventario)
    public void ShowFeedback(string message, float duration = 2f)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(message, duration));
    }

    IEnumerator FeedbackRoutine(string message, float duration)
    {
        if (promptUI)  promptUI.SetActive(true);
        if (promptText) promptText.text = message;
        yield return new WaitForSeconds(duration);
        // Después del feedback, el PlayerInteraction lo sobreescribirá solo
        feedbackCoroutine = null;
    }

    public void ShowPuzzleObjectiveUI()
    {
        if (progressUI) progressUI.SetActive(true);
        if (progressCanvasGroup) progressCanvasGroup.alpha = 1f;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutPuzzleUI());
    }

    public void HidePuzzleObjectiveUI()
    {
        if (progressUI) progressUI.SetActive(false);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (progressUI) progressUI.SetActive(false);
    }
    IEnumerator FadeOutPuzzleUI()
    {
        yield return new WaitForSeconds(tiempoVisibleAntesDeFade);

        float t = 0f;
        while (t < duracionFade)
        {
            t += Time.deltaTime;
            if (progressCanvasGroup) progressCanvasGroup.alpha = 1f - (t / duracionFade);
            yield return null;
        }

        if (progressUI) progressUI.SetActive(false);
        fadeCoroutine = null;
    }
    public void HidePuzzleUI()
    {
        if (progressUI) progressUI.SetActive(false);
    }
    public void UpdatePuzzleProgress(string progreso, string pista)
    {
        if (progressText) progressText.text = progreso;
        if (hintText)
        {
            //hintText.gameObject.SetActive(!string.IsNullOrEmpty(pista));
            hintText.text = pista ?? "";
        }
    }

    bool gameEnded = false;
    public void EndGame()
    {
        gameEnded = true;
        FadeIn();
    }

    public void FadeIn()
    {
        StartFade(0f, 1f);
    }

    public void FadeOut()
    {
        StartFade(1f, 0f);
    }

    private void StartFade(float from, float to, float duration = -1f)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(from, to, duration < 0 ? fadeDuration : duration));
    }

    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (SceneFadeImage == null) return;
        Color c = SceneFadeImage.color;
        c.a = alpha;
        SceneFadeImage.color = c;

        if (gameEnded)
            SceneManager.LoadScene("Credits");
    }
}