using UnityEngine;

public class Neuronet : MonoBehaviour
{
    public float E = 0.7f;
    public float A = 0.3f;

    public float I1 = 1;
    public float I2 = 0;

    public float W1 = 0.45f;    public float W2 = 0.78f;    public float W3 = -0.12f;
    public float W4 = 0.13f;    public float W5 = 1.5f;     public float W6 = -2.3f;

    public float H1_Input;      public float H1_Output;
    public float H2_Input;      public float H2_Output;
    public float O1_Input;      public float O1_Output;     public float O1_Ideal;

    public float dO1;   public float dH1;   public float dH2;

    public float Error;

    public float grad_W1;   public float dW1;   public float dW1_prev = 0;
    public float grad_W2;   public float dW2;   public float dW2_prev = 0;
    public float grad_W3;   public float dW3;   public float dW3_prev = 0;
    public float grad_W4;   public float dW4;   public float dW4_prev = 0;
    public float grad_W5;   public float dW5;   public float dW5_prev = 0;
    public float grad_W6;   public float dW6;   public float dW6_prev = 0;

    public void Calcuate()
    {
        H1_Input = I1 * W1 + I2 * W3;
        H1_Output = Sigmoid(H1_Input);

        H2_Input = I1 * W2 + I2 * W4;
        H2_Output = Sigmoid(H2_Input);

        O1_Input = H1_Output * W5 + H2_Output * W6;
        O1_Output = Sigmoid(O1_Input);
        O1_Ideal = 1;

        Error = Mathf.Pow((O1_Ideal - O1_Output), 2) / 1;

        dO1 = (O1_Ideal - O1_Output) * SigmoidDerivative(O1_Output);
        
        dH1 = SigmoidDerivative(H1_Output) * (W5 * dO1);

        grad_W5 = H1_Output * dO1;
        dW5 = E * grad_W5 + A * dW5_prev;
        W5 += dW5;

        dH2 = SigmoidDerivative(H2_Output) * (W6 * dO1);

        grad_W6 = H2_Output * dO1;
        dW6 = E * grad_W6 + A * dW6_prev;
        W6 += dW6;

        grad_W1 = I1 * dH1;
        grad_W2 = I1 * dH2;
        grad_W3 = I2 * dH1;
        grad_W4 = I2 * dH2;

        dW1 = E * grad_W1 + A * dW1_prev;
        dW2 = E * grad_W2 + A * dW2_prev;
        dW3 = E * grad_W3 + A * dW3_prev;
        dW4 = E * grad_W4 + A * dW4_prev;

        W1 += dW1;
        W2 += dW2;
        W3 += dW3;
        W4 += dW4;

        dW1_prev = dW1;
        dW2_prev = dW2;
        dW3_prev = dW3;
        dW4_prev = dW4;
        dW5_prev = dW5;
        dW6_prev = dW6;
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