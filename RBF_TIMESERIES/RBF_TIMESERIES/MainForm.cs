using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Threading;

namespace RBF_TIMESERIES
{
    public partial class MainForm : Form
    {
        private double[] data = null;
        private double[][] allData = null;
        private int windowSize = 5;
        private double testingRate = 30;

        //Train
        private double[] train = null;
        private double[][] inputTrain = null;
        private double[][] idealTrain = null;
        private int numInput = 0;
        private int numIdeal = 0;
        //Test
        private double[] test = null;
        private double[][] inputTest = null;
        private double[][] idealTest = null;


        public MainForm()
        {
            //
            InitializeComponent();
        }

        // Update data in list view
        private void UpdateDataListView()
        {
            // remove all current records
            dataList.Items.Clear();
            // add new records
            for (int i = 0, n = data.GetLength(0); i < n; i++)
            {
                //add STT column
                dataList.Items.Add((i + 1).ToString());
                //add Real Data column (thêm lần lượt value vào các cột sau...thêm trên một hàng)
                //ListView1.Items[index của tên cột đầu tiên].SubItems.Add("thêm từng cột")
                dataList.Items[i].SubItems.Add(data[i].ToString());
            }
        }
        /// <summary>
        /// Get data both input & ideal with windowsize 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="windowSize"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private double[] GetWindowData(int index, int windowSize, double[] data)
        {
            double[] subData = new double[windowSize];
            for (int i = 0; i < windowSize; i++)
            {
                subData[i] = data[index + i];
            }
            return subData;
        }


        /// <summary>
        /// Get data to input[][] and ideal[][]
        /// </summary>
        /// <param name="data">data train or test</param>
        /// <param name="input"></param>
        /// <param name="ideal"></param>
        /// <param name="windowSize"></param>
        private void GetInputIdeal(double[] data, out double[][] input, out double[][] ideal, int windowSize)
        {
            int sizeData = data.Length;
            //Get input, ideal cho train
            input = new double[sizeData - windowSize][];
            ideal = new double[sizeData - windowSize][];

            for (int i = 0; i < sizeData - windowSize; i++)
            {
                input[i] = GetWindowData(i, windowSize, data);
                ideal[i] = new double[] { data[i + windowSize] };
            }
        }

        private void GetAllData(double[] data, out double[][] allData, int windowSize)
        {
            int sizeData = data.Length;
            //Get input, ideal vao 1 mang allData[][] (windowsize,1)
            allData = new double[sizeData - windowSize][];

            for (int i = 0; i < sizeData - windowSize; i++)
            {
                allData[i] = GetWindowData(i, windowSize + 1, data);
            }
        }
        private void GetTrainTest(double[][] allData, int seed, out double[][] trainData, out double[][] testData, double testingRate)
        {
            int[] allIndices = new int[allData.Length];
            for (int i = 0; i < allIndices.Length; ++i)
                allIndices[i] = i;

            Random rnd = new Random(seed);
            for (int i = 0; i < allIndices.Length; ++i) // shuffle indices
            {
                int r = rnd.Next(i, allIndices.Length);
                int tmp = allIndices[r];
                allIndices[r] = allIndices[i];
                allIndices[i] = tmp;
            }

            int numTrain = (int)((100 - testingRate) / 100 * allData.Length);
            int numTest = allData.Length - numTrain;

            trainData = new double[numTrain][];
            testData = new double[numTest][];

            int j = 0;
            for (int i = 0; i < numTrain; ++i)
                trainData[i] = allData[allIndices[j++]];
            for (int i = 0; i < numTest; ++i)
                testData[i] = allData[allIndices[j++]];

        }

        /// <summary>
        /// Get data to train[] and test[]
        /// </summary>
        /// <param name="data">all data get from File</param>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="testingRate"></param>
        private void GetTrainTest(double[] data, out double[] train, out double[] test, double testingRate)
        {
            //khoi tao
            int sizeData = data.Length;
            int numTest = (int)((testingRate / 100) * sizeData);
            int numTrain = sizeData - numTest;
            train = new double[numTrain];
            test = new double[numTest];
            int j = 0;
            int k = 0;

            //Get data train and test from data[]
            for (int i = 0; i < sizeData; i++)
            {
                if (i < numTrain)
                {
                    train[j] = data[i];
                    j++;
                }
                else
                {
                    test[k] = data[i];
                    k++;
                }
            }
        }

        // Delegates to enable async calls for setting controls properties
        private delegate void SetTextCallback(System.Windows.Forms.Control control, string text);
        private delegate void AddSubItemCallback(System.Windows.Forms.ListView control, int item, string subitemText);

        // Thread safe updating of control's text property
        private void SetText(System.Windows.Forms.Control control, string text)
        {
            if (control.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Invoke(d, new object[] { control, text });
            }
            else
            {
                control.Text = text;
            }
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            // show file selection dialog
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamReader reader = null;
                // read maximum 1000 points
                double[] tempData = new double[1000];

                try
                {
                    // open selected file
                    reader = File.OpenText(openFileDialog.FileName);
                    int i = 0;

                    // read the data

                    string[] lines = System.IO.File.ReadAllLines(openFileDialog.FileName);

                    // Display the file contents by using a foreach loop.
                    foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        int pos = line.IndexOf("\t");
                        string value = line.Substring(pos + 1, line.Length - 1 - pos);
                        //parse the value
                        tempData[i] = double.Parse(value);
                        i++;

                    }

                    // allocate and set data
                    data = new double[i];
                    Array.Copy(tempData, 0, data, 0, i);

                }
                catch (Exception)
                {
                    MessageBox.Show("Failed reading the file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    // close file
                    if (reader != null)
                        reader.Close();
                }

                // update list and chart
                UpdateDataListView();
                // enable "Start" button
                btnStart.Enabled = true;
            }
        }

        private Boolean StringIsNull(string text)
        {
            if (text.Equals(null) || text.Equals(""))
            {
                return true;
            }
            return false;
        }

        // Thread safe adding of subitem to list control
        private void AddSubItem(System.Windows.Forms.ListView control, int item, string subitemText)
        {
            if (control.InvokeRequired)
            {
                AddSubItemCallback d = new AddSubItemCallback(AddSubItem);
                Invoke(d, new object[] { control, item, subitemText });
            }
            else
            {
                control.Items[item].SubItems.Add(subitemText);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //Get input from Form
            //Get testing rate
            if (!StringIsNull(txtTestingRate.Text))
            {
                testingRate = double.Parse(txtTestingRate.Text);
            }

            //Get window size
            if (!StringIsNull(txtWindowSize.Text))
            {
                windowSize = int.Parse(txtWindowSize.Text);
            }

            //Step1: Get inputTrain, idealTrain, inputTest, idealTest
            //GetTrainTest(data, out train, out test, testingRate);
            //GetInputIdeal(train, out inputTrain, out idealTrain, windowSize);
            //GetInputIdeal(train, out inputTest, out idealTest, windowSize);
            GetAllData(data, out allData, windowSize);
            double[][] trainData = null;
            double[][] testData = null;
            int seed = 8; // gives a good demo
            GetTrainTest(allData, seed, out trainData, out testData, testingRate);

            Console.WriteLine("\nCreating a 4-5-3 radial basis function network");
            int numInput = windowSize;
            int numHidden = 2 * numInput;
            int numOutput = 1;
            RadialNetwork rn = new RadialNetwork(numInput, numHidden, numOutput);
            Console.WriteLine("\nBeginning RBF training\n");
            int maxIterations = 50; // max for GA
            int maxEvaluations = 1000;           // max for NSGAII              
            //double[] bestWeights = rn.TrainWithPSO(trainData, maxIterations);
            //double[] bestWeights = rn.TrainWithGA(trainData, maxIterations);
            Population bestWeights = rn.TrainWithNSGAII(trainData, maxEvaluations);

            Console.WriteLine("\nEvaluating result RBF classification accuracy on the test data");
            //rn.SetWeights(bestWeights);

            double acc = rn.Accuracy(testData);

            //Set Textbox
            SetText(txtTestErr, acc.ToString("F5"));

            Console.WriteLine("Complete Program!");
        }

    }
}
