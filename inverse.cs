using System;

class MatrixInverse
{
    static double[,] Inverse(int[,] A)
    {
        int n = A.GetLength(0);
        if (n != A.GetLength(1))
        {
            Console.WriteLine("Only square matrices can have an inverse.");
            return null;
        }

        // Convert int[,] to double[,] and create augmented matrix [A | I]
        double[,] aug = new double[n, 2 * n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                aug[i, j] = A[i, j];  // Copy original matrix
            for (int j = 0; j < n; j++)
                aug[i, j + n] = (i == j) ? 1 : 0;  // Identity matrix
        }

        // Gauss-Jordan elimination
        for (int i = 0; i < n; i++)
        {
            // Make pivot = 1
            double pivot = aug[i, i];
            if (pivot == 0)
            {
                Console.WriteLine("Matrix is singular, inverse does not exist.");
                return null;
            }
            for (int j = 0; j < 2 * n; j++)
                aug[i, j] /= pivot;

            // Make other elements in this column = 0
            for (int k = 0; k < n; k++)
            {
                if (k != i)
                {
                    double factor = aug[k, i];
                    for (int j = 0; j < 2 * n; j++)
                        aug[k, j] -= factor * aug[i, j];
                }
            }
        }

        // Extract inverse from right half of augmented matrix
        double[,] inv = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                inv[i, j] = aug[i, j + n];

        return inv;
    }

    // For testing
    static void PrintMatrix(double[,] M)
    {
        int n = M.GetLength(0);
        int m = M.GetLength(1);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
                Console.Write($"{M[i, j]:F2}\t");
            Console.WriteLine();
        }
    }

    static void Main()
    {
        int[,] matrix = {
            {2, 1, 1},
            {1, 3, 2},
            {1, 0, 0}
        };

        double[,] inv = Inverse(matrix);

        if (inv != null)
        {
            Console.WriteLine("Inverse matrix:");
            PrintMatrix(inv);
        }
    }
}