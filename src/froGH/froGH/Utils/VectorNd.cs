using System;

namespace froGH.Utils
{
    internal struct VectorNd
    {
        public double[] values;

        public VectorNd(double[] values)
        {
            this.values = values;
        }

        public VectorNd(int nDimensions)
        {
            values = new double[nDimensions];
        }

        public double DistanceTo(VectorNd other)
        {
            if (other.values.Length != values.Length) return -1;
            else return Math.Sqrt(DistanceToSquared(other));
        }

        public double DistanceToSquared(VectorNd other)
        {
            if (other.values.Length != values.Length) return -1;
            double distance = 0;
            for (int i = 0; i < values.Length; i++)
            {
                distance += values[i] * values[i] + other.values[i] * other.values[i];
            }
            return distance;
        }

        public static VectorNd operator +(VectorNd a, VectorNd b)
        {
            int nDim = Math.Max(a.values.Length, b.values.Length);
            VectorNd sum = new VectorNd(nDim);
            double first, second;
            for (int i = 0; i < nDim; i++)
            {
                if (i < a.values.Length - 1)
                    first = a.values[i];
                else first = a.values[a.values.Length - 1];
                if (i < b.values.Length - 1)
                    second = b.values[i];
                else second = b.values[b.values.Length - 1];

                sum.values[i] = first + second;
            }

            return sum;
        }

        public static VectorNd operator -(VectorNd a, VectorNd b)
        {
            int nDim = Math.Max(a.values.Length, b.values.Length);
            VectorNd diff = new VectorNd(nDim);
            double first, second;
            for (int i = 0; i < nDim; i++)
            {
                if (i < a.values.Length - 1)
                    first = a.values[i];
                else first = a.values[a.values.Length - 1];
                if (i < b.values.Length - 1)
                    second = b.values[i];
                else second = b.values[b.values.Length - 1];

                diff.values[i] = first - second;
            }

            return diff;
        }

        public static VectorNd operator *(VectorNd a, VectorNd b)
        {
            int nDim = Math.Max(a.values.Length, b.values.Length);
            VectorNd dot = new VectorNd(nDim);
            double first, second;
            for (int i = 0; i < nDim; i++)
            {
                if (i < a.values.Length - 1)
                    first = a.values[i];
                else first = a.values[a.values.Length - 1];
                if (i < b.values.Length - 1)
                    second = b.values[i];
                else second = b.values[b.values.Length - 1];

                dot.values[i] = first * second;
            }

            return dot;
        }

        public static VectorNd operator *(VectorNd a, double num)
        {
            for (int i = 0; i < a.values.Length; i++)
                a.values[i] *= num;

            return a;
        }

        public static VectorNd operator /(VectorNd a, double num)
        {
            for (int i = 0; i < a.values.Length; i++)
                a.values[i] /= num;

            return a;
        }
    }
}
