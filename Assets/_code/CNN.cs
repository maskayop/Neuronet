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
        public ImagesCreator imagesCreator;
        public Convoluter convoluter;

        public List<Action> actions = new List<Action>();
        public int currentAction = 0;

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

                if (actions[currentAction].visualize)
                    imagesCreator.CreatePictureImages(pictureSplitter.height, pictureSplitter.width, pictureSplitter.pictureMatrix);
            }
            else
                currentAction++;
        }

        void Convolute()
        {
            currentAction++;
        }
    }
}