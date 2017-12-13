using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Project
{
    public class Bahn
    {
        //Festlegen von Absetzkosten 
        private const int ABSETZKOSTEN = 20;
        private const int MARKIERKOSTEN = 100;               
        /*
         * Generiere eine Voxelliste für Testzwecke. Dabei wird der Schichtrand-Tag zufällig gesetzt.
         */
        public List<Voxel> GenerateTestData()
        {
            List<Voxel> testVoxel = new List<Voxel>();
            Random randomizer = new Random();
            bool randRandomizer = Convert.ToBoolean((randomizer.Next(-1, 1)));
            
           for (ushort x = 0; x < 5; x++)
            {
                for (ushort y = 0; y < 5; y++)
                {
                    for (ushort z = 0; z < 5; z++)
                    {
                        randRandomizer = Convert.ToBoolean((randomizer.Next(-1, 1)));
                        testVoxel.Add(new Voxel(randRandomizer, false, x,y,z));
                    }
                }
            }
            return testVoxel;
        }     
        
        public List<List<Voxel>> SplitVoxelList(List<Voxel> voxelList)
        {
            List<Voxel> voxelListEins = new List<Voxel>();           
            List<Voxel> voxelListZwei = new List<Voxel>(); 
            
            foreach (var v in voxelList)
            {
                if (v.getSchichtrand() == true)
                    voxelListEins.Add(v);
                else
                    voxelListZwei.Add(v);
            }
            List<List<Voxel>> splitList = new List<List<Voxel>>();
            splitList.Add(voxelListEins);
            splitList.Add(voxelListZwei);
            return splitList;
        }


        public double EudklidDistanzAusVoxelDistanz(int[] distanz)
        {
            return (Math.Sqrt(Math.Pow(distanz[0], 2) + 
                              Math.Pow(distanz[1], 2) + 
                              Math.Pow(distanz[2], 2)));
        }
        /*
         * Umwandlung einer Voxelschicht zu einem Graphen. Anhand von Nachbarschaften werden kosten für die Kanten festgelegt, und absetzpunkte werden definiert.
         * Ein Absetzpunkt ensteht immer wenn ein Voxel nicht direkt zu einem der 26 umliegenden (oder sich selbst) benachbart ist.
         */
        private Graph_ VoxelToGraph(List<Voxel> voxel)
        {       
            Graph_ graph = new Graph_();
            
            foreach (var v in voxel)
            {
                List<double> graphElemente = new List<double>();
                foreach (var w in voxel)
                {
                    int[] distanz = new int[3] {0, 0, 0};
                    distanz = v.VoxelKoordinatenDistanz(w);                   
                    /*
                     * Berechne für alle Benachbarten und nicht benachbarten Knoten jeden Knotens, die Distanz
                     * zu den anderen Knoten. Füge die VoxelKoordinaten hinzu. Nachbarschaftsfunktion je nach Druckwunsch auswählen
                    */
                    if (v.IsNeighbor6(w))                  
                        graphElemente.Add(EudklidDistanzAusVoxelDistanz(distanz));
                    
                    else                 
                        graphElemente.Add(ABSETZKOSTEN + EudklidDistanzAusVoxelDistanz(distanz));                                     
                }
                graph.AddGraphElement(graphElemente);             
                graph.AddVoxelKoordinaten(v.getKoords());
            }
            MarkiereEigenknoten(graph); 
            return graph;
        }

        /*
         * Markiert die Kante eines Knotens zu sich selbst, unter Berücksichtigung
         * der Matrixeigenschaft der Liste von Listen.
         */
        private Graph_ MarkiereEigenknoten(Graph_ graph)
        {
            for(int i = 0; i < graph.GetGraph().Count; i++)
            {
                graph.SetGraph(graph.GetGraphElement(i,i)+MARKIERKOSTEN, i, i);
            }
            return graph;
        }
        
        //Markiert einen Knoten, für alle anderen Knoten in den Kostenlisten
        private static Graph_ MarkiereKnoten(int knoten, Graph_ graph)
        {
            for(int i =0; i < graph.GetGraph().Count; i++)
            {
                graph.SetGraph(graph.GetGraphElement(i,knoten)+MARKIERKOSTEN, i, knoten);
            }
            graph.SetGraph(graph.GetGraphElement(knoten,knoten)-MARKIERKOSTEN, knoten, knoten);
            return graph;
        }
        
        // Generieren einer ersten Bahnplanungslösung mit dem Nearest-Neighbor Verfahren
        private Druckfolge NearestNeighbor(Graph_ graph, int startNode)
        {                
            Druckfolge initialLösung = new Druckfolge();       
            // 1. Startpunkt = Erster Knoten
            int aktuellerKnoten = startNode;
            int minimumKnotenNummer = startNode;      
            MarkiereKnoten(aktuellerKnoten, graph);
            initialLösung.AddPriority((uint)startNode);
                 
            for (int i = 0; i < graph.GetGraph().Count - 1; i++)
            {                
                double minimum = MARKIERKOSTEN*10;
                for (int j = 0; j < graph.GetGraph().Count; j++)
                {
                    if (graph.GetGraphElement(aktuellerKnoten, j) < minimum)
                    {
                        minimum = graph.GetGraphElement(aktuellerKnoten, j);
                        minimumKnotenNummer = j;
                    }
                }
                // Füge den Knoten mit günstigster Kante in die Druckreihenfolge ein               
                initialLösung.AddPriority((uint) minimumKnotenNummer);
                initialLösung.SummiereGesamtkosten((uint) minimum);
                // Aktualisiere den Knoten von dem aus die günstigste Kante gesucht wird
                aktuellerKnoten = minimumKnotenNummer;                
                MarkiereKnoten(aktuellerKnoten, graph);
            }        
            return initialLösung;
        }

        public double CalculateDistanceAll(Druckfolge druckfolge, List<ushort[]> voxelList)
        {
            double distanzKosten = 0;
            for (int i = 0; i < voxelList.Count; i++)
            {
                uint index = druckfolge.GetPriorityItem(i);
                Voxel v = new Voxel(voxelList[(int) index]);
              
                if((i + 1) < voxelList.Count)
                {                      
                    uint index2 = druckfolge.GetPriorityItem((i + 1));                   
                    Voxel v2 = new Voxel(voxelList[(int)index2]);
                    if(v.IsNeighbor6(v2))
                        distanzKosten += EudklidDistanzAusVoxelDistanz(v.VoxelKoordinatenDistanz(v2));
                    else
                        distanzKosten += ABSETZKOSTEN+EudklidDistanzAusVoxelDistanz(v.VoxelKoordinatenDistanz(v2));
                }       
                        
            }
            return distanzKosten;
        }

        public void _2OptSwap(Druckfolge neueLösung,List<ushort[]> voxelList, int i, int j)
        {
            Druckfolge swap = new Druckfolge(neueLösung.DeepCopy());
            for (int m = 0; m < i; m++)
            {
                neueLösung.SetPriority((int)swap.GetPriorityItem(m),m);
            }
            int decrease = 0;
            
            for (int m = i; m <= j; m++)
            {
                    neueLösung.SetPriority((int)swap.GetPriorityItem(j-decrease),m);
                    decrease++;
            }            
            for (int m = j + 1; m < neueLösung.GetPriority().Count; m++)
            {
                neueLösung.SetPriority((int)swap.GetPriorityItem(m),m);
            }           
            neueLösung.SetGesamtkosten(CalculateDistanceAll(neueLösung, voxelList));
        }
        
        public Druckfolge _2Opt(Druckfolge initialLösung, Graph_ graph)
        {
            Druckfolge _2optLösung = new Druckfolge(initialLösung);
            Druckfolge neueLösung = _2optLösung.DeepCopy();
            for(int improve = 0; improve < 5; improve++)
            {
                for (int i = 0; i < graph.GetVoxelKoordinaten().Count; i++)
                {
                    for (int j = i + 1; j < graph.GetVoxelKoordinaten().Count; j++)
                    {
                        neueLösung = _2optLösung.DeepCopy();
                        _2OptSwap(neueLösung,graph.GetVoxelKoordinaten(), i, j);
                        if (!(neueLösung.GetGesamtkosten() > _2optLösung.GetGesamtkosten()))
                            _2optLösung = neueLösung.DeepCopy();
                    }
                }
            }
            return _2optLösung;
        }
        
        public void Bahnplanung(List<Voxel> voxelList)
        {
            /*
             * Teilen der gesamten Voxelliste in Randvoxel und Rest, damit unterschiedliche Bahnen geplant werden können
             * splitList[0] enthält Schichtränder
             * splitList[1] enthält alle anderen Voxel
             */            
            List<List<Voxel>> splitList = new List<List<Voxel>>(SplitVoxelList(voxelList));
            
            // Erstelle zwei Graphen : Randvoxel-Graph und Restvoxel-Graph
            Graph_ randGraph = new Graph_(VoxelToGraph(splitList[0]));
            Graph_ restGraph = new Graph_(VoxelToGraph(splitList[1]));

            // Erstellen der Druckfolgen
            Druckfolge initialRand = new Druckfolge();
            Druckfolge initialRest = new Druckfolge();

            Druckfolge _2optRand = new Druckfolge();
            Druckfolge _2optRest = new Druckfolge();
            
            Druckfolge optimizedRand = new Druckfolge();
            Druckfolge optimizedRest = new Druckfolge();
            
            for (int NNRUNS = 0; NNRUNS < 10; NNRUNS++)
            {
                Random randomizer = new Random();                
                int startNode = (randomizer.Next(0, restGraph.GetGraph().Count));
                // Generieren einer NN-Tour mit random Startknoten
                initialRand =NearestNeighbor(randGraph.DeepCopy(), startNode);
                initialRest = NearestNeighbor(restGraph.DeepCopy(), startNode);
      
                // Verbesserung der initialen Lösung durch 2-opt
                _2optRand = _2Opt(initialRand, randGraph.DeepCopy());
                _2optRest = _2Opt(initialRest, restGraph.DeepCopy());
                
                //Behalten des besten lokalen Optimums
                if (_2optRand.GetGesamtkosten() < optimizedRand.GetGesamtkosten())
                    optimizedRand = _2optRand.DeepCopy();
                if (_2optRest.GetGesamtkosten() < optimizedRest.GetGesamtkosten())
                    optimizedRest = _2optRest.DeepCopy();
            }                                                
            // Textoutput für Koordinate(X,Y,Z), Priority
            string path = "F:\\Uni\\Uni WS 17_18\\Studienprojekt\\ProgrammierKram\\GraphUmwandlung";
            using (StreamWriter outputFile = new StreamWriter(path + @"\Data.txt"))
            {
                uint index = 0;
                for (int i = 0; i < restGraph.GetVoxelKoordinaten().Count; i++)
                {
                    index = optimizedRest.GetPriorityItem(i);
                    outputFile.Write(restGraph.GetVoxelKoordinate(0, (int) index) + " " +
                                     restGraph.GetVoxelKoordinate(1, (int) index) + " " +
                                     restGraph.GetVoxelKoordinate(2, (int) index) + "\r\n");
                }
            }     
            
            using (StreamWriter outputFile = new StreamWriter(path + @"\DataINIT.txt"))
            {
                uint index = 0;
                for (int i = 0; i < restGraph.GetVoxelKoordinaten().Count; i++)
                {
                    index = initialRest.GetPriorityItem(i);
                    outputFile.Write(restGraph.GetVoxelKoordinate(0, (int) index) + " " +
                                     restGraph.GetVoxelKoordinate(1, (int) index) + " " +
                                     restGraph.GetVoxelKoordinate(2, (int) index) + "\r\n");
                }
            }   
        }
        /*       
        static void Main(string[] args)
        {
            Bahn bahn = new Bahn();
            List<Voxel> voxelList = new List<Voxel>(bahn.GenerateTestData());
            bahn.Bahnplanung(voxelList);
        }
        */
    }  
}