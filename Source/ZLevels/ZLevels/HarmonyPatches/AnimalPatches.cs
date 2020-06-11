﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ZLevels
{
    [StaticConstructorOnStartup]
    public static class AnimalPatches
    {
        [HarmonyPatch(typeof(WildAnimalSpawner), "SpawnRandomWildAnimalAt")]
        public class AnimalPatch2
        {
            [HarmonyPrefix]
            private static bool AnimalRemovalPrefix(WildAnimalSpawner __instance, ref IntVec3 loc, ref bool __result)
            {
                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                bool result = false;
                var comp = map.GetComponent<MapComponentZLevel>();
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                if (map.Parent is MapParent_ZLevel && comp != null 
                    && ZTracker.GetUpperLevel(map.Tile, map) != null &&
                    !ZTracker.GetUpperLevel(map.Tile, map).GetComponent<MapComponentZLevel>()
                    .hasCavesBelow.GetValueOrDefault(false))
                {
                    result = false;
                }
                else
                {
                    PawnKindDef pawnKindDef = (from a in map.Biome.AllWildAnimals
                                               where map.mapTemperature.SeasonAcceptableFor(a.race) select a)
                                               .RandomElementByWeight((PawnKindDef def) =>
                                               map.Biome.CommonalityOfAnimal(def) / def.wildGroupSize.Average);
                    if (pawnKindDef == null)
                    {
                        ZLogger.Error("No spawnable animals right now.");
                        result = false;
                    }
                    else
                    {
                        int randomInRange = pawnKindDef.wildGroupSize.RandomInRange;
                        int radius = Mathf.CeilToInt(Mathf.Sqrt((float)pawnKindDef.wildGroupSize.max));
                        if (map.Parent is MapParent_ZLevel && !loc.Walkable(map))
                        {
                            loc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Walkable(map), map);
                        }
                        for (int i = 0; i < randomInRange; i++)
                        {
                            IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(loc, map, radius, null);
                            GenSpawn.Spawn(PawnGenerator.GeneratePawn(pawnKindDef, null), loc2, map, WipeMode.Vanish);
                        }
                        result = true;
                    }
                }
                __result = result;
                return false;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_Infestation), "TryExecuteWorker")]
        internal class Patch_Infestation_TryExecuteWorker
        {
            [HarmonyPrefix]
            private static bool PreFix(ref bool __result, IncidentParms parms)
            {
                Map map = (Map)parms.target;
                var comp = map.GetComponent<MapComponentZLevel>();
                if (comp.hasCavesBelow.HasValue && comp.hasCavesBelow.Value)
                {
                    var foods = map.listerThings.AllThings.Where(x => !(x is Plant) && !(x is Pawn)
                            && x.GetStatValue(StatDefOf.Nutrition, true) > 0.1f);
                    if (foods != null && foods.Count() > 0)
                    {
                        List<PawnKindDef> infestators = new List<PawnKindDef>();
                        infestators.Add(ZLevelsDefOf.ZL_UndegroundBiome.AllWildAnimals.RandomElement());
                        var infestationPlace = foods.RandomElement().Position;
                        var infestationData = new InfestationData(infestators, parms.points, infestationPlace);
                        if (comp.ActiveInfestations == null)
                        {
                            comp.ActiveInfestations = new List<InfestationData>();
                        }
                        comp.ActiveInfestations.Add(infestationData);
                        if (comp.TotalInfestations == null)
                        {
                            comp.TotalInfestations = new List<InfestationData>();
                        }
                        comp.TotalInfestations.Add(infestationData);
                        var naturalHole = (Building_StairsDown)ThingMaker.MakeThing(ZLevelsDefOf.ZL_NaturalHole);
                        naturalHole.infestationData = infestationData;
                        GenSpawn.Spawn(naturalHole, infestationPlace, map, WipeMode.Vanish);
                        Find.LetterStack.ReceiveLetter("ZLevelInfestation"
                            .Translate(infestators.RandomElement().race.label), "ZLevelInfestationDesc".Translate(),
                            LetterDefOf.ThreatBig, naturalHole);
                    }
                }
                else
                {
                    ZLogger.Message("The map has no caves below to generate infestation");
                }
                __result = false;
                return false;
            }
        }
    }
}

