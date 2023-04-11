using UnityEngine;
using TMPro;

namespace Neurotenwork
{
    public class CNN : MonoBehaviour
    {
        public TextureParser textureParser;

        public float timeStep = 1.0f;
        public TextMeshProUGUI testTimeText;

        bool go = false;
        float currentTime = 0;
        
        public float timePassed = 0;

        void Start()
        {
            textureParser.CNN = this;
        }

        void FixedUpdate()
        {
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
            textureParser.NextStep();
            
            testTimeText.text = timePassed.ToString();
        }

        public void Go(bool state)
        {
            go = state;
        }
    }
}