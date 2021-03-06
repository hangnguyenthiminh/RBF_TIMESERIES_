﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RBF_TIMESERIES
{
    /// <summary>
    ///  Đây là lớp cá thể
    ///  Mỗi cá thể sẽ gồm 2 thuộc tính
    ///  Em đọc qua đi xem có gì hỏi không? sau đó mới vào quá trình tiến hóa của nó. Được rồi thầy ạ, em đọc xong rồi ạ
    /// </summary>
    public class Individual_NSGA
    {
        private int n_gens;  // Chiều dài của chuỗi cá thể ( độ dài của mảng weight)
        public double[] values; // Mảng lưu giá trị của weight ây
        //public double fitness;
        private double[] obj;
        private int numberOfObjectives;
        public double CrowdingDistance { get; set; }
        //public double Fitness
        //{
        //    get { return fitness; }
        //    set { fitness = value; }
        //}
        public int N_gens
        {
            get { return n_gens; }
            set { n_gens = value; }
        }

        public double[] Values
        {
            get { return values; }
            set { values = value; }
        }

        
        public double[] Objective
        {
            get
            {
                return obj;
            }
            set
            {
                obj = value;
            }
        }

        /// <summary>
        /// Returns the number of objectives.
        /// </summary>
        public int NumberOfObjectives
        {
            get
            {
                if (this.Objective == null)
                {
                    return 0;
                }
                else
                {
                    return numberOfObjectives;
                };
            }
            set
            {
                numberOfObjectives = value;
            }
        }

        public int Rank { get; set; }

        /// <summary>
        /// Stores the overall constraint violation of the solution
        /// </summary>
        public double OverallConstraintViolation { get; set; }

        public Individual_NSGA(int n_gens, int numberOfObjectives)
        {
            this.n_gens = n_gens;
            values = new double[n_gens];
            this.numberOfObjectives = numberOfObjectives;
            this.Objective = new double[numberOfObjectives];
        }
        // Khởi tạo giá trị ngẫu nhiên của 1 cá thể 
        public void Individual_init(Random r)
        {
            for (int i = 0; i < n_gens; i++)
            {
                values[i] = -1.5 + 3 * r.NextDouble();  // Ở đây mình đang giới hạn giá trị của weight trong khoảng -1.5 to 1.5
                // ind_val[i] = -1 + 2 * r.NextDouble();
            }
        }

        public void Individual_init(double[] w)
        {
            values = w;
        }
        public Individual_NSGA()
        {
            this.OverallConstraintViolation = 0.0;
            this.CrowdingDistance = 0.0;
            this.numberOfObjectives = 0;
            this.Objective = null;
            this.Values = null;
        }

        public Individual_NSGA(Individual_NSGA w)
        {
            this.n_gens = w.n_gens;
            this.values = w.Values;
            this.numberOfObjectives = w.numberOfObjectives;
            this.Objective = new double[this.numberOfObjectives];
            for (int i = 0; i < this.Objective.Length; i++)
            {
                this.Objective[i] = w.Objective[i];
            }
            this.CrowdingDistance = w.CrowdingDistance;
            this.OverallConstraintViolation = w.OverallConstraintViolation;
            
        }
        public double GetLowerBound()
        {
            double min = 9999999;
            for (int i = 0; i < n_gens; i++)
            {
                if (values[i] < min)
                {
                    min = values[i];
                }
            }
            return min;
        }

        public double GetUpperBound()
        {
            double max = -9999999;
            for (int i = 0; i < n_gens; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                }
            }
            return max;
        }

    }
}
