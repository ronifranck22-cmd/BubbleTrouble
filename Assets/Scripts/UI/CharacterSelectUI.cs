using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public static PlayerSkin SelectedSkin;

    public PlayerSkin[] skins;
    public Image previewImage;
    public Button previousButton;
    public Button nextButton;

    private int selectedIndex;

    private void OnEnable()
    {
        selectedIndex = SelectedSkin != null ? System.Array.IndexOf(skins, SelectedSkin) : 0;
        if (selectedIndex < 0) selectedIndex = 0;
        ApplySelection();

        previousButton.onClick.AddListener(SelectPrevious);
        nextButton.onClick.AddListener(SelectNext);
    }

    private void OnDisable()
    {
        previousButton.onClick.RemoveListener(SelectPrevious);
        nextButton.onClick.RemoveListener(SelectNext);
    }

    private void SelectPrevious()
    {
        selectedIndex = (selectedIndex - 1 + skins.Length) % skins.Length;
        ApplySelection();
        ClearButtonSelection();
    }

    private void SelectNext()
    {
        selectedIndex = (selectedIndex + 1) % skins.Length;
        ApplySelection();
        ClearButtonSelection();
    }

    private void ApplySelection()
    {
        SelectedSkin = skins[selectedIndex];
        previewImage.sprite = SelectedSkin.front;
    }

    // A clicked button stays selected, and Space is a Submit key, so the Space press
    // that starts the game would otherwise "click" the last arrow again.
    private void ClearButtonSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
