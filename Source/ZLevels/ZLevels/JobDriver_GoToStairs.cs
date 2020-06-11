﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ZLevels
{
    public class JobDriver_GoToStairs : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil useStairs = Toils_General.Wait(60, 0);
            ToilEffects.WithProgressBarToilDelay(useStairs, TargetIndex.A, false, -0.5f);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(useStairs, TargetIndex.A);
            ToilFailConditions.FailOnCannotTouch<Toil>(useStairs, TargetIndex.A, PathEndMode.OnCell);
            yield return useStairs;

            yield return new Toil
            {
                initAction = delegate ()
                {
                    var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                    if (TargetA.Thing is Building_StairsUp stairsUp)
                    {
                        Map map = ZTracker.GetUpperLevel(this.pawn.Map.Tile, this.pawn.Map);
                        if (map == null)
                        {
                            map = stairsUp.Create(this.pawn.Map);
                            if (stairsUp.pathToPreset != null && stairsUp.pathToPreset.Length > 0)
                            {
                                var comp = map.GetComponent<MapComponentZLevel>();
                                comp.DoGeneration = true;
                                comp.path = stairsUp.pathToPreset;
                            }
                            ZTracker.TeleportPawn(GetActor(), GetActor().Position, map, true, false, stairsUp.shouldSpawnStairsUpper);
                            stairsUp.shouldSpawnStairsUpper = false;
                        }
                        else
                        {
                            if (stairsUp.pathToPreset != null && stairsUp.pathToPreset.Length > 0)
                            {
                                var comp = map.GetComponent<MapComponentZLevel>();
                                comp.DoGeneration = true;
                                comp.path = stairsUp.pathToPreset;
                            }
                            ZTracker.TeleportPawn(GetActor(), GetActor().Position, map, false, false, stairsUp.shouldSpawnStairsUpper);
                            stairsUp.shouldSpawnStairsUpper = false;
                        }
                    }

                    else if (TargetA.Thing is Building_StairsDown stairsDown)
                    {
                        Map map = ZTracker.GetLowerLevel(this.pawn.Map.Tile, this.pawn.Map);
                        if (map == null)
                        {
                            map = stairsDown.Create(this.pawn.Map);
                            if (stairsDown.pathToPreset != null && stairsDown.pathToPreset.Length > 0)
                            {
                                var comp = map.GetComponent<MapComponentZLevel>();
                                comp.DoGeneration = true;
                                comp.path = stairsDown.pathToPreset;
                            }
                            ZTracker.TeleportPawn(GetActor(), GetActor().Position, map, true, stairsDown.shouldSpawnStairsBelow);
                            stairsDown.shouldSpawnStairsBelow = false;
                        }
                        else
                        {
                            if (stairsDown.pathToPreset != null && stairsDown.pathToPreset.Length > 0)
                            {
                                var comp = map.GetComponent<MapComponentZLevel>();
                                comp.DoGeneration = true;
                                comp.path = stairsDown.pathToPreset;
                            }
                            ZTracker.TeleportPawn(GetActor(), GetActor().Position, map, false, stairsDown.shouldSpawnStairsBelow);
                            stairsDown.shouldSpawnStairsBelow = false;
                        }
                    }
                }
            };
        }
    }
}
