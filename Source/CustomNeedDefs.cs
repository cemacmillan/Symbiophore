using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    public static class CustomNeedDefs
    {
        public static NeedDef SymbiophoreMood;
        public static NeedDef SymbiophoreFood;
        public static NeedDef SymbiophoreSleep;
        public static NeedDef SymbiophoreFullness;

        static CustomNeedDefs()
        {
            SymbiophoreMood = new NeedDef
            {
                defName = "SymbiophoreMood",
                needClass = typeof(CustomNeed),
                label = "Mood",
                description = "Mood affected by Symbiophores.",
                listPriority = 600,
                major = true,
                showForCaravanMembers = true,
                developmentalStageFilter = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult,
                showUnitTicks = true
            };
            DefDatabase<NeedDef>.Add(SymbiophoreMood);

            SymbiophoreFood = new NeedDef
            {
                defName = "SymbiophoreFood",
                needClass = typeof(CustomNeed),
                label = "Food",
                description = "Food level for Symbiophores.",
                listPriority = 500,
                major = true,
                showForCaravanMembers = true,
                developmentalStageFilter = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult,
                showUnitTicks = true
            };
            DefDatabase<NeedDef>.Add(SymbiophoreFood);

            SymbiophoreSleep = new NeedDef
            {
                defName = "SymbiophoreSleep",
                needClass = typeof(CustomNeed),
                label = "Sleep",
                description = "Sleep level for Symbiophores.",
                listPriority = 400,
                major = true,
                showForCaravanMembers = true,
                developmentalStageFilter = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult
            };
            DefDatabase<NeedDef>.Add(SymbiophoreSleep);

            SymbiophoreFullness = new NeedDef
            {
                defName = "SymbiophoreFullness",
                needClass = typeof(CustomNeed),
                label = "Silk Fullness",
                description = "Fullness level for Symbiophores.",
                listPriority = 300,
                major = true,
                showForCaravanMembers = true,
                developmentalStageFilter = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult
            };
            DefDatabase<NeedDef>.Add(SymbiophoreFullness);
        }
    }
}
