﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ZLevels
{
    public static class Toils_ZLevels
    {

        public static IEnumerable<Toil> GoToMap(Pawn pawn, Map dest, JobDriver instance)
        {
            Toil end = new Toil
            {
                initAction = delegate ()
                {
                    ZLogger.Message("if (pawn.Map == dest): " + pawn.Map + " - " + dest);
                }
            };
            yield return new Toil
            {
                initAction = delegate ()
                {
                    if (pawn.Map == dest)
                    {
                        instance.JumpToToil(end);
                    }
                }
            };

            Toil setStairs = new Toil
            {
                initAction = delegate ()
                {
                    var ZTracker = ZUtils.ZTracker;
                    ZTracker.ReCheckStairs();
                    ZLogger.Message("1 Total count of stairs up: " 
                        + pawn.Map.listerThings.AllThings.Where(x => x is Building_StairsUp).Count());
                    ZLogger.Message("1 Total count of stairs down: "
                        + pawn.Map.listerThings.AllThings.Where(x => x is Building_StairsDown).Count());

                    ZLogger.Message("2 Total count of stairs up: " + ZTracker.stairsUp[pawn.Map].Count);
                    ZLogger.Message("2 Total count of stairs down: " + ZTracker.stairsDown[pawn.Map].Count);
                    ZLogger.Message("pawn: " + pawn);
                    ZLogger.Message("pawn.Map: " + pawn.Map);
                    ZLogger.Message("dest: " + dest);
                    if (ZTracker.GetZIndexFor(pawn.Map) > ZTracker.GetZIndexFor(dest))
                    {
                        var stairs = ZTracker.stairsDown[pawn.Map];
                        if (stairs?.Count() > 0)
                        {
                            pawn.CurJob.targetC = new LocalTargetInfo(stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position)));
                        }
                        else
                        {
                            ZLogger.Pause(pawn + " cant find stairs down");
                        }
                    }
                    else if (ZTracker.GetZIndexFor(pawn.Map) < ZTracker.GetZIndexFor(dest))
                    {
                        var stairs = ZTracker.stairsUp[pawn.Map];
                        if (stairs?.Count() > 0)
                        {
                            pawn.CurJob.targetC = new LocalTargetInfo(stairs.MinBy(y => IntVec3Utility.DistanceTo(pawn.Position, y.Position)));
                        }
                        else
                        {
                            ZLogger.Pause(pawn + " cant find stairs up");
                        }
                    }
                    else
                    {
                        pawn.CurJob.targetC = null;
                    }

                }
            };
            var goToStairs = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell);
            Toil useStairs = Toils_General.Wait(60, 0);
            ToilEffects.WithProgressBarToilDelay(useStairs, TargetIndex.C, false, -0.5f);
            //ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(useStairs, TargetIndex.C);
            //ToilFailConditions.FailOnCannotTouch<Toil>(useStairs, TargetIndex.C, PathEndMode.OnCell);

            Toil teleport = new Toil
            {
                initAction = delegate ()
                {
                    var ZTracker = ZUtils.ZTracker;
                    if (pawn.CurJob.targetC.Thing is Building_StairsUp stairsUp)
                    {
                        Map map = ZTracker.GetUpperLevel(pawn.Map.Tile, pawn.Map);
                        if (map == null)
                        {
                            map = ZTracker.CreateUpperLevel(pawn.Map, stairsUp.Position);
                            if (stairsUp.pathToPreset != null && stairsUp.pathToPreset.Length > 0)
                            {
                                var comp = map.GetComponent<MapComponentZLevel>();
                                comp.DoGeneration = true;
                                comp.path = stairsUp.pathToPreset;
                            }
                            ZTracker.TeleportPawn(pawn, pawn.Position, map, true, false, true);
                        }
                        else
                        {
                            if (stairsUp.pathToPreset != null && stairsUp.pathToPreset.Length > 0)
                            {
                                var comp = map.GetComponent<MapComponentZLevel>();
                                comp.DoGeneration = true;
                                comp.path = stairsUp.pathToPreset;
                            }
                            ZTracker.TeleportPawn(pawn, pawn.Position, map, false, false, stairsUp.shouldSpawnStairsUpper);
                            stairsUp.shouldSpawnStairsUpper = false;
                        }
                    }
                    else if (pawn.CurJob.targetC.Thing is Building_StairsDown stairsDown)
                    {
                        Map map = ZTracker.GetLowerLevel(pawn.Map.Tile, pawn.Map);
                        if (map == null)
                        {
                            //map = ZTracker.CreateLowerLevel(pawn.Map, stairsDown.Position);
                            //if (stairsDown.pathToPreset != null && stairsDown.pathToPreset.Length > 0)
                            //{
                            //    var comp = map.GetComponent<MapComponentZLevel>();
                            //    comp.DoGeneration = true;
                            //    comp.path = stairsDown.pathToPreset;
                            //}
                            //ZTracker.TeleportPawn(pawn, pawn.Position, map, true, true);
                        }
                        else
                        {
                            if (stairsDown.pathToPreset != null && stairsDown.pathToPreset.Length > 0)
                            {
                                var comp = map.GetComponent<MapComponentZLevel>();
                                comp.DoGeneration = true;
                                comp.path = stairsDown.pathToPreset;
                            }
                            ZTracker.TeleportPawn(pawn, pawn.Position, map, false, stairsDown.shouldSpawnStairsBelow);
                            stairsDown.shouldSpawnStairsBelow = false;
                        }
                    }

                    if (pawn.Map != dest)
                    {
                        instance.JumpToToil(setStairs);
                    }
                }
            };

            yield return setStairs;
            yield return goToStairs;
            yield return useStairs;
            yield return teleport;
            yield return end;
        }
    }
}

