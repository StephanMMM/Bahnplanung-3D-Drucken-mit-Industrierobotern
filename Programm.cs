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

        public Voxelmodell Input(string filename)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "\\");
            path += filename;
            if (!File.Exists(path)) { return null; }
            string[] lines = System.IO.File.ReadAllLines(@filename);
            string[] tokens = lines[0].Split(',');
            int boundB_x = 0, boundB_y = 0, boundB_z = 0, anzSchichten = 0;
            Voxel[,,] voxelmatrix = new Voxel[boundB_x, boundB_y, boundB_z];
            List<List<Voxel>> schichten = new List<List<Voxel>>();
            if (Int32.TryParse(tokens[0], out boundB_x) &&
                Int32.TryParse(tokens[1], out boundB_y) &&
                Int32.TryParse(tokens[2], out boundB_z) &&
                Int32.TryParse(tokens[3], out anzSchichten))
            {
                for (int i = 0; i < anzSchichten; i++)
                {
                    schichten.Add(new List<Voxel>());
                }
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] voxelparam = lines[i].Split(',');
                    ushort[] voxelKoords = new ushort[3];
                    int schicht = 0;
                    bool schichtrand = false, modellrand = false;
                    if (ushort.TryParse(voxelparam[0], out voxelKoords[0]) &&
                        ushort.TryParse(voxelparam[1], out voxelKoords[1]) &&
                        ushort.TryParse(voxelparam[2], out voxelKoords[2]) &&
                        bool.TryParse(voxelparam[3], out schichtrand) &&
                        bool.TryParse(voxelparam[4], out modellrand) &&
                        Int32.TryParse(voxelparam[5], out schicht))
                    {
                        voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]] = new Voxel(schichtrand, modellrand, voxelKoords[0], voxelKoords[1], voxelKoords[2]);
                        schichten[i].Add(voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]]);
                    }
                    else
                    {
                        continue;
                    }
                }
                Voxelmodell voxelmodell = new Voxelmodell(anzSchichten, boundB_x, boundB_y, boundB_z, voxelmatrix, schichten);
                return voxelmodell;
            }
            else
            {
                return null;
            }

        }
    }
}
