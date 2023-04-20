using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Maskayop
{
    public class CNN : MonoBehaviour
    {
        public TextMeshProUGUI spentTimeText;
        public float timeStep = 1.0f;
        public float timePassed = 0;

        public PictureSplitter pictureSplitter;
        public PicturePreparator picturePreparator;
        public ImagesCreator imagesCreator;
        public ConvolutionCoresController coresController;
        public Convoluter convoluter;

        public List<Action> actions = new List<Action>();
        public int currentAction = 0;

        Color32[,] currentMatrix;
        public Color32[,] Matrix => currentMatrix;

        bool go = false;
        float currentTime = 0;

        void FixedUpdate()
        {
            if (timeStep <= 0)
            {
                Debug.LogWarning("Time Step не может быть равен или меньше 0");
                timeStep = 1.0f;
                return;
            }

            if (currentAction >= actions.Count)
                return;

            if (go && currentTime < 0)
            {
                DoNextStep();
                currentTime = timeStep;
            }

            if (go)
            {
                currentTime -= Time.deltaTime;
                timePassed += Time.deltaTime;
            }
        }

        void DoNextStep()
        {
            switch (actions[currentAction].action)
            {
                case Action.ActionType.PictureSplitting:
                    SplitPicture();
                    break;
                case Action.ActionType.PicturePreparation:
                    PreparePicture();
                    break;
                case Action.ActionType.Convolution:
                    Convolute();
                    break;
            }

            spentTimeText.text = Mathf.Round(timePassed).ToString();
        }

        public void Go(bool state)
        {
            go = state;
        }

        void SplitPicture()
        {
            if (!pictureSplitter.Finished)
            {
                pictureSplitter.Init();
                currentMatrix = pictureSplitter.pictureMatrix;

                if (actions[currentAction].visualize)
                    imagesCreator.CreatePictureImages(pictureSplitter.height, pictureSplitter.width, currentMatrix, "Splitted Picture");
            }
            else
                currentAction++;
        }

        void PreparePicture()
        {
            if (!picturePreparator.Finished)
            {
                picturePreparator.padding = actions[currentAction].additionalPadding;
                picturePreparator.pictureMatrix = currentMatrix;
                picturePreparator.Init();
                currentMatrix = picturePreparator.Matrix;

                if (actions[currentAction].visualize)
                    imagesCreator.CreatePictureImages(picturePreparator.Matrix.GetLength(0), picturePreparator.Matrix.GetLength(1), currentMatrix, 
                                                      new string("Prepared Picture " + currentAction.ToString()));
            }
            else
                currentAction++;
        }

        void Convolute()
        {
            if (!convoluter.Finished)
            {
                if(convoluter.GetCurrentAction() != actions[currentAction])
                {
                    convoluter.coresController = coresController;
                    convoluter.CalculateConvolutionCoresIDs(actions[currentAction].coresInterval);
                    convoluter.pictureMatrix = currentMatrix;
                    convoluter.InitAction(actions[currentAction]);
                }

                convoluter.Convolute();
            }
            else
                currentAction++;
        }
    }
}