using System;
using System.Collections.Generic;
using System.Text;
// Đây là lớp quần thể
namespace RBF_TIMESERIES
{
    public class Population : ICloneable
    {
        private int population_size; // Kích thước quần thể
        private Individual[] individuals; // Tập cá thể trong quần thể
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
            IndividualList = new List<Individual>();
        }

        public Individual Get(int i)
        {
            if (i >= this.IndividualList.Count)
            {
                throw new IndexOutOfRangeException("Index out of Bound " + i);
            }
            return this.IndividualList[i];
        }

        public List<Individual> IndividualList
        {
            get;
            protected set;
        }

        public int Size()
        {
            return this.IndividualList.Count;
        }

        public bool Add(Individual individual)
        {
            if (IndividualList.Count == population_size)
            {
                return false;
            }
            IndividualList.Add(individual);
            return true;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Clear()
        {
            this.IndividualList.Clear();
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

        public Population Union(Population population)
        {
            int newSize = this.Size() + population.Population_size;
            if (newSize < population_size)
            {
                newSize = population_size;
            }

            Population union = new Population(newSize);

            for (int i = 0; i < this.Size(); i++)
            {
                union.Add(this.IndividualList[i]);
            }

            for (int i = 0; i < population.Size(); i++)
            {
                union.Add(population.IndividualList[i]);
            }

            return union;
        }
        public void Sort(IComparer<Individual> comparator)
        {
            if (comparator == null)
            {
                return;
            }
            IndividualList.Sort(comparator);
        }
    }
}
