using System;
using System.Collections.Generic;
using System.Text;

namespace RBF_TIMESERIES
{
    /// <summary>
    ///  Đây là lớp cá thể
    ///  Mỗi cá thể sẽ gồm 2 thuộc tính
    ///  Em đọc qua đi xem có gì hỏi không? sau đó mới vào quá trình tiến hóa của nó. Được rồi thầy ạ, em đọc xong rồi ạ
    /// </summary>
    public class Individual
    {
        private int n_gens;  // Chiều dài của chuỗi cá thể ( độ dài của mảng weight)
        public double[] values; // Mảng lưu giá trị của weight ây
        public double fitness;
        public double Fitness
        {
            get { return fitness; }
            set { fitness = value; }
        }
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

        public Individual(int n_gens)
        {
            this.n_gens = n_gens;
            values = new double[n_gens];
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
        public Individual(Individual w)
        {
            this.n_gens = w.n_gens;
            this.values = w.Values;
        }
        public double GetLowerBound()
        {
            double min = 9999999;
            for (int i = 0; i < n_gens; i++)
            {
                if(values[i] < min)
                {
                    min = values[i];
                }
            }
            return min;
        }//2 thằng này để làm j ạ, sau này có lúc sẽ cần xét giá trị lớn nhất của cả mảng, để chặn thôi, ko thích thì bỏ đi, vâng thôi cứ để ở đấy ạ
        // Xem qua cả population đi, 2 phút nhé
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
