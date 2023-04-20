using System.Collections.Generic;
using UnityEngine;

namespace Maskayop
{
    public class Convoluter : MonoBehaviour
    {
        [SerializeField] ConvolutionCoresVisualiser visualiser;

        public ConvolutionCoresController coresController;

        public Color32[,] pictureMatrix;

        bool finished = false;
        public bool Finished => finished;

        List<ConvolutedLayer> layers = new List<ConvolutedLayer>();
        List<int> coresIDs = new List<int>();

        Action currentAction = null;

        int currentStep = 0;
        int maxSteps = 0;

        void Awake()
        {
            Init();
        }

        public void Init()
        {
            layers.Clear();
            coresIDs.Clear();
        }

        public void InitAction(Action a)
        {
            SetAction(a);
            CreateConvolutedLayers();

            currentStep = 0;
            maxSteps = 0;
        }

        public Action GetCurrentAction()
        {
            return currentAction;
        }

        void SetAction(Action a)
        {
            currentAction = a;
        }

        public void Convolute()
        {
            if(currentStep >= maxSteps)
            {
                finished = true;
                currentAction = null;
                return;
            }

            NextStep();
        }

        void NextStep()
        {

        }

        public void CalculateConvolutionCoresIDs(string interval)
        {
            if (interval == "")
            {
                Debug.LogWarning("Пустой диапазон значений!");
                return;
            }

            coresIDs.Clear();

            string[] IDs = interval.Split(',');

            for (int i = 0; i < IDs.Length; i++)
            {
                string[] values = IDs[i].Split('-');

                if (values.Length == 1)
                    coresIDs.Add(int.Parse(values[0]));
                else if (values.Length == 2)
                {
                    int x = int.Parse(values[0]);
                    int y = int.Parse(values[1]);

                    for (int z = Mathf.Min(x, y); z <= Mathf.Abs(x - y); z++)
                    {
                        coresIDs.Add(z);
                    }
                }
                else
                    Debug.LogWarning("Недопустимый диапазон значений!");
            }
        }

        void CreateConvolutedLayers()
        {
            for (int i = 0; i < coresIDs.Count; i++)
            {
                CreateLayer(coresController.convolutionCores[coresIDs[i]]);
            }
        }

        void CreateLayer(ConvolutionCore core)
        {
            ConvolutedLayer layer = new ConvolutedLayer();
            layer.Core = core;
            layer.CreateMatrix(pictureMatrix);
        }
    }
}