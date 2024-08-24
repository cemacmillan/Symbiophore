using System.Linq;
using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    public class IncidentWorker_SymbiophoreJoin : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            Map map = (Map)parms.target;

            // Do not fire the incident if a Symbiophore already exists on the map
            return !map.mapPawns.AllPawns.Any(p => p.kindDef.defName == "Symbiophore");
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(map.Center, map, 10);
            PawnKindDef symbiophoreKind = PawnKindDef.Named("Symbiophore");

            // Create the Symbiophore
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(symbiophoreKind, Faction.OfPlayer));
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random);

            // Send a letter to the player about the incident
            string letterLabel = "A lost Symbiophore joins";
            string letterText = "A lost Symbiophore has wandered into your colony. It seems to want to stay.";
            SendStandardLetter(new TaggedString(letterLabel), new TaggedString(letterText), LetterDefOf.PositiveEvent, parms, pawn, new NamedArgument(pawn.Label, "PAWN"));

            return true;
        }

    }
}
