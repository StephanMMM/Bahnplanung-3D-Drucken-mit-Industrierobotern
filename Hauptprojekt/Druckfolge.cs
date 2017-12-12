using System.Collections.Generic;

namespace Project
{
    public class Druckfolge
    {
        private List<uint> m_priority;
        private uint m_gesamtKosten;

        //Konstruktoren
        public Druckfolge()
        {
            m_priority = new List<uint>();
            m_gesamtKosten = 0;
        }

        public Druckfolge(Druckfolge druckfolge)
        {
            m_priority = druckfolge.GetPriority();
            m_gesamtKosten = druckfolge.GetGesamtkosten();
        }
        
        //Getter
        public List<uint> GetPriority()
        {
            return m_priority;
        }

        public uint GetPriorityItem(int i)
        {
            return m_priority[i];
        }

        public uint GetGesamtkosten()
        {
            return m_gesamtKosten;
        }
            
        //Setter
        public void SetPriority(List<uint> priority)
        {
            m_priority = priority;
        }
        
        public void SetPriority(uint u, int i)
        {
            m_priority[i] = u;
        }

        public void SetGesamtkosten(uint u)
        {
            m_gesamtKosten = u;
        }
        
        //Add falls set nicht verfügbar
        public void AddPriority(uint u)
        {
            m_priority.Add(u);
        }

        //Addierer für Gesamtkosten
        public void SummiereGesamtkosten(uint u)
        {
            m_gesamtKosten += u;
        }
    }
}
