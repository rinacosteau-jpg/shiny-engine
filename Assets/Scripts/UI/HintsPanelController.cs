using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the visibility and content of the on-screen hints panel.
/// </summary>
public class HintsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text hintsLabel;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        EnsurePanelIsLowestLayer();

        if (closeButton != null)
            closeButton.onClick.AddListener(HidePanel);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(HidePanel);
    }

    /// <summary>
    /// Shows the hints panel if it is currently hidden.
    /// </summary>
    public void ShowPanel()
    {
        EnsurePanelIsLowestLayer();

        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the hints panel.
    /// </summary>
    public void HidePanel()
    {
        if (panelRoot != null && panelRoot.activeSelf)
            panelRoot.SetActive(false);
    }

    /// <summary>
    /// Updates the hint message displayed on the panel.
    /// </summary>
    public void SetHint(string message)
    {
        if (hintsLabel != null)
            hintsLabel.text = message;
    }

    /// <summary>
    /// Returns whether the hints panel is currently active.
    /// </summary>
    public bool IsPanelActive()
    {
        return panelRoot != null && panelRoot.activeSelf;
    }

    private void EnsurePanelIsLowestLayer()
    {
        if (panelRoot == null)
            return;

        Transform panelTransform = panelRoot.transform;

        if (panelTransform.parent != null)
            panelTransform.SetAsFirstSibling();
    }
}
