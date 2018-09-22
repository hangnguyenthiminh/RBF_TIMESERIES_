using System;
using System.Collections.Generic;
using System.Text;
// Đây là lớp quần thể
namespace RBF_TIMESERIES
{
    public class Population:ICloneable
    {
        private int population_size; // Kích thước quần thể
        private Individual[] individuals; // Tập cá thể trong quần thể
        // Các hàm này hiểu hết rồi chứ nàng?
        public int Population_size
        {
            get { return population_size; }
            set { population_size = value; }
        }

        public Individual[] Individuals
        {
            get { return individuals; }
            set { individuals = value; }
        }
        public Population(int population_size)
        {
            this.population_size = population_size;
            individuals = new Individual[population_size];
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public void Population_init(int n_gens)
        {
            Random r = new Random();
            
            for (int i = 0; i < population_size; i++)
            {
                individuals[i] = new Individual(n_gens);
                individuals[i].Individual_init(r);

            }
        }

        public Individual getBestOne()
        {
            double max = -9999999;
            int indeMax = 0;
            for (int i = 0; i < population_size; i++)
            {
                if (individuals[i].Fitness > max)
                {
                    max = individuals[i].Fitness;
                    indeMax = i;
                }
            }
            return individuals[indeMax];
        }
    }
}
