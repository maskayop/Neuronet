using System.Collections.Generic;
using System;
using UnityEngine;

namespace Maskayop
{
    [Serializable]
    public class ConvolutionCore
    {
        public string name;
        public int size = 3;

        public enum FilterColor { Red, Green, Blue, Alpha }
        public FilterColor filterColor;

        public List<int> values = new List<int>();
        public List<GameObject> cells = new List<GameObject>();
    }
}