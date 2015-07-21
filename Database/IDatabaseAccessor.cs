﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseModels;

namespace Database
{
    public interface IDatabaseAccessor
    {
        void AddLeague(DLeague league);
        List<DProspect> GetTopProspects(int year, int count);
    }

    public class DatabaseAccessorSpoof : IDatabaseAccessor
    {
        public void AddLeague(DLeague league) { }
        public List<DProspect> GetTopProspects(int year, int count)
        {
            Queue<DProspect> allProspects = new Queue<DProspect>();
            /*
            DProspect matthews = new DProspect() 
            {
                Id = 1,
                Name = "Auston Matthews",
                Team = "European Europes",
                Position = "C",
                Handedness = "L",
                DraftYear = 2016
            };
            DProspect chychrun = new DProspect() 
            {
                Id = 2,
                Name = "Jakob Chychrun",
                Team = "Sarnia Sting",
                Position = "D",
                Handedness = "L",
                DraftYear = 2016
            };
            DProspect juolevi = new DProspect()
            {
                Id = 3,
                Name = "Olli Juolevi",
                Team = "Jokerit U20",
                Position = "D",
                Handedness = "L",
                DraftYear = 2016
            };
            DProspect connor = new DProspect()
            {
                Id = 4,
                Name = "Kyle Connor",
                Team = "Youngstown Phantoms",
                Position = "C",
                Handedness = "L",
                DraftYear = 2015
            };
            DProspect sReinhart = new DProspect()
            {
                Id = 5,
                Name = "Sam Reinhart",
                Team = "Kootenay Ice",
                Position = "C/RW",
                Handedness = "R",
                DraftYear = 2014
            };
            DProspect pettersson = new DProspect()
            {
                Id = 6,
                Name = "Elias Pettersson",
                Team = "Timra IK",
                Position = "C",
                Handedness = "L",
                DraftYear = 2017
            };

            allProspects.Enqueue(matthews);
            allProspects.Enqueue(chychrun);
            allProspects.Enqueue(juolevi);
            allProspects.Enqueue(connor);
            allProspects.Enqueue(sReinhart);
            allProspects.Enqueue(pettersson);
            */
            List<DProspect> returnedProspects = new List<DProspect>();
            if (count == -1) count = allProspects.Count;
            for (int i = 0; i < count; i++)
            {
                if (allProspects.Count == 0) break;
                var prospect = allProspects.Dequeue();
                if (prospect.DraftYear == year)
                {
                    returnedProspects.Add(prospect);
                }
                else i--;
            }

            return returnedProspects;
        }
    }
}
