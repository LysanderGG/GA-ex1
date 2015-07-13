using System;
using System.Collections.Generic;

// http://www.boente.eti.br/fuzzy/ebook-fuzzy-mitchell.pdf
//
// Implement a simple GA with fitness−proportionate selection, roulettewheel sampling, population size
// 100, single−point crossover rate pc = 0.7, and bitwise mutation rate pm = 0.001. Try it on the
// following fitness function: ƒ(x) = number of ones in x, where x is a chromosome of length 20.
// Perform 20 runs, and measure the average generation at which the string of all ones is discovered.

namespace GA_ex1
{
    delegate float FitnessFunc(Chromosome c);

    class Chromosome
    {
        private const int c_length = 20;
        private static readonly Random c_rnd = new Random();

        public int[] Genes { get; private set; }
        public float Fitness { get; private set; }

        public Chromosome()
        {
            Genes = new int[c_length];
            for( int i = 0; i < c_length; ++i )
            {
                Genes[i] = c_rnd.Next(2);
            }
        }

        public Chromosome(Chromosome other)
        {
            Genes = new int[c_length];
            Array.Copy(other.Genes, Genes, c_length);
        }

        public void Crossover(Chromosome other)
        {
            int crossoverPosition = c_rnd.Next(c_length);
            //Console.WriteLine("Crossover at " + crossoverPosition);
            for( int i = crossoverPosition; i < c_length; ++i )
            {
                int tmp = Genes[i];
                Genes[i] = other.Genes[i];
                other.Genes[i] = tmp;
            }
        }

        public void Mutate(float mutationRate)
        {
            for( int i = 0; i < c_length; ++i )
            {
                if( (float)c_rnd.NextDouble() < mutationRate )
                {
                    Genes[i] = (Genes[i] + 1) % 2;
                }
            }
        }

        public float UpdateFitness(FitnessFunc f)
        {
            Fitness = f(this);
            return Fitness;
        }

        public override string ToString()
        {
            string s = String.Empty;


            foreach( int iVal in Genes )
            {
                s += iVal + " ";
            }

            s += "\n";

            return s;
        }
    }

    class Population
    {
        private static readonly Random c_rnd = new Random();

        private float m_crossoverRate;
        private float m_mutationRate;

        public int Generation { get; private set; }
        public Chromosome[] Chromosomes { get; private set; }
        public float CrossoverRate 
        {
            get { return m_crossoverRate;  }
            private set
            {
                if( value < 0.0f || value > 1.0f ) { throw new ArgumentOutOfRangeException(); }
                m_crossoverRate = value;
            }
        }
        public float MutationRate
        {
            get { return m_mutationRate; }
            private set
            {
                if( value < 0.0f || value > 1.0f ) { throw new ArgumentOutOfRangeException(); }
                m_mutationRate = value;
            }
        }
        private float Fitness { get; set; } // The population total fitness


        public Population(int nbChromosomes, float crossoverRate, float mutationRate)
        {
            CrossoverRate = crossoverRate;
            MutationRate  = mutationRate;
            Fitness       = 0f;

            Chromosomes = new Chromosome[nbChromosomes];
            for( int i = 0; i < nbChromosomes; ++i )
            {
                Chromosomes[i] = new Chromosome();
            }
        }

        // The fitness of the population and of the chromosomes must have been updated before calling this function.
        private Chromosome Selection()
        {
            float selectRnd = (float)(c_rnd.NextDouble() * Fitness);
            float summedFitness = Fitness;
            for( int i = Chromosomes.Length - 1; i >= 0; --i )
            {
                Chromosome c = Chromosomes[i];
                summedFitness -= c.Fitness;
                if( summedFitness <= selectRnd )
                {
                    return c;
                }

            }

            throw new Exception("Should not pass here.");
        }

        public void Evolve(FitnessFunc f)
        {
            Chromosome[] newGen = new Chromosome[Chromosomes.Length];

            Fitness = 0f;
            foreach( Chromosome c in Chromosomes )
            {
                Fitness += c.UpdateFitness(f);
            }

            for( int i = 0; i < newGen.Length; )
            {
                Chromosome c0 = new Chromosome(Selection());
                Chromosome c1 = new Chromosome(Selection());
                //Console.WriteLine("New Gen - " + i);
                //Console.WriteLine(String.Format("Select chromosome {0} and {1}.", Array.IndexOf(Chromosomes, c0), Array.IndexOf(Chromosomes, c1)));

                if( (float)c_rnd.NextDouble() < CrossoverRate )
                {
                    c0.Crossover(c1);
                }

                c0.Mutate(MutationRate);
                c1.Mutate(MutationRate);

                newGen[i++] = c0;
                // If the population is odd, discard one member.
                // It could be done at random into the whole population instead of the last one.
                if( i < newGen.Length )
                {
                    newGen[i++] = c1;
                }
            }

            Chromosomes = newGen;
            ++Generation;

            Fitness = 0f;
            foreach( Chromosome c in Chromosomes )
            {
                Fitness += c.UpdateFitness(f);
            }
        }

        public override string ToString()
        {
            string s = String.Empty;
            s += "Generation: " + Generation + "\n";
            s += "Mean Fitness: " + Fitness / Chromosomes.Length + "\n";
            s += "Population Members: \n";
            
            for( int i = 0; i < Chromosomes.Length; ++i )
            {
                Chromosome c =  Chromosomes[i];
                s += String.Format("[{0}] {1}", i.ToString("D2"), c.ToString());
            }

            return s;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Population p = new Population(70, 0.7f, 0.001f);
                Console.WriteLine(p.ToString());

                bool end = false;
                do
                {
                    p.Evolve(c => { float fitness = 0f; foreach( int iVal in c.Genes ) { fitness += iVal; } return fitness; });
                    Console.WriteLine(p.ToString());

                    foreach( Chromosome c in p.Chromosomes )
                    {
                        // We stop if an individual's genome is is 111...111
                        if( c.Genes.Length - c.Fitness < 0.0001f )
                        {
                            end = true;
                        }
                    }
                } while( !end );

                Console.WriteLine("--------------------");
                Console.WriteLine("End!");
            }
            catch( Exception e )
            {
                Console.WriteLine(e);
            }

            Console.ReadKey();
        }
    }
}
