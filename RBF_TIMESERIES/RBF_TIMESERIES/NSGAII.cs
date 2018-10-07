using RBF_TIMESERIES.Utils;
using System;

namespace RBF_TIMESERIES
{
    /// <summary>
    /// Implementation of NSGA-II. This implementation of NSGA-II makes use of a
    /// QualityIndicator object to obtained the convergence speed of the algorithm.
    /// This version is used in the paper: A.J. Nebro, J.J. Durillo, C.A. Coello
    /// Coello, F. Luna, E. Alba "A Study of Convergence Speed in Multi-Objective
    /// Metaheuristics." To be presented in: PPSN'08. Dortmund. September 2008.
    /// </summary>
    public class NSGAII
    {
        private int m_Population_size;
        private int m_IndividualLength;
        private Random m_Random;
        private Population m_Population;
        private RadialNetwork m_RadialNetwork;
        private double m_MaxEvaluations;
        //
        private double crossoverProbability = 0.9;
        private static readonly double EPS = 1.0e-14;
        private static readonly double ETA_C_DEFAULT = 20.0;
        private double distributionIndex = ETA_C_DEFAULT;
        private static readonly double ETA_M_DEFAULT = 20.0;
        private static readonly double eta_m = ETA_M_DEFAULT;
        private double mutationProb;

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

        public double MutationProb
        {
            get { return mutationProb; }
            set { mutationProb = value; }
        }

        public NSGAII(int m_Population_size, int m_IndividualLength, RadialNetwork RadialNetwork, double m_MaxEvaluations, double[][] inputData)
        {
            this.m_Population_size = m_Population_size;
            this.m_IndividualLength = m_IndividualLength;
            this.m_Random = new Random();
            this.Population = new Population(m_Population_size);
            this.m_RadialNetwork = RadialNetwork;
            this.m_MaxEvaluations = m_MaxEvaluations;
            this.Population.Population_init(m_IndividualLength);
            mutationProb = 1.0 / m_IndividualLength;

            // set fitness
            for (int i = 0; i < m_Population_size; i++)
            {
                CalculateFitnessOf(Population.Individuals[i], inputData);
            }
        }

        private double CalculateFitnessOf(Individual individual, double[][] inputData)
        {
            // Chỗ này chính là chỗ cần dùng với RBF đây
            RadialNetwork rn = new RadialNetwork(m_RadialNetwork);
            double fitness = 0.0;
            rn.SetWeights(individual.values);
            fitness = rn.Accuracy(inputData);
            individual.Fitness = fitness;
            return fitness;
        }



        /// <summary>
        /// Runs the NSGA-II algorithm.
        /// </summary>
        /// <returns>a <code>SolutionSet</code> that is a set of non dominated solutions as a result of the algorithm execution</returns>
        public Population Execute(double[][] inputData)
        {
            int evaluations;

            Population population;
            Population offspringPopulation;
            Population union;

            //Initialize the variables
            evaluations = 0;

            //  requiredEvaluations = 0;

            // Bước 1: Khởi tạo quần thể
            // Create the initial solutionSet
            population = new Population(m_Population_size);

            // Vòng lặp tiến hóa 
            while (evaluations < m_MaxEvaluations)
            {
                // Create the offSpring solutionSet      
                offspringPopulation = new Population(m_Population_size); // Tập cá thể con
                Individual[] parents = new Individual[2];

                for (int i = 0; i < (m_Population_size / 2); i++) // N/2
                {
                    if (evaluations < m_MaxEvaluations)
                    {
                        //Lấy 2 Cha parents
                        int sol1 = (int)(m_Random.NextDouble() * (m_Population_size - 1));
                        int sol2 = (int)(m_Random.NextDouble() * (m_Population_size - 1));
                        parents[0] = population.Individuals[sol1];
                        parents[1] = population.Individuals[sol2];
                        //
                        while (sol1 == sol2)
                        {
                            sol2 = (int)(m_Random.NextDouble() * (m_Population_size - 1));
                            parents[1] = population.Individuals[sol2];
                        }

                        // Sử dụng SBX crossover tạo ra 2 thằng con
                        Individual[] offSpring = SBXCrossover(parents);

                        DoMutation(offSpring[0]);
                        DoMutation(offSpring[1]);

                        offSpring[0].Objective[0] = CalculateFitnessOf(offSpring[0], inputData);
                        offSpring[1].Objective[0] = CalculateFitnessOf(offSpring[1], inputData);

                        // Đưa 2 con vào danh sách 
                        offspringPopulation.Add(offSpring[0]);
                        offspringPopulation.Add(offSpring[1]);
                        evaluations += 2;
                    }
                }
                //union = ((SolutionSet)population).Union(offspringPopulation);
                // Đến đây mình có n cá thể cha ((population),), N có thể con (offspringPopulation)
                // Create the solutionSet union of solutionSet and offSpring
                union = ((Population)population).Union(offspringPopulation);

                // Ranking the union - Sxep ko troi - non-dominated sorting
                Ranking ranking = new Ranking(union);

                int remain = m_Population_size;
                int index = 0;
                Population front = null;
                population.Clear();

                // Obtain the next front
                front = ranking.GetSubfront(index);
                // Đưa các Front vào quần thể mới, đến thằng cuối cùng 
                while ((remain > 0) && (remain >= front.Size()))
                {
                    //Assign crowding distance to individuals 
                    // Gắn khoang cach CrowdingDistance cho ca the
                    CrowdingDistanceAssignment(front);

                    //Add the individuals of this front
                    for (int k = 0; k < front.Size(); k++)
                    {
                        population.Add(front.Get(k));
                    }

                    //Decrement remain
                    remain = remain - front.Size();

                    //Obtain the next front
                    index++;
                    if (remain > 0)
                    {
                        front = ranking.GetSubfront(index);
                    }
                }

                // Remain is less than front(index).size, insert only the best one
                if (remain > 0)
                {  // front contains individuals to insert                        
                    CrowdingDistanceAssignment(front);
                    front.Sort(new CrowdingComparator());
                    for (int k = 0; k < remain; k++)
                    {
                        population.Add(front.Get(k));
                    }

                    remain = 0;
                }

                // Đoạn mã này cho thấy cách sử dụng đối tượng chỉ thị vào mã
                // của NSGA-II. Đặc biệt, nó tìm thấy số lượng các đánh giá cần thiết
                // bởi thuật toán để có được front Pareto với một hypervolume cao hơn
                // so với hypervolume của front Pareto thực sự.

                /*     if ((indicators != null) && (requiredEvaluations == 0))
                     {
                         double HV = indicators.GetHypervolume(population);
                         if (HV >= (0.98 * indicators.TrueParetoFrontHypervolume))
                         {
                             requiredEvaluations = evaluations;
                         }
                     }
                 }

                 // Return as output parameter the required evaluations
                 SetOutputParameter("evaluations", requiredEvaluations);

                 // Return the first non-dominated front
                 Ranking rank = new Ranking(population);

                 Result = rank.GetSubfront(0);

                 return Result;*/
                return new Population(100);
            }
        }

        private Individual[] SBXCrossover(Individual[] parents)
        {
            if (parents.Length != 2)
            {
                Console.WriteLine("Exception in " + this.GetType().FullName + ".Execute()");
                throw new Exception("Exception in " + this.GetType().FullName + ".Execute()");
            }

            Individual[] offSpring;
            offSpring = DoCrossover(crossoverProbability, parents[0], parents[1]);

            return offSpring;
        }

        private Individual[] DoCrossover(double crossoverProbability, Individual parent1, Individual parent2)
        {
            Individual[] offSpring = new Individual[2];

            offSpring[0] = new Individual(parent1);
            offSpring[1] = new Individual(parent2);

            int i;
            double rand;
            double y1, y2, yL, yu;
            double c1, c2;
            double alpha, beta, betaq;
            double valueX1, valueX2;
            Individual x1 = new Individual(parent1);
            Individual x2 = new Individual(parent2);
            Individual offs1 = new Individual(offSpring[0]);
            Individual offs2 = new Individual(offSpring[1]);

            int numberOfVariables = x1.N_gens;

            Random r = new Random();
            if (r.NextDouble() <= crossoverProbability)
            {
                for (i = 0; i < numberOfVariables; i++)
                {
                    valueX1 = x1.Values[i];
                    valueX2 = x2.Values[i];
                    if (r.NextDouble() <= 0.5)
                    {
                        if (Math.Abs(valueX1 - valueX2) > EPS)
                        {

                            if (valueX1 < valueX2)
                            {
                                y1 = valueX1;
                                y2 = valueX2;
                            }
                            else
                            {
                                y1 = valueX2;
                                y2 = valueX1;
                            }

                            yL = x1.GetLowerBound(); //lowerBound
                            yu = x1.GetUpperBound(); //upperBound

                            rand = r.NextDouble();
                            beta = 1.0 + (2.0 * (y1 - yL) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c1 = 0.5 * ((y1 + y2) - betaq * (y2 - y1));
                            beta = 1.0 + (2.0 * (yu - y2) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c2 = 0.5 * ((y1 + y2) + betaq * (y2 - y1));

                            if (c1 < yL)
                            {
                                c1 = yL;
                            }

                            if (c2 < yL)
                            {
                                c2 = yL;
                            }

                            if (c1 > yu)
                            {
                                c1 = yu;
                            }

                            if (c2 > yu)
                            {
                                c2 = yu;
                            }

                            if (r.NextDouble() <= 0.5)
                            {
                                offs1.Values[i] = c2;
                                offs2.Values[i] = c1;
                                //offs1.SetValue(i, c2);
                                //offs2.SetValue(i, c1);
                            }
                            else
                            {
                                offs1.Values[i] = c1;
                                offs2.Values[i] = c2;
                                //offs1.SetValue(i, c1);
                                //offs2.SetValue(i, c2);
                            }
                        }
                        else
                        {
                            offs1.Values[i] = valueX1;
                            offs2.Values[i] = valueX2;
                            //offs1.SetValue(i, valueX1);
                            //offs2.SetValue(i, valueX2);
                        }
                    }
                    else
                    {
                        offs1.Values[i] = valueX2;
                        offs2.Values[i] = valueX1;
                        //offs1.SetValue(i, valueX2);
                        //offs2.SetValue(i, valueX1);
                    }
                }
            }

            return offSpring;
        }

        private void DoMutation(Individual individual)
        {
            double rnd, delta1, delta2, mut_pow, deltaq;
            double y, yl, yu, val, xy;
            Random r = new Random();

            Individual x = new Individual(individual);
            for (int var = 0; var < individual.N_gens; var++)
            {
                if (r.NextDouble() <= mutationProb)
                {
                    y = x.Values[var];
                    yl = x.GetLowerBound();
                    yu = x.GetUpperBound();
                    delta1 = (y - yl) / (yu - yl);
                    delta2 = (yu - y) / (yu - yl);
                    rnd = r.NextDouble();
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
                    x.Values[var] = y;
                    //x.SetValue(var, y);
                }
            }
        }

        public void CrowdingDistanceAssignment(Population population)
        {
            int size = population.Population_size;
            if (size == 0)
                return;

            if (size == 1)
            {
                population.Get(0).CrowdingDistance = Double.MaxValue;
                return;
            }
            if (size == 2)
            {
                population.Get(0).CrowdingDistance = Double.MaxValue;
                population.Get(1).CrowdingDistance = Double.MaxValue;
                return;
            }

            Population front = new Population(size);
            for (int i = 0; i < size; i++)
            {
                front.Add(population.Get(i));
            }

            for (int i = 0; i < size; i++)
                front.Get(i).CrowdingDistance = 0.0;

            double objetiveMaxn;
            double objetiveMinn;
            double distance;

            //2 hàm mục tiêu
           for (int i = 0; i < 2; i++)
			{
				// Sort the population by Obj n            
				front.Sort(new ObjectiveComparator(i));
				objetiveMinn = front.Get(0).Objective[i];
				objetiveMaxn = front.Get(front.Size() - 1).Objective[i];

				//Set de crowding distance            
				front.Get(0).CrowdingDistance = double.PositiveInfinity;
				front.Get(size - 1).CrowdingDistance = double.PositiveInfinity;

				for (int j = 1; j < size - 1; j++)
				{
					distance = front.Get(j + 1).Objective[i] - front.Get(j - 1).Objective[i];
					distance = distance / (objetiveMaxn - objetiveMinn);
					distance += front.Get(j).CrowdingDistance;
					front.Get(j).CrowdingDistance = distance;
				}
			}
        }
    }
}
