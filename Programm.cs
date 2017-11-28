using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Werkzeugbahnplanung
{
    class Programm
    {
        static void Main(string[] args)
        {

        }
        #region Input-Method
        /*Funktion für eine Input-Textdatei
          entsprechend der Einigung
          1. Zeile : Grundwerte Voxelmodell
          2.-N. Zeile: Voxel
          Als Ausgabewert wird das fertige Voxelmodell
          zurückgegeben */
        public Voxelmodell Input(string filename)
        {
            //Erstelle Dateipfad und checke ob Datei vorhanden
            var path = Path.Combine(Directory.GetCurrentDirectory(), "\\");
            path += filename;
            if (!File.Exists(path)) { return null; }
            //Speichere Zeile für Zeile die Daten ab
            string[] lines = System.IO.File.ReadAllLines(@path);
            //
            string[] tokens = lines[0].Split(',');
            int boundB_x = 0, boundB_y = 0, boundB_z = 0, anzSchichten = 0;
            List<List<Voxel>> schichten = new List<List<Voxel>>();
            //Versuche Werte aus der 1. Zeile zu parsen ansonsten Abbruch
            if (Int32.TryParse(tokens[0], out boundB_x) &&
                Int32.TryParse(tokens[1], out boundB_y) &&
                Int32.TryParse(tokens[2], out boundB_z) &&
                Int32.TryParse(tokens[3], out anzSchichten))
            {
                //Erstelle für jede Schicht eine eigene Liste
                for (int i = 0; i < anzSchichten; i++)
                {
                    schichten.Add(new List<Voxel>());
                }
                //Erstelle Voxelmatrix entsprechend den Dimensionen des Modells
                Voxel[,,] voxelmatrix = new Voxel[boundB_x, boundB_y, boundB_z];
                //Schleife über alle Zeilen(bis auf die erste)
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] voxelparam = lines[i].Split(',');
                    ushort[] voxelKoords = new ushort[3];
                    int schicht = 0;
                    bool schichtrand = false, modellrand = false;
                    //Versuche alle Werte der Zeile zu parsen ansonsten gehe zur nächsten Zeile
                    if (ushort.TryParse(voxelparam[0], out voxelKoords[0]) &&
                        ushort.TryParse(voxelparam[1], out voxelKoords[1]) &&
                        ushort.TryParse(voxelparam[2], out voxelKoords[2]) &&
                        bool.TryParse(voxelparam[3], out schichtrand) &&
                        bool.TryParse(voxelparam[4], out modellrand) &&
                        Int32.TryParse(voxelparam[5], out schicht))
                    {
                        //Erstelle anhand der Werte einen neuen Voxel in der Matrix 
                        //und in der entsprechenden Schicht
                        voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]] = new Voxel(schichtrand, modellrand, voxelKoords[0], voxelKoords[1], voxelKoords[2]);
                        schichten[i].Add(voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]]);
                    }
                    else
                    {
                        continue;
                    }
                }
                //Erstellt Voxelmodell wenn Datei vollkommen durchlaufen und gebe diese zurück
                Voxelmodell voxelmodell = new Voxelmodell(anzSchichten, boundB_x, boundB_y, boundB_z, voxelmatrix, schichten);
                return voxelmodell;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
