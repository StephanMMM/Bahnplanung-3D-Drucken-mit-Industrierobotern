using System;
using System.Collections.Generic;

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
            int i = 0;
            
            foreach (var v in voxel)
            {
                List<double> graphElemente = new List<double>();
                List<bool> absetzPunkte = new List<bool>();
                foreach (var w in voxel)
                {
                    int[] distanz = new int[3] {0, 0, 0};
                    distanz = v.VoxelKoordinatenDistanz(w);
                    
                    /*
                     * Berechne für alle Benachbarten und nicht benachbarten Knoten jeden Knotens, die Distanz
                     * zu den anderen Knoten. Füge die VoxelKoordinaten hinzu und die Punkte an denen abgesetzt
                     * werden muss.
                    */
                    if (v.IsNeighbor(w))
                    {
                        graphElemente.Add(Math.Sqrt(Math.Pow(distanz[0], 2) + Math.Pow(distanz[1], 2) + Math.Pow(distanz[2],2)));
                        absetzPunkte.Add(false);
                    }
                    else
                    {
                        graphElemente.Add(ABSETZKOSTEN + Math.Sqrt(Math.Pow(distanz[0], 2) + Math.Pow(distanz[1], 2) + Math.Pow(distanz[2],2)));
                        absetzPunkte.Add(true);
                    }                  
                }
                graph.AddGraphElement(graphElemente);             
                graph.AddAbsetzPunkt(absetzPunkte);
                graph.AddVoxelKoordinaten(v.getKoords());
            }
            return graph;
        }


        private static Graph_ MarkiereKnoten(int Knoten, Graph_ graph)
        {
            for(int i =0; i < graph.GetGraph().Count; i++)
            {
                graph.SetGraph(MARKIERKOSTEN, i, Knoten);
            }
            return graph;
        }
        
        /*
         * 1. Startvoxel markieren notwendig mit extra kosten
         * 2. Richtige Reihenfolge :/
         * 3. Kosten für Tour (nach der Funktion erforderllch, nicht in der Funktion)
         */
        private List<uint> NearestNeighbor(Graph_ graph)
        {
            // List zum Speichern der Druckreihenfolge der Voxel (Graphknoten)
            List<uint> priority = new List<uint>();
            
            // 1. Startpunkt = Erster Knoten
            /*
            //Markiere alle Knoten in der Liste die zu sich selbst mit Kosten 0 benachbart sind
            for (int i = 0; i < graph.GetGraph().Count; i++)
            {
                MarkiereKnoten(i, graph);
                Console.Write(graph.GetGraphElement(i,0));
                Console.Write(" ");
            }
                minimum = graph.GetGraphElement(0,1);
                priority.Add(1);
                MarkiereKnoten(1, graph);
            */
            
            /*
             * 2. Finde Kante mit geringsten Kosten vom Startknoten aus
             * Iteriere über die Anzahl der Elemente in jeder Liste                
            */
            int minimumKnotenNummer = 0;
            
            for (int i = 0; i < graph.GetGraph().Count; i++)
            {
                double minimum = 150;
                for (int j = 0; j < graph.GetGraph().Count; j++)
                {
                    if (graph.GetGraphElement(i, j) < minimum)
                    {
                        minimum = graph.GetGraphElement(i, j);
                        minimumKnotenNummer = i;
                    }
                }
                //3. Füge den Knoten in die Druckreihenfolge ein
                priority.Add((uint) minimumKnotenNummer);
                //4. Markiere den eingefügten Knoten für alle anderen Knoten (Kosten +100?)
                MarkiereKnoten(i, graph);
            }        
            return priority;
        }
        

        // Bahnplanungsalgorithmen
        public void Bahnplanung(List<Voxel> voxelList)
        {
            // Teilen der gesamten Voxelliste in Randvoxel und Rest, damit unterschiedliche Bahnen geplant werden können
            List<Voxel> rand = new List<Voxel>();
            List<Voxel> rest = new List<Voxel>();
            foreach (var v in voxelList)
            {
                if (v.getSchichtrand() == true)
                {
                    rand.Add(v);
                }
                else
                {
                    rest.Add(v);
                }
            }
            // Erstelle zwei Graphen : Randvoxel-Graph und Restvoxel-Graph
            Graph_ randGraph = new Graph_(VoxelToGraph(rand));
            Graph_ restGraph = new Graph_(VoxelToGraph(rest));

            // Erstellen der ersten Druckreihenfolge
            List<uint> priority_rand = NearestNeighbor(randGraph);
            List<uint> priority_rest = NearestNeighbor(restGraph);
            
            // Verbesserung der ersten Lösung durch 2-opt
        }
    }
}