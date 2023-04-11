using Neurotenwork;
using System.Collections.Generic;
using UnityEngine;

namespace Neurotenwork
{
    public class ConvolutedLayer : ScriptableObject
    {
        public ConvolutionCore core;
        public List<int> convolutedLayerValues = new List<int>();
    }
}