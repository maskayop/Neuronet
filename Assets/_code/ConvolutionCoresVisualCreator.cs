using System.Collections.Generic;
using UnityEngine;

namespace Maskayop
{
    public class ConvolutionCoresVisualCreator : MonoBehaviour
    {
        [SerializeField] Transform convolutionCoresVisualContainer;
        [SerializeField] GameObject convolutionVisualPrefab;

        public List<ConvolutionCore> convolutionCores = new List<ConvolutionCore>();

        List<Transform> coresVisualTransforms = new List<Transform>();

        public void CreateConvolutionCoresVisual()
        {
            foreach (Transform t in convolutionCoresVisualContainer.transform)
                Destroy(t.gameObject);

            foreach (var cc in convolutionCores)
                cc.cells.Clear();

            coresVisualTransforms.Clear();

            for (int i = 0; i < convolutionCores.Count; i++)
            {
                GameObject newCore = new GameObject("Core - " + convolutionCores[i].name);
                newCore.transform.SetParent(convolutionCoresVisualContainer.transform);

                RectTransform rt = newCore.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;

                coresVisualTransforms.Add(rt);
                newCore.SetActive(false);

                CreateCoreCells(convolutionCores[i], rt);
            }
        }

        void CreateCoreCells(ConvolutionCore core, Transform parent)
        {
            int h = 0;

            for (int i = 0; i < core.values.Count; i++)
            {
                GameObject newGO = Instantiate(convolutionVisualPrefab, parent);
                newGO.name = "Cell - " + i;
                core.cells.Add(newGO);

                RectTransform rt = newGO.GetComponent<RectTransform>();
                h = Mathf.FloorToInt(i / core.size);
                rt.SetLocalPositionAndRotation(new Vector3((i - h * core.size) * rt.sizeDelta.x + rt.sizeDelta.x * 0.5f, h * rt.sizeDelta.y + rt.sizeDelta.y * 0.5f, 0), Quaternion.identity);

                ConvolutionVisual newCell = newGO.GetComponent<ConvolutionVisual>();
                newCell.valueText.text = core.values[i].ToString();
            }
        }

        public void ShowAllCores()
        {
            for (int i = 0; i < convolutionCores.Count; i++)
                ShowCore(i);
        }

        public void HideAllCores()
        {
            for (int i = 0; i < convolutionCores.Count; i++)
                HideCore(i);
        }

        void ShowCore(int id)
        {
            foreach (GameObject g in convolutionCores[id].cells)
                g.SetActive(true);
        }

        void HideCore(int id)
        {
            foreach (GameObject g in convolutionCores[id].cells)
                g.SetActive(false);
        }
    }
}