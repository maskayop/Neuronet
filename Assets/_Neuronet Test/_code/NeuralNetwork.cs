using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neurotenwork
{
    [Serializable]
    public class Input
    {
        public string Name;
        public float Value;
    }

    [Serializable]
    public class Output
    {
        public string Name;
        public float Ideal;
        public float In;
        public float Out;
        public float Delta;
    }

    [Serializable]
    public class Neuron
    {
        public string Name;
        public float In;
        public float Out;
        public float Delta;
    }

    [Serializable]
    public class Synapse
    {
        public string Name;
        public float Weight;
        public float Delta;
        public float Gradient;
        public float DeltaPrevious;
    }

    public class NeuralNetwork : MonoBehaviour
    {
        public float speed = 0.7f;
        public float moment = 0.3f;
        public int neuronsCount = 2;

        public float Error;

        public List<Input> inputs = new List<Input>();
        public List<Output> outputs = new List<Output>();

        public List<Neuron> neurons = new List<Neuron>();
        public List<Synapse> inputSynapses = new List<Synapse>();
        public List<Synapse> outputSynapses = new List<Synapse>();

        public void Init()
        {
            neurons.Clear();
            inputSynapses.Clear();
            outputSynapses.Clear();

            CreateNeurons();
            CreateSynapses();
        }

        void CreateNeurons()
        {
            for (int i = 0; i < neuronsCount; i++)
            {
                Neuron newNeuron = new Neuron();
                newNeuron.Name = "N" + (i + 1).ToString();
                neurons.Add(newNeuron);
            }    
        }

        void CreateSynapses()
        {
            for (int n = 0; n < neuronsCount; n++)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    Synapse newSynapse = new Synapse();
                    newSynapse.Name = inputs[i].Name + " - " + "N" + (n + 1).ToString();
                    newSynapse.Weight = GetRandomWeight();
                    inputSynapses.Add(newSynapse);
                }

                for (int o = 0; o < outputs.Count; o++)
                {
                    Synapse newSynapse = new Synapse();
                    newSynapse.Name = "N" + (n + 1).ToString() + " - " + outputs[o].Name;
                    newSynapse.Weight = GetRandomWeight();
                    outputSynapses.Add(newSynapse);
                }
            }
        }

        float GetRandomWeight()
        {
            return UnityEngine.Random.Range(-1.0f, 1.0f);
        }

        public void Calculate()
        {
            CalculateNeuronsWeights();
            CalculateOutputsWeights();
            CalculateError();            
        }

        public void RecalculateWeights()
        {
            CalculateOutputsDeltas();
            CalculateNeuronsDeltas();
            RecalculateOutputSynapsesWeights();
            RecalculateInputSynapsesWeights();
        }

        void CalculateNeuronsWeights()
        {
            for (int n = 0; n < neuronsCount; n++)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    neurons[n].In += inputs[i].Value * inputSynapses[inputs.Count * n + i].Weight;
                }

                neurons[n].Out = Sigmoid(neurons[n].In);
            }
        }

        void CalculateOutputsWeights()
        {
            for (int o = 0; o < outputs.Count; o++)
            {
                for (int n = 0; n < neuronsCount; n++)
                {
                    outputs[o].In += neurons[n].Out * outputSynapses[neurons.Count * o + n].Weight;
                }

                outputs[o].Out = Sigmoid(outputs[o].In);
            }
        }

        void CalculateError()
        {
            Error = 0;

            for (int o = 0; o < outputs.Count; o++)
            {
                Error += (outputs[o].Ideal - outputs[o].Out) * (outputs[o].Ideal - outputs[o].Out);
            }

            Error /= outputs.Count;
        }

        void CalculateOutputsDeltas()
        {
            for (int o = 0; o < outputs.Count; o++)
            {
                outputs[o].Delta = (outputs[o].Ideal - outputs[o].Out) * SigmoidDerivative(outputs[o].Out);
            }
        }

        void CalculateNeuronsDeltas()
        {
            for (int n = 0; n < neuronsCount; n++)
            {
                float summ = 0;

                for (int o = 0; o < outputs.Count; o++)
                {
                    summ += outputs[o].Delta * outputSynapses[n * outputs.Count + o].Weight;
                }

                neurons[n].Delta = SigmoidDerivative(neurons[n].Out) * summ;
            }
        }

        void RecalculateOutputSynapsesWeights()
        {
            for (int n = 0; n < neuronsCount; n++)
            {
                for (int o = 0; o < outputs.Count; o++)
                {
                    //считаем градиенты выходных синапсов
                    outputSynapses[n * outputs.Count + o].Gradient = neurons[n].Out * outputs[o].Delta;
                    //считаем дельты
                    outputSynapses[n * outputs.Count + o].Delta = speed * outputSynapses[n * outputs.Count + o].Gradient + moment * outputSynapses[n * outputs.Count + o].DeltaPrevious;
                    //обновляем веса
                    outputSynapses[n * outputs.Count + o].Weight += outputSynapses[n * outputs.Count + o].Delta;
                    //переписываем значения предыдущих значений дельт
                    outputSynapses[n * outputs.Count + o].DeltaPrevious = outputSynapses[n * outputs.Count + o].Delta;
                }
            }
        }

        void RecalculateInputSynapsesWeights()
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                for (int n = 0; n < neuronsCount; n++)
                {
                    //считаем градиенты входных синапсов
                    inputSynapses[i * neuronsCount + n].Gradient = inputs[i].Value * neurons[n].Delta;
                    //считаем дельты
                    inputSynapses[i * neuronsCount + n].Delta = speed * inputSynapses[i * neurons.Count + n].Gradient + moment * inputSynapses[i * neurons.Count + n].DeltaPrevious;
                    //обновляем веса
                    inputSynapses[i * neuronsCount + n].Weight += inputSynapses[i * neurons.Count + n].Delta;
                    //переписываем значения предыдущих значений дельт
                    inputSynapses[i * neuronsCount + n].DeltaPrevious = inputSynapses[i * neurons.Count + n].Delta;
                }
            }
        }

        float Sigmoid(float input)
        {
            return 1 / (1 + Mathf.Pow(2.71828f, -input));
        }

        float SigmoidDerivative(float input)
        {
            return (1 - input) * input;
        }
    }
}