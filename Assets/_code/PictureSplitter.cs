using UnityEngine;

namespace Maskayop
{
    public class PictureSplitter : MonoBehaviour
    {
        public int mipLevel = 0;
        public Texture2D picture;

        public int pixelsCount = 0;

        Color32[] pixels;

        public Color32[,] pictureMatrix;

        [Header("Свойства выходного изображения")]
        public int height = 0;
        public int width = 0;
        public int mipMapPixelsCount = 0;

        bool finished = false;
        public bool Finished => finished;

        public void Init()
        {
            if(mipLevel >= picture.mipmapCount - 1)
            {
                mipLevel = picture.mipmapCount - 1;
                pixels = picture.GetPixels32(mipLevel);
                height = width = pixelsCount = 1;
            }
            else
            {
                pixels = picture.GetPixels32(mipLevel);
                pixelsCount = pixels.Length;

                if(mipLevel != 0)
                {
                    height = Mathf.RoundToInt(Mathf.Sqrt((picture.height * picture.height * pixels.Length) / (picture.width * picture.height)));
                    width = Mathf.RoundToInt(Mathf.Sqrt((picture.width * picture.width * pixels.Length) / (picture.width * picture.height)));
                }
                else
                {
                    height = picture.height;
                    width = picture.width;
                }
            }

            mipMapPixelsCount = height * width;

            if(mipMapPixelsCount != pixelsCount)
            {
                Debug.LogWarning("Не получается просчитать Mip Map для изображения");
                return;
            }

            pictureMatrix = new Color32[height, width];

            for (int i = 0; i < pixelsCount; i++)
            {
                int h = Mathf.FloorToInt(i / width);
                int w = i - h * width;

                pictureMatrix[h, w] = pixels[i];
            }

            finished = true;
        }
    }
}