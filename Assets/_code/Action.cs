using System;

namespace Maskayop
{
    [Serializable]
    public class Action
    {
        public enum ActionType 
        { 
            PictureSplitting,
            PicturePreparation,
            Convolution,
            MaxPooling
        }

        public ActionType action;
        public int additionalPadding = 0;
        public string coresInterval;
        public int convolutionStep = 1;
        public bool visualize = false;
    }
}