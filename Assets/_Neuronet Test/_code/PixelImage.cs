using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Maskayop
{
    public class PixelImage : MonoBehaviour
    {
        public Image image;

        public GameObject textsContainer;

        public TextMeshProUGUI RValueText;
        public TextMeshProUGUI GValueText;
        public TextMeshProUGUI BValueText;

        public void ShowTexts(bool state)
        {
            textsContainer.SetActive(state);
        }
    }
}