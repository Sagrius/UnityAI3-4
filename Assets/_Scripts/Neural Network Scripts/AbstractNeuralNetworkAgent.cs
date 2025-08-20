using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// Abstract base for neural-network-based AI agents with evolutionary features and persistence.
/// </summary>
public abstract class AbstractNeuralNetworkAgent : MonoBehaviour, IComparable<AbstractNeuralNetworkAgent>
{
    // --- Core data ---
    protected float[] InputValues; 
    protected float[] OutputValues; 

    /// <summary>
    /// Sequence of weight matrices: each is (outputSize x inputSize).
    /// </summary>
    protected List<Matrix<float>> CalculationSequence = new List<Matrix<float>>();

    /// <summary>
    /// Optional biases for each layer (must align with CalculationSequence).
    /// </summary>
    protected List<Vector<float>> BiasSequence = new List<Vector<float>>();

    // --- Evolution parameters ---
    /// <summary>Probability per weight/bias element to mutate.</summary>
    public float MutationRate { get; set; } = 0.01f;

    // --- Cached fitness score ---
    public float CachedScore { get; private set; } = float.NaN;

    // --- Input/output adaptation internals ---
    private Matrix<float> _inputProjection;
    private Matrix<float> _outputProjection;
    private Vector<float> _outputProjectionBias;

    protected bool AllowInputPadTruncate = true;
    protected bool UseInputProjectionIfMismatch = true;
    protected bool UseOutputProjectionIfMismatch = true;

    #region Public API

    /// <summary>
    /// Set the input array. Must not be null. Size mismatches will be handled per adaptation flags.
    /// </summary>
    public void SetInput(float[] input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        InputValues = new float[input.Length];
        Array.Copy(input, InputValues, input.Length);
    }

    /// <summary>
    /// Returns a copy of the last produced output.
    /// </summary>
    public float[] GetOutputCopy()
    {
        if (OutputValues == null) return null;
        var copy = new float[OutputValues.Length];
        Array.Copy(OutputValues, copy, OutputValues.Length);
        return copy;
    }

    /// <summary>
    /// Define a fixed sequence of weight matrices and optional biases. Optionally validate expected input size.
    /// </summary>
    public void SetSequence(List<Matrix<float>> matrices, List<Vector<float>> biases = null, int expectedInputSize = -1)
    {
        if (matrices == null || matrices.Count == 0)
            throw new ArgumentException("Matrix sequence cannot be null or empty.");
        CalculationSequence = new List<Matrix<float>>(matrices);

        for (int i = 0; i < CalculationSequence.Count - 1; i++)
        {
            int outSize = CalculationSequence[i].RowCount;
            int nextIn = CalculationSequence[i + 1].ColumnCount;
            if (outSize != nextIn)
                throw new InvalidOperationException($"Layer {i} output ({outSize}) != layer {i + 1} input ({nextIn}).");
        }

        if (expectedInputSize >= 0)
        {
            int firstIn = CalculationSequence[0].ColumnCount;
            if (firstIn != expectedInputSize)
                throw new InvalidOperationException($"Expected input size {expectedInputSize} != first matrix input {firstIn}.");
        }

        if (biases != null)
        {
            if (biases.Count != matrices.Count)
                throw new ArgumentException("Bias count must match matrix count.");
            BiasSequence = new List<Vector<float>>(biases);
            for (int i = 0; i < BiasSequence.Count; i++)
            {
                if (BiasSequence[i].Count != CalculationSequence[i].RowCount)
                    throw new InvalidOperationException($"Bias at layer {i} size {BiasSequence[i].Count} != output size {CalculationSequence[i].RowCount}.");
            }
        }
        else
        {
            BiasSequence = new List<Vector<float>>();
        }

        _inputProjection = null;
        _outputProjection = null;
        _outputProjectionBias = null;
    }

    /// <summary>
    /// Generate a random sequence (weights + zero biases) given layer sizes.
    /// Example: layerSizes = [inputDim, hidden, outputDim]
    /// </summary>
    protected void GenerateRandomSequence(int[] layerSizes, float weightInitScale = 1f, int? seed = null)
    {
        if (layerSizes == null || layerSizes.Length < 2)
            throw new ArgumentException("layerSizes must have at least two elements.");

        CalculationSequence.Clear();
        BiasSequence.Clear();
        var rnd = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            int inSize = layerSizes[i];
            int outSize = layerSizes[i + 1];
            var W = Matrix<float>.Build.Dense(outSize, inSize,
                (r, c) => (float)((rnd.NextDouble() * 2.0 - 1.0) * weightInitScale));
            CalculationSequence.Add(W);
            BiasSequence.Add(Vector<float>.Build.Dense(outSize, 0f));
        }

        _inputProjection = null;
        _outputProjection = null;
        _outputProjectionBias = null;
    }

    /// <summary>
    /// Forward evaluation. Optionally specify the desired output size (will trigger adaptation).
    /// </summary>
    public void Evaluate(int desiredOutputSize = -1)
    {
        if (InputValues == null)
            throw new InvalidOperationException("InputValues is null. Call SetInput first.");
        if (CalculationSequence.Count == 0)
            throw new InvalidOperationException("CalculationSequence is empty.");

        var current = Vector<float>.Build.Dense(InputValues);

        int expectedIn = CalculationSequence[0].ColumnCount;
        if (current.Count != expectedIn)
        {
            if (UseInputProjectionIfMismatch)
            {
                if (_inputProjection == null || _inputProjection.ColumnCount != current.Count || _inputProjection.RowCount != expectedIn)
                {
                    _inputProjection = Matrix<float>.Build.Dense(expectedIn, current.Count,
                        (r, c) => UnityEngine.Random.Range(-1f, 1f) * 0.1f);
                }
                current = _inputProjection * current;
            }
            else if (AllowInputPadTruncate)
            {
                var adapted = Vector<float>.Build.Dense(expectedIn);
                int copy = Math.Min(current.Count, expectedIn);
                for (int i = 0; i < copy; i++) adapted[i] = current[i];
                current = adapted;
            }
            else
            {
                throw new InvalidOperationException($"Input length {current.Count} != expected {expectedIn} and adaptation disabled.");
            }
        }

        for (int i = 0; i < CalculationSequence.Count; i++)
        {
            current = CalculationSequence[i] * current;
            if (BiasSequence.Count > i && BiasSequence[i] != null)
                current += BiasSequence[i];
            current = ApplyActivation(current, i);
        }

        if (desiredOutputSize >= 0 && current.Count != desiredOutputSize)
        {
            if (UseOutputProjectionIfMismatch)
            {
                if (_outputProjection == null || _outputProjection.RowCount != desiredOutputSize || _outputProjection.ColumnCount != current.Count)
                {
                    _outputProjection = Matrix<float>.Build.Dense(desiredOutputSize, current.Count,
                        (r, c) => UnityEngine.Random.Range(-1f, 1f) * 0.1f);
                    _outputProjectionBias = Vector<float>.Build.Dense(desiredOutputSize, 0f);
                }
                current = _outputProjection * current;
                current += _outputProjectionBias;
            }
            else
            {
                var adapted = Vector<float>.Build.Dense(desiredOutputSize);
                int copy = Math.Min(current.Count, desiredOutputSize);
                for (int i = 0; i < copy; i++) adapted[i] = current[i];
                current = adapted;
            }
        }

        OutputValues = current.ToArray();
    }

    /// <summary>
    /// Convenience: set input, evaluate, and get output copy.
    /// </summary>
    public float[] EvaluateWithInput(float[] input, int desiredOutputSize = -1)
    {
        SetInput(input);
        Evaluate(desiredOutputSize);
        return GetOutputCopy();
    }

    /// <summary>
    /// Mutate the current weight/bias sequence in place using MutationRate.
    /// </summary>
    public void MutateSequence()
    {
        var rnd = new System.Random();
        for (int l = 0; l < CalculationSequence.Count; l++)
        {
            var W = CalculationSequence[l];
            for (int r = 0; r < W.RowCount; r++)
                for (int c = 0; c < W.ColumnCount; c++)
                    if (rnd.NextDouble() < MutationRate)
                        W[r, c] += (float)((rnd.NextDouble() * 2.0 - 1.0) * MutationRate);

            if (BiasSequence.Count > l && BiasSequence[l] != null)
            {
                var b = BiasSequence[l];
                for (int i = 0; i < b.Count; i++)
                    if (rnd.NextDouble() < MutationRate)
                        b[i] += (float)((rnd.NextDouble() * 2.0 - 1.0) * MutationRate);
            }
        }
    }

    /// <summary>
    /// Uniform crossover: combine two parents’ weight matrices into a child sequence.
    /// </summary>
    public static List<Matrix<float>> CrossoverSequences(AbstractNeuralNetworkAgent A, AbstractNeuralNetworkAgent B, float crossoverRate)
    {
        if (A.CalculationSequence.Count != B.CalculationSequence.Count)
            throw new InvalidOperationException("Parent sequences must match length.");

        var rnd = new System.Random();
        var child = new List<Matrix<float>>(A.CalculationSequence.Count);
        for (int l = 0; l < A.CalculationSequence.Count; l++)
        {
            var WA = A.CalculationSequence[l];
            var WB = B.CalculationSequence[l];
            if (WA.RowCount != WB.RowCount || WA.ColumnCount != WB.ColumnCount)
                throw new InvalidOperationException("Layer shape mismatch between parents.");

            var Wc = Matrix<float>.Build.Dense(WA.RowCount, WA.ColumnCount);
            for (int r = 0; r < WA.RowCount; r++)
                for (int c = 0; c < WA.ColumnCount; c++)
                    Wc[r, c] = (rnd.NextDouble() < crossoverRate) ? WA[r, c] : WB[r, c];
            child.Add(Wc);
        }
        return child;
    }

    #endregion

    #region Evolutionary Evaluation

    /// <summary>
    /// Subclass must compute fitness/score based on current state. Does not auto-cache; use ComputeAndCacheScore to store.
    /// </summary>
    public abstract float EvaluateScore();

    /// <summary>
    /// Computes score via EvaluateScore and caches it in <see cref="CachedScore"/>.</summary>
    public float ComputeAndCacheScore()
    {
        CachedScore = EvaluateScore();
        return CachedScore;
    }

    public void SetCachedScore(float score)
    {
        CachedScore = score;
    }

    public int CompareTo(AbstractNeuralNetworkAgent other)
    {
        return EvaluateScore().CompareTo(other.EvaluateScore());
    }

    public static bool operator >(AbstractNeuralNetworkAgent a, AbstractNeuralNetworkAgent b) => a.EvaluateScore() > b.EvaluateScore();
    public static bool operator <(AbstractNeuralNetworkAgent a, AbstractNeuralNetworkAgent b) => a.EvaluateScore() < b.EvaluateScore();
    public static bool operator >=(AbstractNeuralNetworkAgent a, AbstractNeuralNetworkAgent b) => a.EvaluateScore() >= b.EvaluateScore();
    public static bool operator <=(AbstractNeuralNetworkAgent a, AbstractNeuralNetworkAgent b) => a.EvaluateScore() <= b.EvaluateScore();

    #endregion

    #region Activation Hook

    /// <summary>
    /// Override to inject nonlinearity per layer. Default is identity.</summary>
    protected virtual Vector<float> ApplyActivation(Vector<float> v, int layerIndex)
    {
        return v;
    }

    #endregion

    #region JSON Persistence

    [Serializable]
    private class NeuralNetworkData
    {
        public List<LayerData> layers;
        public float cachedScore;
    }

    [Serializable]
    private class LayerData
    {
        public int rows;
        public int columns;
        public float[][] weights;
        public float[] bias;
    }

    /// <summary>
    /// Serializes weights, biases, and cached score to JSON.</summary>
    public string ToJson()
    {
        var data = new NeuralNetworkData
        {
            layers = new List<LayerData>(CalculationSequence.Count),
            cachedScore = CachedScore
        };

        for (int i = 0; i < CalculationSequence.Count; i++)
        {
            var W = CalculationSequence[i];
            var layer = new LayerData
            {
                rows = W.RowCount,
                columns = W.ColumnCount,
                weights = new float[W.RowCount][],
                bias = new float[W.RowCount]
            };

            for (int r = 0; r < W.RowCount; r++)
            {
                layer.weights[r] = new float[W.ColumnCount];
                for (int c = 0; c < W.ColumnCount; c++)
                    layer.weights[r][c] = W[r, c];
            }

            if (BiasSequence.Count > i && BiasSequence[i] != null)
                layer.bias = BiasSequence[i].ToArray();

            data.layers.Add(layer);
        }

        return JsonUtility.ToJson(data, prettyPrint: true);
    }

    /// <summary>
    /// Loads weights, biases, and cached score from JSON.</summary>
    public void FromJson(string json)
    {
        var data = JsonUtility.FromJson<NeuralNetworkData>(json);
        CalculationSequence.Clear();
        BiasSequence.Clear();

        foreach (var ld in data.layers)
        {
            var W = Matrix<float>.Build.DenseOfRowArrays(ld.weights);
            CalculationSequence.Add(W);
            var b = Vector<float>.Build.Dense(ld.bias);
            BiasSequence.Add(b);
        }

        CachedScore = data.cachedScore;

        _inputProjection = null;
        _outputProjection = null;
        _outputProjectionBias = null;
    }

    /// <summary>
    /// Writes the current network + cached score to disk.</summary>
    public void SaveToFile(string filePath)
    {
        var json = ToJson();
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads the network + cached score from disk.</summary>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        var json = File.ReadAllText(filePath);
        FromJson(json);
    }

    #endregion
}
