using System;
using System.Collections.Generic;
using System.Text;
// Đây là lớp quần thể
namespace RBF_TIMESERIES
{
    public class Population_GA:ICloneable
    {
        private int population_size; // Kích thước quần thể
        private Individual_GA[] individuals; // Tập cá thể trong quần thể
        public int Population_size
        {
            get { return population_size; }
            set { population_size = value; }
        }

        public Individual_GA[] Individuals
        {
            get { return individuals; }
            set { individuals = value; }
        }
        public Population_GA(int population_size)
        {
            this.population_size = population_size;
            individuals = new Individual_GA[population_size];
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
                individuals[i] = new Individual_GA(n_gens);
                individuals[i].Individual_init(r);

            }
        }

        public Individual_GA getBestOne()
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
