using System;
using System.Collections.Generic;
using System.IO;

namespace Project
{
    public class Bahn
    {
        //Festlegen von Absetzkosten 
        private const int ABSETZKOSTEN = 30;
        private const int MARKIERKOSTEN = 100;
        
        
        /*
         * Generiere eine Voxelliste für Testzwecke. Dabei wird der Schichtrand-Tag zufällig gesetzt.
         */
        public List<Voxel> GenerateTestData()
        {
            List<Voxel> testVoxel = new List<Voxel>();
            //Random inkludiert nur die untere Grenze die obere wird nicht erreicht: Hier also Zufallszahl zwischen -1 == true und 0 == false
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
                     * zu den anderen Knoten. Füge die VoxelKoordinaten hinzu und die Punkte an denen abgesetzt
                     * werden muss.
                    */
                    /*
                     * Nachbarschaftsfunktion je nach Druckwunsch auswählen
                     */
                    if (v.IsNeighbor6(w))                  
                        graphElemente.Add(Math.Sqrt(Math.Pow(distanz[0], 2) + Math.Pow(distanz[1], 2) + Math.Pow(distanz[2],2)));
                    
                    else                 
                        graphElemente.Add(ABSETZKOSTEN + Math.Sqrt(Math.Pow(distanz[0], 2) + Math.Pow(distanz[1], 2) + Math.Pow(distanz[2],2)));
                                      
                }
                graph.AddGraphElement(graphElemente);             
                graph.AddVoxelKoordinaten(v.getKoords());
            }
            return graph;
        }


        //Markiert jeweils die Kosten eines Knotens zu sich selber
        private Graph_ MarkiereEigenknoten(int knoten, Graph_ graph)
        {
            for(int i = 0; i < graph.GetGraph().Count; i++)
            {
                // Wegen der Matrixeigenschaft der Liste von Listen im Graph
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
        private Druckfolge NearestNeighbor(Graph_ graph)
        {
            // List zum Speichern der Druckreihenfolge der Voxel (Graphknoten)
            Druckfolge druckfolge = new Druckfolge();
            Graph_ workingGraph = new Graph_(graph);

            // Markiere die Diagonale in der Kostenmatrix mit zusätzlichen Kosten
            MarkiereEigenknoten(0, workingGraph);
            
            // 1. Startpunkt = Erster Knoten
            int aktuellerKnoten = 0;
            int minimumKnotenNummer = 0;       
            
            // Füge den Startknoten in die Druckreihenfolge ein
            druckfolge.AddPriority(0);
                 
            for (int i = 0; i < workingGraph.GetGraph().Count - 1; i++)
            {
                // Aktualisiere den aktuellen Knoten
                aktuellerKnoten = minimumKnotenNummer;
                
                // Markiere aktuellen Knoten mit zusätzlichen Kosten
                MarkiereKnoten(aktuellerKnoten, workingGraph);
                
                double minimum = MARKIERKOSTEN*10;
                for (int j = 0; j < workingGraph.GetGraph().Count; j++)
                {
                    if (workingGraph.GetGraphElement(aktuellerKnoten, j) < minimum)
                    {
                        minimum = workingGraph.GetGraphElement(aktuellerKnoten, j);
                        minimumKnotenNummer = j;
                    }
                }
                // Füge den Knoten mit günstigster Kante in die Druckreihenfolge ein               
                druckfolge.AddPriority((uint) minimumKnotenNummer);
                druckfolge.SummiereGesamtkosten((uint) minimum);
            }        
            return druckfolge;
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
        
        // Bahnplanungsalgorithmen
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
            Druckfolge druckFolgeRand = new Druckfolge(NearestNeighbor(randGraph));
            Druckfolge druckFolgeRest = new Druckfolge(NearestNeighbor(restGraph));
                        
            // Textoutput für Koordinate(X,Y,Z), Priority
            string path = "F:\\Uni\\Uni WS 17_18\\Studienprojekt\\ProgrammierKram\\GraphUmwandlung";
            using (StreamWriter outputFile = new StreamWriter(path + @"\Data.txt"))
            {
                uint index = 0;
                for (int i = 0; i < randGraph.GetVoxelKoordinaten().Count; i++)
                {
                    index = druckFolgeRand.GetPriorityItem(i);
                    outputFile.Write(randGraph.GetVoxelKoordinate((int) index, 0) + " " +
                                     randGraph.GetVoxelKoordinate((int) index, 1) + " " +
                                     randGraph.GetVoxelKoordinate((int) index, 2) + "\r\n");
                }
            }           
            // Verbesserung der ersten Lösung durch 2-opt
        }
               
        static void Main(string[] args)
        {
            Bahn bahn = new Bahn();
            List<Voxel> voxelList = new List<Voxel>(bahn.GenerateTestData());
            bahn.Bahnplanung(voxelList);
        }
        
    }  
}
