using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Werkzeugbahnplanung
{
    public class Voxel
    {
        private bool m_Schichtrand;
        private bool m_Modellrand;
        private ushort[] m_koordinaten;

        public Voxel()
        {
            m_Schichtrand = false;
            m_Modellrand = false;
            m_koordinaten = new ushort[3] { 0, 0, 0 };

        }

        public Voxel(bool schichtrand, bool modellrand, ushort xKoord, ushort yKoord, ushort zKoord)
        {
            m_Schichtrand = schichtrand;
            m_Modellrand = modellrand;
            m_koordinaten = new ushort[3];
            m_koordinaten[0] = xKoord;
            m_koordinaten[1] = yKoord;
            m_koordinaten[2] = zKoord;
        }

        public void setSchichtrand(bool value)
        {
            m_Schichtrand = value;
        }

        public bool getSchichtrand()
        {
            return m_Schichtrand;
        }

        public void setModellrand(bool value)
        {
            m_Modellrand = value;
        }

        public bool getModellrand()
        {
            return m_Modellrand;
        }

        public ushort[] getKoords()
        {
            return m_koordinaten;
        }
    }
}
