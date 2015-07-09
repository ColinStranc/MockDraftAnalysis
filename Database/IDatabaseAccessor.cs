using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseModels;

namespace Database
{
    public interface IDatabaseAccessor
    {
        List<DProspect> GetTopProspects(int count);
    }

    public class DatabaseAccessorSpoof : IDatabaseAccessor
    {
        public List<DProspect> GetTopProspects(int count)
        {
            Queue<DProspect> allProspects = new Queue<DProspect>();

            DProspect matthews = new DProspect() 
            {
                Name = "Auston Matthews",
                Team = "European Europes"
            };
            DProspect chychrun = new DProspect() 
            {
                Name = "Jakob Chychrun",
                Team = "Sarnia Sting"
            };
            DProspect juolevi = new DProspect()
            {
                Name = "Olli Juolevi",
                Team = "Jokerit U20"
            };

            allProspects.Enqueue(matthews);
            allProspects.Enqueue(chychrun);
            allProspects.Enqueue(juolevi);

            List<DProspect> returnedProspects = new List<DProspect>();
            for (int i = 0; i < count; i++)
            {
                if (allProspects.Count == 0) break;
                returnedProspects.Add(allProspects.Dequeue());
            }

            return returnedProspects;
        }
    }
}
