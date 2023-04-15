using System.Collections.Generic;
using UnityEngine;

namespace Maskayop
{
    public class ImagesCreator : MonoBehaviour
    {
        [SerializeField] Transform imagesContainer;
        [SerializeField] GameObject pixelImagePrefab;
        [SerializeField] int pixelImageSize = 100;
        [SerializeField] int spacing = 0;

        [SerializeField] PictureSplitter pictureSplitter;
        [SerializeField] bool showValuesTexts = false;

        List<GameObject> imageObjects = new List<GameObject>();
        List<PixelImage> pixelImages = new List<PixelImage>();

        public void CreatePictureImages(int height, int width, Color32[,] pictureMatrix, string containerName)
        {
            foreach (Transform t in imagesContainer)
            {
                if(t.name == containerName)
                    Destroy(t.gameObject);
            }

            imageObjects.Clear();
            pixelImages.Clear();

            GameObject container = new GameObject(containerName);
            container.transform.SetParent(imagesContainer);

            RectTransform containerTransform = container.AddComponent<RectTransform>();
            containerTransform.anchorMin = Vector2.zero;
            containerTransform.anchorMax = Vector2.one;
            containerTransform.sizeDelta = containerTransform.anchoredPosition = Vector2.zero;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    GameObject newGO = Instantiate(pixelImagePrefab, containerTransform);
                    newGO.name = "Image - " + h + ":" + w;
                    imageObjects.Add(newGO);

                    RectTransform rt = newGO.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(pixelImageSize, pixelImageSize);
                    rt.SetLocalPositionAndRotation(new Vector3(w * pixelImageSize + w * spacing - (height * pixelImageSize + spacing * height * pixelImageSize) * 0.5f,
                                                               h * pixelImageSize + h * spacing - (width * pixelImageSize + spacing * width * pixelImageSize) * 0.5f, 0),
                                                               Quaternion.identity);

                    PixelImage newPixelImage = newGO.GetComponent<PixelImage>();

                    newPixelImage.image.color = pictureMatrix[h, w];

                    newPixelImage.RValueText.text = Mathf.FloorToInt(newPixelImage.image.color.r * 255).ToString();
                    newPixelImage.GValueText.text = Mathf.FloorToInt(newPixelImage.image.color.g * 255).ToString();
                    newPixelImage.BValueText.text = Mathf.FloorToInt(newPixelImage.image.color.b * 255).ToString();
                    
                    newPixelImage.ShowTexts(showValuesTexts);
                    pixelImages.Add(newPixelImage);
                }
            }
        }

        public void ShowValuesTexts()
        {
            foreach (PixelImage pi in pixelImages)
                pi.ShowTexts(true);
        }

        public void HideValuesTexts()
        {
            foreach (PixelImage pi in pixelImages)
                pi.ShowTexts(false);
        }
    }
}