/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SetupLib;

// Specifies for the configuration of a Neuron Layer
public class GANeuronLayerConfigDto
{
    public int[] NbrOutputs { get; set; }

    public string[] ActivationFunction { get; set; }

    public double[] InitialWeightRange { get; set; }
}
