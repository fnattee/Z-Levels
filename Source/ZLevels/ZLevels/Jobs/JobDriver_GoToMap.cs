﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ZLevels
{
    public class JobDriver_GoToMap : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ZLogger.Message(pawn + " 1 ZUtils.ZTracker.jobTracker[pawn].dest: " + ZUtils.ZTracker.jobTracker[pawn].dest);
            foreach (var toil in Toils_ZLevels.GoToMap(GetActor(), ZUtils.ZTracker.jobTracker[pawn].dest, this))
            {
                ZLogger.Message(pawn + " 2 ZUtils.ZTracker.jobTracker[pawn].dest: " + ZUtils.ZTracker.jobTracker[pawn].dest);
                yield return toil;
            }
        }
    }
}

