using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Werkzeugbahnplanung
{
    public class Voxelmodell
    {
        private int m_AnzahlSchichten;
        private int m_Boundingbox_x;
        private int m_Boundingbox_y;
        private int m_Boundingbox_z;
        //3-D Voxelmodell
        private Voxel[,,] m_Voxelmatrix;
        //Liste auf die entsprechenden Schichten-Listen
        private List<List<Voxel>> m_Schichten;

        //Konstruktor für Input-Funktion vorgesehen
        public Voxelmodell(int anzahlSchichten, int Bb_x, int Bb_y, int Bb_z, Voxel[,,] voxelmatrix, List<List<Voxel>> schichten)
        {
            m_AnzahlSchichten = anzahlSchichten;
            m_Boundingbox_x = Bb_x;
            m_Boundingbox_y = Bb_y;
            m_Boundingbox_z = Bb_z;
            m_Voxelmatrix = voxelmatrix;
            m_Schichten = schichten;
        }

        /*Funktion, die eine Boundingbox eines Infill-Musters
          (derselben Größe(!)) mit dem Voxelmodell merged.
          Bounding-Box : true = Voxel gesetzt im Infill */
        public void InsertInfill(bool[,,] boundingBox)
        {
            ushort[] koords = new ushort[3];
            //Schleifen die über alle Voxel des Modells gehen
            foreach (List<Voxel> schicht in m_Schichten)
            {
                foreach (var voxel in schicht)
                {
                    //Voxel die Teil des Randes sind kommen nicht in Frage
                    if (voxel.getSchichtrand() != true)
                    {
                        koords = voxel.getKoords();
                        //Falls kein Infill an Stelle des Voxels, lösche diesen
                        //aus unserem Voxelmodell
                        if(!boundingBox[koords[0], koords[1], koords[2]])
                        {
                            m_Voxelmatrix[koords[0], koords[1], koords[2]] = null;
                            schicht.Remove(voxel); 
                        }
                    }
                }
            }
        }

    }
    
        #region Randverbreiterung
        /// <summary>
        /// Diese Methode ist dazu da, um den Modellrand (voll zu druckenden äußeren Bereich) verbreitern.
        /// Der Parameter randBreite soll die gewünschte resultierende Breite dieses Bereiches sein.
        /// </summary>
        /// <param name="randBreite"></param>
        public void randVerbreiterung(int randBreite)
        {
            //Voxel werden erst in eine Liste eingetragen, bevor sie zum Rand hinzugefügt werden
            List<int[]> positionenHinzufügenderVoxel = new List<int[]>();
            /*Es werden immer benachbarte Voxel hinzugefügt.
            Um die gewünschte Breite zu erreichen, muss der Vorgang entsprechend oft durchgeführt werden.*/
            for (int i = 0; i < randBreite - 1; i++)
            {
                //Die Verwendung einer foreach Schleife ist hier nicht möglich, da die Koodidaten des zu betrachtenden Voxels benötigt werden.
                foreach (Voxel voxel in m_Voxelmatrix)
                {
                    ushort x = voxel.getKoords()[0];
                    ushort y = voxel.getKoords()[1];
                    ushort z = voxel.getKoords()[2];
                    /*Es sind nur Voxel relevant, die zum Rand gehören.
                    Leere Voxel müssen vorher aussortiert werden, weil wir nicht auf Attribute leerer Objekte zugreifen können.
                    Die zweite Bedingung wird nicht ausgeführt, wenn die erste nicht erfüllt ist -> Fehlervermeidung*/
                    if (voxel != null && voxel.getModellrand() == true)
                    {
                        /*Jeder Voxel hat 6, 18 oder 26 Nachbarn, je nach dem ob man Nachbarschaften über Flächen, Kanten und/oder Ecken als Nachbarschaft ansieht.
                         Die Programmierung ist auf 26 ausgelegt.
                         Bei jedem möglichen Nachbar ist zu prüfen, ob dieser im Modell liegt(nicht out of range) und existiert (nicht null)
                         Die Prüfung wurde in eine neue Methode ausgelagert. Sie wird mit allen Voxeln aufgerunfen, bei denen sich jede Koodinate, um max 1 unterscheiden.*/
                        for (int x_div = -1; x_div <= 1; x_div++)
                        {
                            for (int y_div = -1; y_div <= 1; y_div++)
                            {
                                for (int z_div = -1; z_div <= 1; z_div++)
                                {
                                    if (pruefeVoxelAufVerbreiterung(x + x_div, y + y_div, z + z_div))
                                    {
                                        int[] pos = { x + x_div, y + y_div, z + z_div };
                                        positionenHinzufügenderVoxel.Add(pos);
                                    }
                                }
                            }
                        }
                    }
                    //Damit das Verändern von Voxeln sich erst auf die nächste Iteration auswirkt, dürfen die Voxel nicht direkt verändert werden
                    foreach (int[] pos in positionenHinzufügenderVoxel)
                    {
                        m_Voxelmatrix[pos[0], pos[1], pos[2]].setModellrand(true);
                    }
                    positionenHinzufügenderVoxel.Clear();
                }
            }
        }
        /// <summary>
        /// Diese Methode überprüft ob die übergebenen Koodinaten im Modell liegen und wenn ja ob der Voxel existiert und wenn ja ob er noch nicht zum Rand gehört
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private bool pruefeVoxelAufVerbreiterung(int x, int y, int z)
        {
            return (x >= 0) && (x < m_Boundingbox_x) && //liegt mit der x-Koodinate im Modell
                (y >= 0) && (y < m_Boundingbox_y) && //liegt mit der y-Koodinate im Modell
                (z >= 0) && (z < m_Boundingbox_z) && //liegt mit der z-Koodinate im Modell
                (m_Voxelmatrix[x, y, z] != null) && //nicht null
                (m_Voxelmatrix[x, y, z].getModellrand() == false); //und noch kein Rand
        }
        #endregion
    }
}
