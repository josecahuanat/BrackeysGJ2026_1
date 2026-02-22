using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public class MainMenuButtonVisualController : MonoBehaviour
{
    [Header("Buttons in order (Play first)")]
    [SerializeField] private Button[] buttons;

    [Header("Scene Names (same order as Buttons)")]
    [SerializeField] private string[] sceneNames;

    [Header("Alpha")]
    [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float visibleAlpha = 1f;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.18f;
    [Header("Options Panel (índice del botón Options)")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private int optionsButtonIndex = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip navigateSfx;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private int currentIndex = -1;
    private Graphic[] containerGraphics;
    private Graphic[] selectGraphics;
    private float[] currentAlphas;
    private float[] targetAlphas;
    private AudioSource sfxSource;

    private void Start()
    {
        SetupSfxAudio();

        if (buttons == null || buttons.Length == 0)
            return;

        CacheButtonVisuals();
        SetCurrentIndex(0, true);
        SelectButtonObject(0);
    }

    private void Update()
    {
        if (buttons == null || buttons.Length == 0 || EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null)
            return;

        int selectedIndex = FindIndexBySelectedObject(selectedObject);
        if (selectedIndex >= 0 && selectedIndex != currentIndex)
            SetCurrentIndex(selectedIndex, false);

        // ── Confirmar con Enter / Submit ──────────────────────────────────────
        if (Input.GetButtonDown("Submit"))
            ConfirmCurrentButton();
        // ─────────────────────────────────────────────────────────────────────

        AnimateVisuals();
    }

    private void ConfirmCurrentButton()
    {
        if (currentIndex < 0 || currentIndex >= buttons.Length)
            return;

        // Botón Options
        if (currentIndex == optionsButtonIndex && optionsPanel != null)
        {
            optionsPanel.SetActive(!optionsPanel.activeSelf);
            return;
        }
        if (sceneNames != null && currentIndex < sceneNames.Length)
        {
            string scene = sceneNames[currentIndex];
            if (!string.IsNullOrEmpty(scene))
            {
                SceneManager.LoadScene(scene);
                return;
            }
        }

        buttons[currentIndex]?.onClick.Invoke();
    }
    public void SetMainMenu()
    {
        optionsPanel.SetActive(false);
    }
    private int FindIndexBySelectedObject(GameObject selectedObject)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
                continue;

            GameObject buttonObject = button.gameObject;
            if (selectedObject == buttonObject || selectedObject.transform.IsChildOf(buttonObject.transform))
                return i;
        }

        return -1;
    }

    private void SetCurrentIndex(int index, bool force)
    {
        if (index < 0 || index >= buttons.Length)
            return;

        if (!force && currentIndex == index)
            return;

        currentIndex = index;
        ApplyAlphaTargets(force);

        if (!force)
            PlayNavigateSfx();
    }

    private void SetupSfxAudio()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = sfxVolume;
    }

    private void PlayNavigateSfx()
    {
        if (sfxSource == null || navigateSfx == null)
            return;

        sfxSource.PlayOneShot(navigateSfx, sfxVolume);
    }

    private void CacheButtonVisuals()
    {
        int count = buttons.Length;
        containerGraphics = new Graphic[count];
        selectGraphics = new Graphic[count];
        currentAlphas = new float[count * 2];
        targetAlphas = new float[count * 2];

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
                continue;

            containerGraphics[i] = button.targetGraphic as Graphic;
            selectGraphics[i] = FindSelectGraphic(button);

            if (selectGraphics[i] != null && !selectGraphics[i].gameObject.activeSelf)
                selectGraphics[i].gameObject.SetActive(true);
        }
    }

    private void ApplyAlphaTargets(bool instant)
    {
        if (buttons == null || targetAlphas == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
        {
            bool isSelected = i == currentIndex;
            float target = isSelected ? visibleAlpha : hiddenAlpha;

            int containerSlot = i * 2;
            int selectSlot = containerSlot + 1;

            targetAlphas[containerSlot] = target;
            targetAlphas[selectSlot] = target;

            if (instant)
            {
                currentAlphas[containerSlot] = target;
                currentAlphas[selectSlot] = target;
            }
        }

        if (instant)
            RenderCurrentAlphas();
    }

    private void AnimateVisuals()
    {
        if (buttons == null || currentAlphas == null || targetAlphas == null)
            return;

        float step = Time.unscaledDeltaTime / transitionDuration;
        bool changed = false;

        for (int i = 0; i < currentAlphas.Length; i++)
        {
            float next = Mathf.MoveTowards(currentAlphas[i], targetAlphas[i], step);
            if (!Mathf.Approximately(next, currentAlphas[i]))
            {
                currentAlphas[i] = next;
                changed = true;
            }
        }

        if (changed)
            RenderCurrentAlphas();
    }

    private void RenderCurrentAlphas()
    {
        if (buttons == null || containerGraphics == null || selectGraphics == null || currentAlphas == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
        {
            int containerSlot = i * 2;
            int selectSlot = containerSlot + 1;

            Graphic containerGraphic = containerGraphics[i];
            if (containerGraphic != null)
                SetGraphicAlpha(containerGraphic, currentAlphas[containerSlot]);

            Graphic selectGraphic = selectGraphics[i];
            if (selectGraphic != null)
                SetGraphicAlpha(selectGraphic, currentAlphas[selectSlot]);
        }
    }

    private static Graphic FindSelectGraphic(Button button)
    {
        Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null || graphic.gameObject == button.gameObject)
                continue;

            string name = graphic.gameObject.name.ToLowerInvariant();
            if (name.Contains("selectimage") || name.Contains("selectimagen") || name.Contains("selected"))
                return graphic;
        }

        return null;
    }

    private static void SetGraphicAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
        graphic.canvasRenderer.SetAlpha(alpha);
    }

    private void SelectButtonObject(int index)
    {
        if (EventSystem.current == null)
            return;

        Button button = buttons[index];
        if (button == null)
            return;

        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void FocusButton(int index, bool playNavigateSfx = false)
    {
        if (buttons == null || buttons.Length == 0)
            return;

        if (index < 0 || index >= buttons.Length)
            return;

        currentIndex = index;
        ApplyAlphaTargets(true);
        SelectButtonObject(index);

        if (playNavigateSfx)
            PlayNavigateSfx();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}