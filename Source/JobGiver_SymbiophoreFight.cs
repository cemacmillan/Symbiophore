using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace DIL_Symbiophore
{
    public class JobGiver_SymbiophoreFight : ThinkNode_JobGiver
    {
        private readonly List<Ability> tmpAbilities = new List<Ability>();

        protected override Job TryGiveJob(Pawn pawn)
        {
            Log.Message($"JobGiver_SymbiophoreFight triggered for pawn: {pawn}");

            if (pawn.MentalStateDef != MentalStateDefOf.Manhunter)
            {
                Log.Message($"Pawn: {pawn} is not in Manhunter state.");
                return null;
            }

            Pawn target = FindValidTarget(pawn);
            if (target == null)
            {
                Log.Message($"No valid target found for pawn: {pawn}");
                return null;
            }

            Ability ability = GetRandomPossibleAbility(pawn, target);
            if (ability == null)
            {
                Log.Message($"No applicable ability found for pawn: {pawn}");
                return null;
            }

            Log.Message($"Pawn: {pawn} is casting ability: {ability.def.defName} on target: {target}");
            return ability.GetJob(target, null);
        }

        private Pawn FindValidTarget(Pawn pawn)
        {
            Log.Message($"Finding valid target for pawn: {pawn}");
            List<Pawn> potentialTargets = (List<Pawn>)pawn.Map.mapPawns.AllPawnsSpawned;
            Pawn bestTarget = null;
            float bestScore = float.MinValue;

            foreach (Pawn potentialTarget in potentialTargets)
            {
                if (IsValidPawn(potentialTarget))
                {
                    Log.Message($"Evaluating potential target: {potentialTarget}");
                    float score = ScoreTarget(pawn, potentialTarget);
                    Log.Message($"Score for target {potentialTarget}: {score}");
                    if (score > bestScore)
                    {
                        bestTarget = potentialTarget;
                        bestScore = score;
                    }
                }
            }

            Log.Message($"Best target found: {bestTarget}");
            return bestTarget;
        }

        private bool IsValidPawn(Pawn target)
        {
            // For symbiophore, all pawns are valid targets
            return true;
        }

        private float ScoreTarget(Pawn pawn, Pawn target)
        {
            float distance = (pawn.Position - target.Position).LengthHorizontalSquared;
            return 1f / distance;
        }

        private Ability GetRandomPossibleAbility(Pawn pawn, Thing target)
        {
            tmpAbilities.Clear();

            List<Ability> abilities = pawn.abilities.abilities;
            foreach (Ability ability in abilities)
            {
                if (ability.CanCast && ability.def.verbProperties.targetParams.CanTarget(target))
                {
                    tmpAbilities.Add(ability);
                }
            }

            if (tmpAbilities.Count == 0)
            {
                return null;
            }

            return tmpAbilities.RandomElement();
        }
    }
}