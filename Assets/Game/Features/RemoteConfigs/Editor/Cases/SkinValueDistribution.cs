using System;
using System.Collections.Generic;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class SkinValueDistribution
    {
        public double Mean { get; }
        public double MeanOfSquares { get; }
        public double Min { get; }
        public double Max { get; }

        private readonly double[] _values;
        private readonly double[] _tailProbabilities;

        public SkinValueDistribution(IReadOnlyList<double> values, IReadOnlyList<double> probabilities, double min, double max)
        {
            var order = new int[values.Count];

            for (var index = 0; index < order.Length; index++)
            {
                order[index] = index;
            }

            Array.Sort(order, (left, right) => values[left].CompareTo(values[right]));

            _values = new double[order.Length];
            _tailProbabilities = new double[order.Length];

            var tail = 0d;

            for (var index = order.Length - 1; index >= 0; index--)
            {
                _values[index] = values[order[index]];
                tail += probabilities[order[index]];
                _tailProbabilities[index] = tail;
            }

            var mean = 0d;
            var meanOfSquares = 0d;

            for (var index = 0; index < values.Count; index++)
            {
                mean += values[index] * probabilities[index];
                meanOfSquares += values[index] * values[index] * probabilities[index];
            }

            Mean = mean;
            MeanOfSquares = meanOfSquares;
            Min = min;
            Max = max;
        }

        public double GetProbabilityAtLeast(double value)
        {
            var index = Array.BinarySearch(_values, value);

            if (index < 0)
            {
                index = ~index;
            }
            else
            {
                while (index > 0 && _values[index - 1] >= value)
                {
                    index--;
                }
            }

            if (index >= _values.Length)
            {
                return 0d;
            }

            return _tailProbabilities[index];
        }
    }
}
