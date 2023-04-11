using System;

namespace Maskayop
{
    [Serializable]
    public class Action
    {
        public enum ActionType 
        { 
            PictureSplitting,
            Convolution,
            MaxPooling
        }

        public ActionType action;
        public bool visualize = false;
    }
}