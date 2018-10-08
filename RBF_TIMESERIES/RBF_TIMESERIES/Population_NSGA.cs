using System;
using System.Collections.Generic;
using System.Text;
// Đây là lớp quần thể
namespace RBF_TIMESERIES
{
    public class Population_NSGA : ICloneable
    {
        private int population_size; // Kích thước quần thể
        private Individual_NSGA[] individuals; // Tập cá thể trong quần thể
        private List<Individual_NSGA> individualList; //
        public int Population_size
        {
            get { return population_size; }
            set { population_size = value; }
        }

        public Individual_NSGA[] Individuals
        {
            get { return individuals; }
            set { individuals = value; }
        }
        public Population_NSGA(int population_size)
        {
            this.population_size = population_size;
            individuals = new Individual_NSGA[population_size];
            individualList = new List<Individual_NSGA>();
        }

        public Individual_NSGA Get(int i)
        {
            if (i >= this.individualList.Count)
            {
                throw new IndexOutOfRangeException("Index out of Bound " + i);
            }
            return this.individualList[i];
        }

        public List<Individual_NSGA> IndividualList
        {
            get;
            protected set;
        }

        public int Size()
        {
            return this.individualList.Count;
        }

        public bool Add(Individual_NSGA individual)
        {
            if (individualList.Count == population_size)
            {
                return false;
            }
            individualList.Add(individual);
            return true;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Clear()
        {
            this.individualList.Clear();
        }

        public void Population_init(int n_gens, int numberOfObjectives)
        {
            Random r = new Random();

            for (int i = 0; i < population_size; i++)
            {
                individuals[i] = new Individual_NSGA(n_gens, numberOfObjectives);
                individuals[i].Individual_init(r);
            }
        }

        public Individual_NSGA getBestOne()
        {
            double max = -9999999;
            int indeMax = 0;
            for (int i = 0; i < population_size; i++)
            {
                if (individuals[i].Objective[0] > max)
                {
                    max = individuals[i].Objective[0];
                    indeMax = i;
                }
            }
            return individuals[indeMax];
        }

        public Population_NSGA Union(Population_NSGA population)
        {
            int newSize = this.Size() + population.Population_size;
            if (newSize < population_size)
            {
                newSize = population_size;
            }

            Population_NSGA union = new Population_NSGA(newSize);

            for (int i = 0; i < this.Size(); i++)
            {
                union.Add(this.individualList[i]);
            }

            for (int i = 0; i < population.Size(); i++)
            {
                union.Add(population.individualList[i]);
            }

            return union;
        }

        public void Sort(IComparer<Individual_NSGA> comparator)
        {
            if (comparator == null)
            {
                return;
            }
            individualList.Sort(comparator);
        }
    }
}
