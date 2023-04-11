using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maskayop;

namespace Neurotenwork
{
    [Serializable]
    public class ConvolutionCore
    {
        public enum FilterColor { Red, Green, Blue }

        public string name;
        public int size = 3;
        public FilterColor filterColor;
        public List<int> values = new List<int>();
        public int bias = 0;
        public List<GameObject> cells = new List<GameObject>();
    }

    [Serializable]
    public class ConvolutedImage
    {
        public string name;
        public List<ConvolutedPixelImage> convolutedPixelImages = new List<ConvolutedPixelImage>();
    }

    public class TextureParser : MonoBehaviour
    {
        [Header("Изображение")]
        public int sourceMipLevel = 0;
        public Texture2D source;

        [Header("Расщепление изображения")]
        public GameObject pictureContainer;
        public GameObject imagePixelPrefab;
        public int imagePixelSize = 100;
        public int spacing = 0;

        [Header("Свойства изображения")]
        public int sourceImagePixelsCount = 0;
        public int pixelsCount = 0;
        public int height;
        public int width;
        public int actualHeightWithPadding;
        public int actualWidthWithPadding;

        [Header("Свёртка")]
        public int padding = 0;
        public GameObject convolutionLayerCellPrefab;
        public GameObject convolutionLayersContainer;
        public GameObject coreCellsVisualizationContainer;
        public List<ConvolutionCore> convolutionCores = new List<ConvolutionCore>();

        [Header("Свёрнутое изображение")]
        public int convolutedImagePixelSize = 10;
        public GameObject convolutedImageContainer;
        public GameObject convolutedImagePixelPrefab;
        public string tempLayersFolderPath;

        List<ConvolutedImage> convolutedImages = new List<ConvolutedImage>();

        [Header("Макс пулинг")]
        public int maxPoolSize = 2;
        public GameObject maxPoolingContainer;

        [Header("Отладка")]
        public TextMeshProUGUI testValuesText;
        public CNN CNN;
        public int currentStep = 0;
        public int currentCore = 0;
        public int currentMaxPoolStep = 0;
        public int currentMaxPoolImage = 0;

        Color32[] pixels;

        List<GameObject> imageObjects = new List<GameObject>();
        List<Image> images = new List<Image>();
        List<PixelImage> pixelImages = new List<PixelImage>();
        List<GameObject> coreCellsVisualization = new List<GameObject>();
        List<int> convolutedLayerValues = new List<int>();
        List<GameObject> convolutedImageContainers = new List<GameObject>();
        List<int> maxPoolLayerValues = new List<int>();

        bool isInitialized = false;

        public void Init()
        {
            pixels = source.GetPixels32(sourceMipLevel);
            sourceImagePixelsCount = pixels.Length;

            currentStep = 0;
            currentCore = 0;
            CNN.timePassed = 0;
            currentMaxPoolStep = 0;

            convolutedLayerValues.Clear();
            maxPoolLayerValues.Clear();

            CreatePictureObjects();
            CreateConvolutionCores();
            CreateCoreCellsVisualization();
            CreateConvolutedPictureObjects();

            SetPixelImagesValuesTextsActive(false);

            isInitialized = true;
        }

        void CreatePictureObjects()
        {
            pixelsCount = 0;

            foreach (GameObject go in imageObjects)
                Destroy(go);

            imageObjects.Clear();
            images.Clear();
            pixelImages.Clear();

            height = Mathf.FloorToInt(source.height / Mathf.Pow(2, sourceMipLevel));
            actualHeightWithPadding = height + padding * 2;

            width = Mathf.FloorToInt(source.width / Mathf.Pow(2, sourceMipLevel));
            actualWidthWithPadding = width + padding * 2;

            for (int h = 0; h < actualHeightWithPadding; h++)
            {
                for (int w = 0; w < actualWidthWithPadding; w++)
                {
                    pixelsCount++;
                    GameObject newGO = Instantiate(imagePixelPrefab, pictureContainer.transform);
                    newGO.name = "Image - " + h + ":" + w;
                    imageObjects.Add(newGO);

                    RectTransform rt = newGO.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(imagePixelSize, imagePixelSize);
                    rt.SetLocalPositionAndRotation(new Vector3(w * rt.sizeDelta.x + w * spacing - (actualHeightWithPadding * rt.sizeDelta.x + spacing * actualHeightWithPadding * rt.sizeDelta.x) * 0.5f,
                                                               h * rt.sizeDelta.y + h * spacing - (actualWidthWithPadding * rt.sizeDelta.y + spacing * actualWidthWithPadding * rt.sizeDelta.y) * 0.5f, 0),
                                                               Quaternion.identity);

                    Image newImage = newGO.GetComponent<Image>();

                    if (h == 0 || w == 0 || h == actualHeightWithPadding - 1 || w == actualWidthWithPadding - 1)
                        newImage.color = Color.black;
                    else
                        newImage.color = pixels[(w - padding) + (h - padding) * width];

                    images.Add(newImage);

                    PixelImage newPixelImage = newGO.GetComponent<PixelImage>();

                    newPixelImage.RValueText.text = Mathf.FloorToInt(newImage.color.r * 255).ToString();
                    newPixelImage.GValueText.text = Mathf.FloorToInt(newImage.color.g * 255).ToString();
                    newPixelImage.BValueText.text = Mathf.FloorToInt(newImage.color.b * 255).ToString();

                    pixelImages.Add(newPixelImage);
                }
            }
        }

        void CreateConvolutionCores()
        {
            foreach (Transform t in convolutionLayersContainer.transform)
                Destroy(t.gameObject);

            foreach (var cc in convolutionCores)
                cc.cells.Clear();

            foreach (Transform t in convolutedImageContainer.transform)
                Destroy(t.gameObject);

            convolutedImageContainers.Clear();

            for (int i = 0; i < convolutionCores.Count; i++)
            {
                GameObject newCore = new GameObject("Core - " + convolutionCores[i].name);
                newCore.transform.SetParent(convolutionLayersContainer.transform);

                RectTransform rt = newCore.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; 
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = new Vector2 (i * 100, 0);

                CreateConvolutionCoreCells(convolutionCores[i], rt);

                GameObject container = new GameObject("Convoluted image - " + convolutionCores[i].name);
                container.transform.SetParent(convolutedImageContainer.transform);

                RectTransform crt = container.AddComponent<RectTransform>();
                crt.anchorMin = Vector2.zero;
                crt.anchorMax = Vector2.one;
                crt.sizeDelta = crt.anchoredPosition = Vector2.zero;
                crt.anchoredPosition = new Vector2(i * 150, i * 300);

                convolutedImageContainers.Add(container);

                ConvolutedImage newConvolutedImage = new ConvolutedImage();
                newConvolutedImage.name = convolutionCores[i].name + " convoluted";
                convolutedImages.Add(newConvolutedImage);
            }
        }

        void CreateConvolutionCoreCells(ConvolutionCore core, Transform parent)
        {
            int h = 0;

            for (int i = 0; i < core.values.Count; i++)
            {
                GameObject newGO = Instantiate(convolutionLayerCellPrefab, parent);
                newGO.name = "Cell - " + i;
                core.cells.Add(newGO);

                RectTransform rt = newGO.GetComponent<RectTransform>();
                h = Mathf.FloorToInt(i / core.size);
                rt.SetLocalPositionAndRotation(new Vector3((i - h * core.size) * rt.sizeDelta.x + rt.sizeDelta.x * 0.5f, h * rt.sizeDelta.y + rt.sizeDelta.y * 0.5f, 0), Quaternion.identity);

                ConvolutionVisual newCell = newGO.GetComponent<ConvolutionVisual>();
                newCell.valueText.text = core.values[i].ToString();
            }
        }

        void CreateCoreCellsVisualization()
        {
            foreach (Transform t in coreCellsVisualizationContainer.transform)
                Destroy(t.gameObject);

            coreCellsVisualization.Clear();

            for (int i = 0; i < convolutionCores[currentCore].size * convolutionCores[currentCore].size; i++)
            {
                GameObject newGO = Instantiate(convolutionLayerCellPrefab, coreCellsVisualizationContainer.transform);
                newGO.name = "Cell - " + i;
                coreCellsVisualization.Add(newGO);

                RectTransform rt = newGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(imagePixelSize, imagePixelSize);

                ConvolutionVisual newCell = newGO.GetComponent<ConvolutionVisual>();

                newGO.SetActive(false);
            }
        }
        
        public void SetPixelImagesValuesTextsActive(bool state)
        {
            foreach (PixelImage pi in pixelImages)
                pi.textsContainer.SetActive(state);
        }

        void CreateConvolutedPictureObjects()
        {
            for (int i = 0; i < convolutionCores.Count; i++)
            {
                if(convolutedImageContainers.Count > 0)
                    foreach (Transform t in convolutedImageContainers[i].transform)
                        Destroy(t.gameObject);

                convolutedImages[i].convolutedPixelImages.Clear();                

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        GameObject newGO = Instantiate(convolutedImagePixelPrefab, convolutedImageContainers[i].transform);
                        newGO.name = "Image - " + h + ":" + w;

                        RectTransform rt = newGO.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(convolutedImagePixelSize, convolutedImagePixelSize);
                        rt.SetLocalPositionAndRotation(new Vector3(w * rt.sizeDelta.x + w * spacing - (height * rt.sizeDelta.x + spacing * height * rt.sizeDelta.x) * 0.5f,
                                                                   h * rt.sizeDelta.y + h * spacing - (width * rt.sizeDelta.y + spacing * width * rt.sizeDelta.y) * 0.5f, 0),
                                                                   Quaternion.identity);

                        Image newImage = newGO.GetComponent<Image>();
                        newImage.color = Color.black;

                        ConvolutedPixelImage cpi = newGO.GetComponent<ConvolutedPixelImage>();
                        cpi.valueText.text = "-";
                        convolutedImages[i].convolutedPixelImages.Add(cpi);
                    }
                }
            }
        }

        public void NextStep()
        {
            if (!isInitialized)
                return;

            if (currentStep >= sourceImagePixelsCount)
            {
                CreateConvolutedLayerAsset();

                currentCore++;
                currentStep = 0;
            }
            else if(currentCore >= convolutionCores.Count && currentMaxPoolImage < convolutedImages.Count)
            {
                MaxPool();
            }
            else if (currentCore >= convolutionCores.Count)
            {
                CNN.Go(false);
                return;
            }
            else
            {
                int size = convolutionCores[currentCore].size;
                int h = 0;
                int f = 0;
                int l = 0;
                int currentValue = 0;
                int currentImageId = 0;
                Image currentImage = null;
                RectTransform currentVisualCellTransform = null;
                ConvolutionVisual currentCell = null;
                
                int summ = 0;

                for (int c = 0; c < convolutionCores[currentCore].values.Count; c++)
                {
                    h = Mathf.FloorToInt(c / size);
                    f = (c - h * size);
                    l = Mathf.FloorToInt(currentStep / width) * (size - 1) + currentStep;
                    currentImageId = f + h * actualWidthWithPadding + l;
                    currentImage = images[currentImageId];

                    float currentColorValue = 0;

                    if (convolutionCores[currentCore].filterColor == ConvolutionCore.FilterColor.Red)
                        currentColorValue = currentImage.color.r;
                    else if (convolutionCores[currentCore].filterColor == ConvolutionCore.FilterColor.Green)
                        currentColorValue = currentImage.color.g;
                    else if (convolutionCores[currentCore].filterColor == ConvolutionCore.FilterColor.Blue)
                        currentColorValue = currentImage.color.b;

                    currentValue = convolutionCores[currentCore].values[c] * Mathf.FloorToInt(currentColorValue * 255);
                    summ += currentValue;

                    coreCellsVisualization[c].gameObject.SetActive(true);

                    currentVisualCellTransform = coreCellsVisualization[c].GetComponent<RectTransform>();
                    currentVisualCellTransform.anchoredPosition = currentImage.rectTransform.anchoredPosition;

                    currentCell = coreCellsVisualization[c].GetComponent<ConvolutionVisual>();
                    currentCell.valueText.text = currentValue.ToString();
                }

                summ += convolutionCores[currentCore].bias;

                convolutedLayerValues.Add(summ);

                testValuesText.text = summ.ToString();
                                
                float floatSumm = 0;
                Color newColor = Color.black;

                floatSumm = (summ + 1000f) / 2000f;
                newColor = new Color(floatSumm, floatSumm, floatSumm);

                convolutedImages[currentCore].convolutedPixelImages[currentStep].image.color = newColor;
                convolutedImages[currentCore].convolutedPixelImages[currentStep].value = summ;
                convolutedImages[currentCore].convolutedPixelImages[currentStep].valueText.text = summ.ToString();
                convolutedImages[currentCore].convolutedPixelImages[currentStep].colorValue = newColor;
                convolutedImages[currentCore].convolutedPixelImages[currentStep].valueText.color = newColor * newColor * 2;

                currentStep++;
            }
        }

        void CreateConvolutedLayerAsset()
        {
            ConvolutedLayer layer = ScriptableObject.CreateInstance<ConvolutedLayer>();

            layer.core = new ConvolutionCore();

            layer.core.name = convolutionCores[currentCore].name;
            layer.core.size = convolutionCores[currentCore].size;
            layer.core.bias = convolutionCores[currentCore].bias;

            for (int i = 0; i < convolutionCores[currentCore].values.Count; i++)
                layer.core.values.Add(convolutionCores[currentCore].values[i]);

            layer.name = convolutionCores[currentCore].name;

            for (int i = 0; i < convolutedLayerValues.Count; i++)
                layer.convolutedLayerValues.Add(convolutedLayerValues[i]);

            var dirPath = Application.dataPath + tempLayersFolderPath;

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dirPath + layer.name + ".txt");
            var json = JsonUtility.ToJson(layer);
            bf.Serialize(file, json);
            file.Close();

            convolutedLayerValues.Clear();

#if UNITY_EDITOR
         UnityEditor.AssetDatabase.Refresh();
#endif
        }

        ConvolutedLayer LoadConvolutedLayerData(string layerName)
        {
            var dirPath = Application.dataPath + tempLayersFolderPath + layerName + ".txt";

            if (!File.Exists(dirPath))
            {
                Debug.LogWarning("Файл не существует - " + dirPath);
                return null;
            }

            ConvolutedLayer layer = ScriptableObject.CreateInstance<ConvolutedLayer>();

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(dirPath, FileMode.Open);
            JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), layer);
            file.Close();

            return layer;
        }

        public void MaxPool()
        {
            if (!isInitialized)
                return;

            int count = maxPoolSize * maxPoolSize;
            int[] valuesArray = new int[count];

            if (currentMaxPoolStep >= convolutedImages[currentMaxPoolImage].convolutedPixelImages.Count / count)
            {
                CreateMaxPoolLayerAsset();

                currentMaxPoolImage++;
                currentMaxPoolStep = 0;
                return;
            }

            int h = 0;
            int f = 0;
            int d = 0;

            int newId = 0;

            int l = 0;
            int currentId = 0;
            int value = 0;

            foreach (var cpi in convolutedImages[currentMaxPoolImage].convolutedPixelImages)
                cpi.image.color = cpi.colorValue;

            for (int i = 0; i < count; i++)
            {
                h = Mathf.FloorToInt(i / maxPoolSize);
                f = (i - h * maxPoolSize);
                                
                newId = currentMaxPoolStep * 2 + Mathf.FloorToInt(currentMaxPoolStep * 2 / width) * (width - 2);

                d = Mathf.FloorToInt(newId / (width - (maxPoolSize - 1)));
                l = newId + d * (maxPoolSize - 1);
                currentId = f + h * width + l;

                valuesArray[i] = convolutedImages[currentMaxPoolImage].convolutedPixelImages[currentId].value;

                convolutedImages[currentMaxPoolImage].convolutedPixelImages[currentId].image.color = Color.red;
            }

            value = Mathf.Max(valuesArray);
            maxPoolLayerValues.Add(value);

            testValuesText.text = value.ToString();

            currentMaxPoolStep++;
        }

        void CreateMaxPoolLayerAsset()
        {
            MaxPoolLayer layer = ScriptableObject.CreateInstance<MaxPoolLayer>();

            layer.name = layer.layerName = convolutedImages[currentMaxPoolImage].name + " Max Pool";

            for (int i = 0; i < maxPoolLayerValues.Count; i++)
                layer.layerValues.Add(maxPoolLayerValues[i]);

            var dirPath = Application.dataPath + tempLayersFolderPath;

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dirPath + layer.name + ".txt");
            var json = JsonUtility.ToJson(layer);
            bf.Serialize(file, json);
            file.Close();

            maxPoolLayerValues.Clear();

#if UNITY_EDITOR
         UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}