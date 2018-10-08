using System;
namespace RBF_TIMESERIES
{
    public class RadialNetwork
    {
        private static Random rnd = null;
        private int numInput;
        private int numHidden;
        private int numOutput;
        private double[] inputs;
        private double[][] centroids;
        private double[] widths;
        private double[][] hoWeights;
        private double[] oBiases;
        private double[] outputs;

        public int NumInput
        {
            get { return numInput; }
            set { numInput = value; }
        }
        public int NumHidden
        {
            get { return numHidden; }
            set { numHidden = value; }
        }
        public int NumOutput
        {
            get { return numOutput; }
            set { numOutput = value; }
        }
        public double[] Inputs
        {
            get { return inputs; }
            set { inputs = value; }
        }
        public double[][] Centroids
        {
            get { return centroids; }
            set { centroids = value; }
        }
        public double[] Widths
        {
            get { return widths; }
            set { widths = value; }
        }
        public double[][] HoWeights
        {
            get { return hoWeights; }
            set { hoWeights = value; }
        }
        public double[] OBiases
        {
            get { return oBiases; }
            set { oBiases = value; }
        }
        public double[] Outputs
        {
            get { return outputs; }
            set { outputs = value; }
        }

        public RadialNetwork(int numInput, int numHidden, int numOutput)
        {
            rnd = new Random(0);
            this.numInput = numInput;
            this.numHidden = numHidden;
            this.numOutput = numOutput;
            this.inputs = new double[numInput];
            this.centroids = MakeMatrix(numHidden, numInput);
            this.widths = new double[numHidden];
            this.hoWeights = MakeMatrix(numHidden, numOutput);
            this.oBiases = new double[numOutput];
            this.outputs = new double[numOutput];
        } // ctor
        public RadialNetwork(RadialNetwork newOne)
        {
            rnd = new Random(0);
            this.numInput = newOne.NumInput;
            this.numHidden = newOne.NumHidden;
            this.numOutput = newOne.NumOutput;
            this.inputs = newOne.Inputs;
            this.centroids = newOne.Centroids;
            this.widths = newOne.Widths;
            this.hoWeights = newOne.HoWeights;
            this.oBiases = newOne.OBiases;
            this.outputs = newOne.Outputs;
        } // ctor
        private static double[][] MakeMatrix(int rows, int cols) // helper for ctor
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < rows; ++r)
                result[r] = new double[cols];
            return result;
        }

        // -- methods related to getting and setting centroids, widths, weights, bias values ----------

        public void SetWeights(double[] weights)
        {
            // this.hoWeights has numHidden row and numOutput cols
            // this.oBiases has numOutput values
            if (weights.Length != (numHidden * numOutput) + numOutput)
                throw new Exception("Bad weights length in SetWeights");
            int k = 0; // ptr into weights
            for (int i = 0; i < numHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    this.hoWeights[i][j] = weights[k++];
            for (int i = 0; i < numOutput; ++i)
                this.oBiases[i] = weights[k++];
        }

        public double[] GetWeights()
        {
            double[] result = new double[numHidden * numOutput + numOutput];
            int k = 0;
            for (int i = 0; i < numHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    result[k++] = this.hoWeights[i][j];
            for (int i = 0; i < numOutput; ++i)
                result[k++] = this.oBiases[i];
            return result;
        }

        // put GetAllWeights() and SetAllWeights() here to fetch and set
        // centroid, width, weight, and bias values as a group

        // -- methods related to training error and test classification accuracy ----------------------
        public double Accuracy(double[][] testData)
        {
            int totalNumber = testData.Length;
            double errorTest = 0;
            double sumError = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // targets
            double[] yValues; // computed Y

            for (int i = 0; i < testData.Length; ++i)
            {
                Array.Copy(testData[i], xValues, numInput); // parse test data into x-values and t-values
                Array.Copy(testData[i], numInput, tValues, 0, numOutput);
                yValues = this.ComputeOutputs(xValues);
                //
                double ideal = testData[i][numInput];
                sumError += Math.Abs(ideal - yValues[0]);
            }
            errorTest = sumError / totalNumber;
            return errorTest;

        }

        private static int MaxIndex(double[] vector) // helper for Accuracy()
        {
            // index of largest value
            int bigIndex = 0;
            double biggestVal = vector[0];
            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] > biggestVal)
                {
                    biggestVal = vector[i]; bigIndex = i;
                }
            }
            return bigIndex;
        }

        // -- methods related to RBF network input-output mechanism -----------------------------------

        public double[] ComputeOutputs(double[] xValues)
        {
            // use centroids, widths, weights and input xValues to compute, store, and return numOutputs output values
            Array.Copy(xValues, this.inputs, xValues.Length); // place data inputs into RBF net inputs

            double[] hOutputs = new double[numHidden]; // hidden node outputs
            for (int j = 0; j < numHidden; ++j) // each hidden node
            {
                double d = EuclideanDist(inputs, centroids[j], inputs.Length); // could use a 'distSquared' approach
                                                                               //Console.WriteLine("\nHidden[" + j + "] distance = " + d.ToString("F4"));
                double r = -1.0 * (d * d) / (2 * widths[j] * widths[j]);
                double g = Math.Exp(r);
                //Console.WriteLine("Hidden[" + j + "] output = " + g.ToString("F4"));
                hOutputs[j] = g;
            }

            double[] tempResults = new double[numOutput];

            for (int k = 0; k < numOutput; ++k)
                for (int j = 0; j < numHidden; ++j)
                    tempResults[k] += (hOutputs[j] * hoWeights[j][k]); // accumulate

            for (int k = 0; k < numOutput; ++k)
                tempResults[k] += oBiases[k]; // add biases
            double[] finalOutputs = Softmax(tempResults); // scale the raw output so values sum to 1.0

            //Console.WriteLine("outputs:");
            //Helpers.ShowVector(finalOutputs, 3, finalOutputs.Length, true);
            //Console.ReadLine();

            Array.Copy(finalOutputs, this.outputs, finalOutputs.Length); // transfer computed outputs to RBF net outputs

            double[] returnResult = new double[numOutput]; // also return computed outputs for convenience
            Array.Copy(finalOutputs, returnResult, outputs.Length);
            return returnResult;
        } // ComputeOutputs



        private static double[] Softmax(double[] rawOutputs)
        {
            // helper for ComputeOutputs
            // does all output nodes at once so scale doesn't have to be re-computed each time
            // determine max output sum
            double max = rawOutputs[0];
            for (int i = 0; i < rawOutputs.Length; ++i)
                if (rawOutputs[i] > max) max = rawOutputs[i];

            // determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < rawOutputs.Length; ++i)
                scale += Math.Exp(rawOutputs[i] - max);

            double[] result = new double[rawOutputs.Length];
            for (int i = 0; i < rawOutputs.Length; ++i)
                result[i] = Math.Exp(rawOutputs[i] - max) / scale;

            return result; // now scaled so that all values sum to 1.0
        }

        // -- methods related to training: DoCentroids, DistinctIndices, AvgAbsDist, DoWidths,
        //    DoWeights, Train, Shuffle ----------------------------------------------------------

        private void DoCentroids(double[][] trainData)
        {
            // centroids are representative inputs that are relatively different
            // compute centroids using the x-value of training data
            // store into this.centroids
            int numAttempts = trainData.Length;
            int[] goodIndices = new int[numHidden];  // need one centroid for each hidden node
            double maxAvgDistance = double.MinValue; // largest average distance for a set of candidate indices
            for (int i = 0; i < numAttempts; ++i)
            {
                int[] randomIndices = DistinctIndices(numHidden, trainData.Length); // candidate indices
                double sumDists = 0.0; // sum of distances between adjacent candidates (not all candiates)
                for (int j = 0; j < randomIndices.Length - 1; ++j) // adjacent pairs only
                {
                    int firstIndex = randomIndices[j];
                    int secondIndex = randomIndices[j + 1];
                    sumDists += AvgAbsDist(trainData[firstIndex], trainData[secondIndex], numInput); // just the input terms
                }

                double estAvgDist = sumDists / numInput; // estimated average distance for curr candidates
                if (estAvgDist > maxAvgDistance) // curr candidates are far apart
                {
                    maxAvgDistance = estAvgDist;
                    Array.Copy(randomIndices, goodIndices, randomIndices.Length); // save curr candidates
                }
            } // now try a new set of candidates

            Console.WriteLine("The indices (into training data) of the centroids are:");
            Helpers.ShowVector(goodIndices, goodIndices.Length, true);

            // store copies of x-vales of data pointed to by good indices into this.centroids
            for (int i = 0; i < numHidden; ++i)
            {
                int idx = goodIndices[i]; // idx points to trainData
                for (int j = 0; j < numInput; ++j)
                {
                    this.centroids[i][j] = trainData[idx][j]; // make a copy of values
                }
            }
        } // DoCentroids

        private static double AvgAbsDist(double[] v1, double[] v2, int numTerms)
        {
            // average absolute difference distance between two vectors, first numTerms only
            // helper for computing centroids
            if (v1.Length != v2.Length)
                throw new Exception("Vector lengths not equal in AvgAbsDist()");
            double sum = 0.0;
            for (int i = 0; i < numTerms; ++i)
            {
                double delta = Math.Abs(v1[i] - v2[i]);
                sum += delta;
            }
            return sum / numTerms;
        }

        private int[] DistinctIndices(int n, int range)
        {
            // helper for ComputeCentroids()
            // generate n distinct numbers in [0, range-1] using reservoir sampling
            // assumes rnd exists
            int[] result = new int[n];
            for (int i = 0; i < n; ++i)
                result[i] = i;
            //set result giá trị mặc định từ 0-4
            for (int t = n; t < range; ++t)
            {//t duyệt từ 5 đến 23
                int m = rnd.Next(0, t + 1);// lấy ngẫu nhiên từ 0 - đến t+1
                if (m < n) result[m] = t; // m < n thì result tại m = t
            }
            return result;//kết quả là 5 chỉ số ngẫu nhiên 
        }

        private void DoWidths(double[][] centroids)
        {
            // compute widths based on centroids, store into this.widths
            // note the centroids parameter could be omitted - the intent is to make relationship clear
            // this version uses a common width which is the average dist between all centroids
            double sumOfDists = 0.0;
            int ct = 0; // could calculate number pairs instead
            for (int i = 0; i < centroids.Length - 1; ++i)
            {
                for (int j = i + 1; j < centroids.Length; ++j)
                {
                    double dist = EuclideanDist(centroids[i], centroids[j], centroids[i].Length);
                    sumOfDists += dist;//tính tổng khoảng cách Euclidean giữa các centroid
                    ++ct;
                }
            }
            double avgDist = sumOfDists / ct;// khoảng cách trung bình 
            double width = avgDist;//độ rộng = khoảng cách trung bình này

            Console.WriteLine("The common width is: " + width.ToString("F4"));

            for (int i = 0; i < this.widths.Length; ++i) // all widths the same
                this.widths[i] = width;
        }

        //private void DoWidths(double[][] centroids)
        //{
        //  // this version uses a common width which is dmax / sqrt(2*numHidden)
        //  double maxDist = double.MinValue;
        //  for (int i = 0; i < centroids.Length - 1; ++i)
        //  {
        //    for (int j = i + 1; j < centroids.Length; ++j)
        //    {
        //      double dist = EuclideanDist(centroids[i], centroids[j], centroids[i].Length);
        //      if (dist > maxDist)
        //        maxDist = dist;
        //    }
        //  }

        //  double width = maxDist / Math.Sqrt(2.0 * numHidden);
        //  Console.WriteLine("The common width is: " + width.ToString("F4"));
        //  for (int i = 0; i < this.widths.Length; ++i) // all widths the same
        //    this.widths[i] = width;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trainData"></param>
        /// <param name="maxIterations"></param>
        /// <returns></returns>
        private double[] DoWeightsWithGA(double[][] trainData, int maxIterations)
        {
            // use GA to find weights and bias values that produce a RBF network
            // that best matches training data
            // Nhin nay, 1 ca the la 1 mang 1 chieu co do dai nhu sau:
            int Dim = (numHidden * numOutput) + numOutput; // dimensions is num weights + num biases
            GAs gaAlgorithm = new GAs(100, Dim, this, maxIterations, trainData);
            gaAlgorithm.Reproduction(trainData);

            double[] bestGlobalPosition = gaAlgorithm.Population.getBestOne().Values;
            // copy best weights found into RBF network, and also return them
            this.SetWeights(bestGlobalPosition);
            double[] returnResult = new double[(numHidden * numOutput) + numOutput];
            Array.Copy(bestGlobalPosition, returnResult, bestGlobalPosition.Length);

            Console.WriteLine("The best weights and bias values found are:\n");
            Helpers.ShowVector(bestGlobalPosition, 3, 10, true);
            return returnResult;
        } // DoWeights

        private Population_NSGA DoWeightsWithNSGAII(double[][] trainData, int maxEvaluations)
        {
            int populationSize = 100;
            int numberOfObjectives = 2;
            int Dim = (numHidden * numOutput) + numOutput; // dimensions is num weights + num biases
            NSGAII nsgaIIAlgorithm = new NSGAII(populationSize, Dim, this, maxEvaluations, trainData, numberOfObjectives);
            Population_NSGA bestWeights = new Population_NSGA(populationSize);
            bestWeights = nsgaIIAlgorithm.Execute();

            ///
            Population_NSGA returnResult = new Population_NSGA(maxEvaluations);
            return returnResult;
        } // DoWeights


        public double[] TrainWithGA(double[][] trainData, int maxIterations)
        {
            // Chu y cho nay:
            Console.WriteLine("\n1. Computing " + numHidden + " centroids");
            DoCentroids(trainData); // find representative data, store their x-values into this.centroids

            Console.WriteLine("\n2. Computing a common width for each hidden node");
            DoWidths(this.centroids); // measure of how far apart centroids are
            // 2 cai trenn minh dang fix co dinh
            int numWts = (numHidden * numOutput) + numOutput;// tính tổnmaxg weights 
            Console.WriteLine("\n3. Determining " + numWts + " weights and bias values using GA algorithm");
            // Gio minh chi toi uu cai Weights nay thoi
            // Ham nay DoWeights tuong duong voi cai GA cua E
            // Cai DoWeights nay dang la PSO
            double[] bestWeights = DoWeightsWithGA(trainData, maxIterations); // use PSO to find weights that best (lowest MSE) weights and biases

            return bestWeights;
        } // Train


        public Population_NSGA TrainWithNSGAII(double[][] trainData, int maxEvaluations)
        {
            // Chu y cho nay:
            DoCentroids(trainData); // find representative data, store their x-values into this.centroids
            DoWidths(this.centroids); // measure of how far apart centroids are
            Population_NSGA bestWeights = DoWeightsWithNSGAII(trainData, maxEvaluations);
            return bestWeights;
        } // Train


        // -- The Euclidean Distance function is used by RBF ComputeOutputs and also DoWidths ----

        private static double EuclideanDist(double[] v1, double[] v2, int numTerms)
        {
            // Euclidean distance between two vectors, first numTerms only
            // helper for computing RBF outputs and computing hidden node widths
            // v1, v2 là 2 centroid liên tiếp trong bộ centroids
            //numTerms là độ dài của mỗi centroid
            if (v1.Length != v2.Length)//nếu 2 vector khác độ rộng => Exception
                throw new Exception("Vector lengths not equal in EuclideanDist()");
            double sum = 0.0;
            for (int i = 0; i < numTerms; ++i)
            {
                double delta = (v1[i] - v2[i]) * (v1[i] - v2[i]);//bình phương hiệu (v1-v2)
                sum += delta;//tổng delta của 2 vector
            }
            return Math.Sqrt(sum);//khoảng cách = căn bậc 2 của bình phương hiệu (v1-v2)
        }


    } // class RadialNetwork

    // ===========================================================================

    public class Helpers
    {
        public static double[][] MakeMatrix(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        public static void ShowVector(double[] vector, int decimals, int valsPerLine, bool blankLine)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                if (i > 0 && i % valsPerLine == 0) // max of 12 values per row 
                    Console.WriteLine("");
                if (vector[i] >= 0.0) Console.Write(" ");
                Console.Write(vector[i].ToString("F" + decimals) + " "); // n decimals
            }
            if (blankLine) Console.WriteLine("\n");
        }

        public static void ShowVector(int[] vector, int valsPerLine, bool blankLine)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                if (i > 0 && i % valsPerLine == 0) // max of 12 values per row 
                    Console.WriteLine("");
                if (vector[i] >= 0.0) Console.Write(" ");
                Console.Write(vector[i] + " ");
            }
            if (blankLine) Console.WriteLine("\n");
        }

        public static void ShowMatrix(double[][] matrix, int numRows, int decimals, bool lineNumbering, bool showLastLine)
        {
            int ct = 0;
            if (numRows == -1) numRows = int.MaxValue; // if numRows == -1, show all rows
            for (int i = 0; i < matrix.Length && ct < numRows; ++i)
            {
                if (lineNumbering == true)
                    Console.Write(i.ToString().PadLeft(3) + ": ");
                for (int j = 0; j < matrix[0].Length; ++j)
                {
                    if (matrix[i][j] >= 0.0) Console.Write(" "); // blank space instead of '+' sign
                    Console.Write(matrix[i][j].ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
                ++ct;
            }
            if (showLastLine == true && numRows < matrix.Length)
            {
                Console.WriteLine("      ........\n ");
                int i = matrix.Length - 1;
                Console.Write(i.ToString().PadLeft(3) + ": ");
                for (int j = 0; j < matrix[0].Length; ++j)
                {
                    if (matrix[i][j] >= 0.0) Console.Write(" "); // blank space instead of '+' sign
                    Console.Write(matrix[i][j].ToString("F" + decimals) + " ");
                }
            }
            Console.WriteLine("");
        }

    } // class Helpers

    // the raw data used in the demo is a 3-item subset of Fisher's Iris data set.
    //

    //double[][] rawData = new double[30][]; // 30-item subset of Iris data set (10 each class)
    //rawData[0] = new double[] { 5.1, 3.5, 1.4, 0.2, 0, 0, 1 }; // sepal length, sepal width, petal length, petal width -> 
    //rawData[1] = new double[] { 4.9, 3.0, 1.4, 0.2, 0, 0, 1 }; // Iris setosa = 0 0 1, Iris versicolor = 0 1 0, Iris virginica = 1 0 0
    //rawData[2] = new double[] { 4.7, 3.2, 1.3, 0.2, 0, 0, 1 };
    //rawData[3] = new double[] { 4.6, 3.1, 1.5, 0.2, 0, 0, 1 };
    //rawData[4] = new double[] { 5.0, 3.6, 1.4, 0.2, 0, 0, 1 };
    //rawData[5] = new double[] { 5.4, 3.9, 1.7, 0.4, 0, 0, 1 };
    //rawData[6] = new double[] { 4.6, 3.4, 1.4, 0.3, 0, 0, 1 };
    //rawData[7] = new double[] { 5.0, 3.4, 1.5, 0.2, 0, 0, 1 };
    //rawData[8] = new double[] { 4.4, 2.9, 1.4, 0.2, 0, 0, 1 };
    //rawData[9] = new double[] { 4.9, 3.1, 1.5, 0.1, 0, 0, 1 };

    //rawData[10] = new double[] { 7.0, 3.2, 4.7, 1.4, 0, 1, 0 };
    //rawData[11] = new double[] { 6.4, 3.2, 4.5, 1.5, 0, 1, 0 };
    //rawData[12] = new double[] { 6.9, 3.1, 4.9, 1.5, 0, 1, 0 };
    //rawData[13] = new double[] { 5.5, 2.3, 4.0, 1.3, 0, 1, 0 };
    //rawData[14] = new double[] { 6.5, 2.8, 4.6, 1.5, 0, 1, 0 };
    //rawData[15] = new double[] { 5.7, 2.8, 4.5, 1.3, 0, 1, 0 };
    //rawData[16] = new double[] { 6.3, 3.3, 4.7, 1.6, 0, 1, 0 };
    //rawData[17] = new double[] { 4.9, 2.4, 3.3, 1.0, 0, 1, 0 };
    //rawData[18] = new double[] { 6.6, 2.9, 4.6, 1.3, 0, 1, 0 };
    //rawData[19] = new double[] { 5.2, 2.7, 3.9, 1.4, 0, 1, 0 };

    //rawData[20] = new double[] { 6.3, 3.3, 6.0, 2.5, 1, 0, 0 };
    //rawData[21] = new double[] { 5.8, 2.7, 5.1, 1.9, 1, 0, 0 };
    //rawData[22] = new double[] { 7.1, 3.0, 5.9, 2.1, 1, 0, 0 };
    //rawData[23] = new double[] { 6.3, 2.9, 5.6, 1.8, 1, 0, 0 };
    //rawData[24] = new double[] { 6.5, 3.0, 5.8, 2.2, 1, 0, 0 };
    //rawData[25] = new double[] { 7.6, 3.0, 6.6, 2.1, 1, 0, 0 };
    //rawData[26] = new double[] { 4.9, 2.5, 4.5, 1.7, 1, 0, 0 };
    //rawData[27] = new double[] { 7.3, 2.9, 6.3, 1.8, 1, 0, 0 };
    //rawData[28] = new double[] { 6.7, 2.5, 5.8, 1.8, 1, 0, 0 };
    //rawData[29] = new double[] { 7.2, 3.6, 6.1, 2.5, 1, 0, 0 };

    //double[] originalMeans = new double[] { 5.843, 3.040, 3.863, 1.213 }; // used to normalize original data
    //double[] originalStdDevs = new double[] { 0.948, 0.367, 1.850, 0.776 };

    //

} // ns
