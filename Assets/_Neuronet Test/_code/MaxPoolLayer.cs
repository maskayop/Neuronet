using System.Collections.Generic;
using UnityEngine;

namespace Neurotenwork
{
    public class MaxPoolLayer : ScriptableObject
    {
        public string layerName;
        public List<int> layerValues = new List<int>();
    }
}