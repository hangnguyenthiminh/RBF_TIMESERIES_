using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBF_TIMESERIES
{
    class GAs
    {
        private int m_Population_size;
        private int m_IndividualLength;
        private Random m_Random;
        private Population m_Population;
        private RadialNetwork m_RadialNetwork;
        private double m_MaxIterations;

        public Population Population
        {
            get
            {
                return m_Population;
            }

            set
            {
                m_Population = value;
            }
        }

        public GAs(int m_Population_size, int m_IndividualLength, RadialNetwork RadialNetwork, double m_MaxIterations, double[][] inputData, int numberOfObjectives)
        {
            this.m_Population_size = m_Population_size;
            this.m_IndividualLength = m_IndividualLength;
            this.m_Random = new Random();
            this.Population = new Population(m_Population_size);
            this.m_RadialNetwork = RadialNetwork;
            this.m_MaxIterations = m_MaxIterations;

            this.Population.Population_init(m_IndividualLength, numberOfObjectives);
            // set fitness
            for (int i = 0; i < m_Population_size; i++)
            {
                CalculateFitnessOf(Population.Individuals[i], inputData);
            }
        }

        // Đây là hàm đột biến, Kiểu đột biến đa thức ( sau này đọc sẽ hiểu)
        private void DoMutation(Individual individual)
        {
            //RadialNetwork rn = new RadialNetwork(numInput, numHidden, numOutput);
            double eta_m = 20.0;
            double distributionIndex = eta_m;
            double probability = 1 / individual.N_gens;
            double rnd, delta1, delta2, mut_pow, deltaq;
            double y, yl, yu, val, xy;

            for (int var = 0; var < individual.N_gens; var++)
            {
                if (m_Random.NextDouble() <= probability)
                {
                    y = individual.Values[var];
                    yl = individual.GetLowerBound();
                    yu = individual.GetUpperBound();
                    delta1 = (y - yl) / (yu - yl);
                    delta2 = (yu - y) / (yu - yl);
                    rnd = m_Random.NextDouble();
                    mut_pow = 1.0 / (eta_m + 1.0);
                    if (rnd <= 0.5)
                    {
                        xy = 1.0 - delta1;
                        val = 2.0 * rnd + (1.0 - 2.0 * rnd) * (Math.Pow(xy, (distributionIndex + 1.0)));
                        deltaq = Math.Pow(val, mut_pow) - 1.0;
                    }
                    else
                    {
                        xy = 1.0 - delta2;
                        val = 2.0 * (1.0 - rnd) + 2.0 * (rnd - 0.5) * (Math.Pow(xy, (distributionIndex + 1.0)));
                        deltaq = 1.0 - (Math.Pow(val, mut_pow));
                    }
                    y = y + deltaq * (yu - yl);
                    if (y < yl)
                    {
                        y = yl;
                    }
                    if (y > yu)
                    {
                        y = yu;
                    }
                    individual.Values[var] = y;
                }
            }
        }
        // Đây là hàm lai ghép, 2 điểm cắt
        private Individual[] DoCrossover(Individual parent1, Individual parent2) // TwoCrossCutingPoint
        {
            double probability = 0.9;
            Individual[] offSpring = new Individual[2];
            
            offSpring[0] = new Individual(parent1);
            offSpring[1] = new Individual(parent2);
            int crosspoint1;
            int crosspoint2;
            if (m_Random.NextDouble() <= probability)
            {
                for (int i = 0; i < m_Population_size; i++)
                {
                    // STEP 1: Get two cutting points
                    crosspoint1 = m_Random.Next(0, m_IndividualLength - 1);
                    crosspoint2 = m_Random.Next(0, m_IndividualLength - 1);
                    while (crosspoint2 == crosspoint1)
                    {
                        crosspoint2 = m_Random.Next(0, m_IndividualLength - 1);
                    }

                    if (crosspoint1 > crosspoint2)
                    {
                        int swap;
                        swap = crosspoint1;
                        crosspoint1 = crosspoint2;
                        crosspoint2 = swap;
                    }
                    // Bắt đầu cắt
                    for (int j = 0; j < m_IndividualLength; j++)
                    {
                        if (crosspoint1 <= j && j < crosspoint2)
                        {
                            offSpring[0].Values[j] = parent2.Values[j];
                            offSpring[1].Values[j] = parent1.Values[j];
                        }
                        else
                        {
                            offSpring[0].Values[j] = parent1.Values[j];
                            offSpring[1].Values[j] = parent2.Values[j];
                        }
                    }
                }
            }

            return offSpring;
        }
        // Đây là quá trình tiến hóa của GA nhé
        public void Reproduction(double[][] inputData)
        {
            for (int i = 0; i < m_MaxIterations; i++)
            {
                Population pop_temp = new Population(m_Population_size); // tao mot population tam
                pop_temp = (Population)Population.Clone();
                // Duyệt qua tất cả các cá thể trong quần thể
                for (int index = 0; index < m_Population_size; index++)
                {
                    Individual currIndividual = pop_temp.Individuals[index];
                    int sol1 = (int)(m_Random.NextDouble() * (m_Population_size - 1));
                    int sol2 = (int)(m_Random.NextDouble() * (m_Population_size - 1));

                    Individual solution1 = pop_temp.Individuals[sol1];
                    Individual solution2 = pop_temp.Individuals[sol2];

                    while (sol1 == sol2)
                    {
                        sol2 = (int)(m_Random.NextDouble() * (m_Population_size - 1));
                        solution2 = pop_temp.Individuals[sol2];
                    }
                    Individual[] Childs = DoCrossover(solution1, solution2);//DoCrossover(solution1, solution2);// 
                    DoMutation(Childs[0]);
                    DoMutation(Childs[1]);
                    // tính toán lại độ thích nghi và từ đó tính ra MSE của cá thể đang xét
                    double MSE_temp1 = CalculateFitnessOf(Childs[0], inputData);
                    double MSE_temp2 = CalculateFitnessOf(Childs[1], inputData);
                    // Chọn lọc
                    // chỉ với những cái cho ra MSE tốt thì mới được cho vào thế hệ mới, còn không vẫn giữ những cái cũ
                    if (MSE_temp1 > MSE_temp2)
                    {
                        if (MSE_temp1 > currIndividual.Objective[0])
                        {
                            Childs[0].Values.CopyTo(Population.Individuals[index].Values, 0);
                        }
                    }
                    else
                    {
                        if (MSE_temp2 > currIndividual.Objective[0])
                        {
                            Childs[1].Values.CopyTo(Population.Individuals[index].Values, 0);
                        }
                    }
                }
            }
        }

        private double CalculateFitnessOf(Individual individual, double[][] inputData)
        {
            // Chỗ này chính là chỗ cần dùng với RBF đây
            RadialNetwork rn = new RadialNetwork(m_RadialNetwork);
            double fitness = 0.0;
            rn.SetWeights(individual.values);
            fitness = rn.Accuracy(inputData);
            individual.Objective[0] = fitness;
            return fitness;
        }
    }
}
