using UnityEngine;

namespace Maskayop
{
    public class PicturePreparator : MonoBehaviour
    {
        public int padding = 0;
        public Color32[,] pictureMatrix;

        Color32[,] currentMatrix;
        public Color32[,] Matrix => currentMatrix;

        bool finished = false;
        public bool Finished => finished;

        int height = 0;
        int width = 0;

        public void Init()
        {
            height = pictureMatrix.GetLength(0);
            width = pictureMatrix.GetLength(1);

            if (padding != 0)
                AddPaddings();

            finished = true;
        }

        void AddPaddings()
        {
            height = height + padding * 2;
            width = width + padding * 2;

            currentMatrix = new Color32[height, width];

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    if (h < padding || w < padding || h >= height - padding || w >= width - padding)
                        currentMatrix[h, w] = new Color32(0, 0, 0, 255);
                    else
                        currentMatrix[h, w] = pictureMatrix[h - padding, w - padding];
                }
            }
        }
    }
}