using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// Runs an evolutionary loop over all GoapAgentNN instances in a scene.
/// Per generation:
///   1) Let the scene run for EpisodeSeconds
///   2) Score each agent via EvaluateScore()
///   3) Pick top-2 parents
///   4) Create full next-gen via uniform crossover + mutation
///   5) Reload scene and assign children to agents
///   6) Optionally checkpoint best networks
/// </summary>
public class EvolutionRunner : MonoBehaviour
{
    [Header("Scene & Loop")]
    [SerializeField] string trainingSceneName = "TrainingScene";
    [SerializeField] bool autoStart = true;
    [SerializeField, Min(1)] int maxGenerations = 50;
    [SerializeField, Min(0.1f)] float episodeSeconds = 30f;
    [SerializeField, Tooltip("Expected number of agents in the training scene")]
    int populationSize = 10;

    [Header("Network Architecture (for initial random nets)")]
    [Tooltip("Layer sizes including input and output. Example: 5-16-5")]
    [SerializeField] int[] layerSizes = new int[] { 5, 16, 5 };
    [SerializeField] float weightInitScale = 0.5f;

    [Header("Genetics")]
    [Range(0f, 1f)] public float crossoverRate = 0.5f;
    [Range(0f, 0.5f)] public float mutationRate = 0.01f;
    [SerializeField] bool elitismKeepParents = true; // keep top-2 unchanged in the next generation

    [Header("Checkpointing")]
    [SerializeField] int saveEveryNGenerations = 5;
    [SerializeField] string checkpointFolderName = "NN_Checkpoints";

    // --- internal state ---
    int generation = 0;
    bool sceneAssignedThisGen = false;
    List<List<Matrix<float>>> nextGenWeightBanks; // children produced at end of previous gen

    string CheckpointDir =>
        Path.Combine(Application.persistentDataPath, checkpointFolderName);

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Directory.CreateDirectory(CheckpointDir);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (autoStart)
            StartCoroutine(EvolutionLoop());
    }

    public void StartEvolution()
    {
        StartCoroutine(EvolutionLoop());
    }

    IEnumerator EvolutionLoop()
    {
        // If we are not in the training scene, load it first.
        if (SceneManager.GetActiveScene().name != trainingSceneName)
        {
            SceneManager.LoadScene(trainingSceneName);
            yield return null; // wait a frame for OnSceneLoaded
        }

        while (generation < maxGenerations)
        {
            // 1) If this is the first generation OR after a scene reload, ensure networks exist on agents
            yield return EnsurePopulationInitializedOrAssigned();

            // 2) Run the episode
            float t = 0f;
            while (t < episodeSeconds)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // 3) Score and select
            var agents = FindObjectsByType<GoapAgentNN>(FindObjectsSortMode.None);
            if (agents.Length == 0)
            {
                Debug.LogError("No GoapAgentNN agents found in scene.");
                yield break;
            }

            // Evaluate and rank
            var ranked = new List<(GoapAgentNN agent, float score)>(agents.Length);
            foreach (var a in agents)
            {
                float s = a.ComputeAndCacheScore(); // caches too
                ranked.Add((a, s));
            }
            ranked.Sort((x, y) => y.score.CompareTo(x.score)); // high to low

            var parentA = ranked[0].agent;
            var parentB = ranked[Mathf.Min(1, ranked.Count - 1)].agent;
            var bestScore = ranked[0].score;

            Debug.Log($"[GEN {generation}] Best score: {bestScore:F3} | A={parentA.name}  B={parentB.name}");

            // 4) Optionally save the champions
            if (saveEveryNGenerations > 0 && (generation % saveEveryNGenerations == 0))
            {
                string aPath = Path.Combine(CheckpointDir, $"gen{generation:000}_A.json");
                string bPath = Path.Combine(CheckpointDir, $"gen{generation:000}_B.json");
                parentA.SaveToFile(aPath);
                parentB.SaveToFile(bPath);
                Debug.Log($"[GEN {generation}] Saved champions to:\n{aPath}\n{bPath}");
            }

            // 5) Build next generation weights now (while we still have parents alive)
            nextGenWeightBanks = new List<List<Matrix<float>>>(populationSize);

            // Elitism: keep exact copies of parents (no mutation)
            int startIdx = 0;
            if (elitismKeepParents)
            {
                // Clone via crossover with self
                nextGenWeightBanks.Add(CrossoverClone(parentA));
                nextGenWeightBanks.Add(CrossoverClone(parentB));
                startIdx = 2;
            }

            for (int i = startIdx; i < populationSize; i++)
            {
                var child = AbstractNeuralNetworkAgent.CrossoverSequences(parentA, parentB, crossoverRate);
                nextGenWeightBanks.Add(child);
            }

            // 6) Reload scene; assignment happens in OnSceneLoaded
            sceneAssignedThisGen = false;
            generation++;
            SceneManager.LoadScene(trainingSceneName);

            // Wait until assignment finished
            yield return new WaitUntil(() => sceneAssignedThisGen);
        }

        Debug.Log($"Evolution finished. Generations run: {maxGenerations}.");
    }

    // Called after each reload. Assigns either random nets (gen0) or the children we cached.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(AssignOnNextFrame());
    }

    IEnumerator AssignOnNextFrame()
    {
        // Wait one frame to ensure all agents are instantiated
        yield return null;

        var agents = FindObjectsByType<GoapAgentNN>(FindObjectsSortMode.None);
        if (agents.Length == 0)
        {
            Debug.LogError("No GoapAgentNN agents found to assign.");
            sceneAssignedThisGen = true;
            yield break;
        }

        // If we have a ready next-gen set, assign + mutate.
        if (nextGenWeightBanks != null && nextGenWeightBanks.Count > 0)
        {
            if (agents.Length != nextGenWeightBanks.Count)
                Debug.LogWarning($"Agent count ({agents.Length}) != nextGen count ({nextGenWeightBanks.Count}). Assigning min(count).");

            int n = Mathf.Min(agents.Length, nextGenWeightBanks.Count);
            for (int i = 0; i < n; i++)
            {
                ApplySequenceToAgent(agents[i], nextGenWeightBanks[i], mutate: ShouldMutate(i));
            }
        }
        else
        {
            // First generation: randomize everyone
            foreach (var a in agents)
            {
                var (W, B) = CreateRandomSequence(layerSizes, weightInitScale);
                a.SetSequence(W, B); // biases included
                a.MutationRate = mutationRate;
            }
        }

        sceneAssignedThisGen = true;
    }

    // Ensures that either (a) next generation was assigned after reload, or (b) gen0 had networks.
    IEnumerator EnsurePopulationInitializedOrAssigned()
    {
        // Nothing to do: sceneAssignedThisGen is set in OnSceneLoaded after any load.
        yield return null;
    }

    // --- Helpers ---

    // Make an exact copy of an agent’s weights via crossover with itself.
    static List<Matrix<float>> CrossoverClone(AbstractNeuralNetworkAgent a)
    {
        return AbstractNeuralNetworkAgent.CrossoverSequences(a, a, 1f);
    }

    // After assigning a sequence to an agent, optionally mutate (except elites)
    void ApplySequenceToAgent(GoapAgentNN agent, List<Matrix<float>> matrices, bool mutate)
    {
        // We don’t pass expectedInputSize to avoid hard coupling; the NN adapts if needed.
        agent.SetSequence(matrices, biases: null);
        agent.MutationRate = mutationRate;
        if (mutate)
            agent.MutateSequence();
    }

    bool ShouldMutate(int index)
    {
        // If using elitism, first 2 are exact parents (no mutation)
        if (elitismKeepParents && index < 2) return false;
        return true;
    }

    // Random network factory (weights + zero biases) using MathNet
    static (List<Matrix<float>> W, List<Vector<float>> B) CreateRandomSequence(int[] sizes, float scale)
    {
        if (sizes == null || sizes.Length < 2)
            throw new ArgumentException("layerSizes must have at least two elements.");

        var rnd = new System.Random();
        var W = new List<Matrix<float>>(sizes.Length - 1);
        var B = new List<Vector<float>>(sizes.Length - 1);
        for (int i = 0; i < sizes.Length - 1; i++)
        {
            int inSize = sizes[i];
            int outSize = sizes[i + 1];
            var Wi = Matrix<float>.Build.Dense(outSize, inSize,
                (r, c) => (float)((rnd.NextDouble() * 2.0 - 1.0) * scale));
            W.Add(Wi);
            B.Add(Vector<float>.Build.Dense(outSize, 0f));
        }
        return (W, B);
    }
}
