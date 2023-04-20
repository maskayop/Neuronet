using System;
using UnityEngine;

namespace Maskayop
{
    [Serializable]
    public class ConvolutedLayer
    {
        public string Name;
        public ConvolutionCore Core;
        public int[,] Matrix;
        public int Height = 0;
        public int Width = 0;
        public int Count = 0;

        public void CreateMatrix(Color32[,] matrix)
        {

        }
    }
}