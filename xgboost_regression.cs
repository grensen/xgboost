using System;
using System.Collections.Generic;
using System.Linq;

public class DecisionTreeXG
{
    public float SplitValue { get; set; }
    public int SplitFeature { get; set; }
    public float LeftValue { get; set; }
    public float RightValue { get; set; }
    public DecisionTreeXG Left { get; set; }
    public DecisionTreeXG Right { get; set; }
    public bool IsLeaf { get; set; }

    public float Predict(float[] features)
    {
        if (IsLeaf)
            return LeftValue; // Prediction value at the leaf node

        if (features[SplitFeature] < SplitValue)
            return Left?.Predict(features) ?? LeftValue;
        else
            return Right?.Predict(features) ?? RightValue;
    }

    public void Train(float[][] X, float[] y, int depth = 0, int maxDepth = 3, int minSamplesSplit = 2, float regularization = 1e-4f)
    {
        int nSamples = X.Length;
        int nFeatures = X[0].Length;

        // Stop condition for recursive splitting
        if (depth >= maxDepth || nSamples < minSamplesSplit)
        {
            IsLeaf = true;
            LeftValue = Mean(y);
            return;
        }

        int bestFeature = 0;
        float bestSplit = 0;
        float bestGain = float.NegativeInfinity;

        for (int featureIndex = 0; featureIndex < nFeatures; featureIndex++)
        {
            var possibleSplits = X.Select(sample => sample[featureIndex]).Distinct().OrderBy(v => v).ToArray();

            foreach (var splitValue in possibleSplits)
            {
                var (leftX, rightX, leftY, rightY) = SplitData(X, y, featureIndex, splitValue);

                if (leftY.Length == 0 || rightY.Length == 0) continue;

                float gain = CalculateGain(y, leftY, rightY, regularization);

                if (gain > bestGain)
                {
                    bestGain = gain;
                    bestFeature = featureIndex;
                    bestSplit = splitValue;
                }
            }
        }

        // If no gain improvement, stop splitting
        if (bestGain == float.NegativeInfinity)
        {
            IsLeaf = true;
            LeftValue = Mean(y);
            return;
        }

        // Assign the best split found
        SplitFeature = bestFeature;
        SplitValue = bestSplit;

        // Perform the split using the best feature and split value
        var (bestLeftX, bestRightX, bestLeftY, bestRightY) = SplitData(X, y, bestFeature, bestSplit);

        // Recursively train the left and right subtrees
        Left = new DecisionTreeXG();
        Right = new DecisionTreeXG();

        Left.Train(bestLeftX, bestLeftY, depth + 1, maxDepth, minSamplesSplit, regularization);
        Right.Train(bestRightX, bestRightY, depth + 1, maxDepth, minSamplesSplit, regularization);

        IsLeaf = false;
    }

    private (float[][], float[][], float[], float[]) SplitData(float[][] X, float[] y, int featureIndex, float splitValue)
    {
        var leftX = new List<float[]>();
        var rightX = new List<float[]>();
        var leftY = new List<float>();
        var rightY = new List<float>();

        for (int i = 0; i < X.Length; i++)
        {
            if (X[i][featureIndex] < splitValue)
            {
                leftX.Add(X[i]);
                leftY.Add(y[i]);
            }
            else
            {
                rightX.Add(X[i]);
                rightY.Add(y[i]);
            }
        }

        return (leftX.ToArray(), rightX.ToArray(), leftY.ToArray(), rightY.ToArray());
    }

    private float CalculateGain(float[] parent, float[] left, float[] right, float regularization)
    {
        float parentVar = Variance(parent);
        float leftVar = Variance(left);
        float rightVar = Variance(right);

        // Weighted sum of variances, subtract regularization term
        float gain = parentVar - (left.Length / (float)parent.Length) * leftVar - (right.Length / (float)parent.Length) * rightVar;
        return gain - regularization;
    }

    private float Variance(float[] values)
    {
        float mean = Mean(values);
        float variance = 0;
        foreach (var value in values)
            variance += (value - mean) * (value - mean);
        return variance / values.Length;
    }

    private float Mean(float[] values)
    {
        float sum = 0;
        foreach (var value in values) sum += value;
        return sum / values.Length;
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Example data
        float[][] X = new float[][] {
            new float[] {1.0f, 2.0f},
            new float[] {2.0f, 3.0f},
            new float[] {3.0f, 4.0f},
            new float[] {4.0f, 5.0f},
            new float[] {5.0f, 6.0f},
        };
        float[] y = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

        // Create the model
        DecisionTreeXG model = new DecisionTreeXG();

        // Train the model
        model.Train(X,y);

        // Predict a new sample
        float[] x_new = new float[] { 6.0f, 7.0f };
        float prediction = model.Predict(x_new);

        Console.WriteLine($"Prediction: {prediction}");
    }
}
