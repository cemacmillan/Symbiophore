using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    public class Pawn_MindState
    {
        private Pawn pawn;
        public SymbiophoreThoughtHandler symbiophoreThoughts;

        public Pawn_MindState(Pawn pawn)
        {
            this.pawn = pawn;
            symbiophoreThoughts = new SymbiophoreThoughtHandler(pawn);
        }

        public class SymbiophoreThoughtHandler
        {
            private Pawn pawn;

            public SymbiophoreThoughtHandler(Pawn pawn)
            {
                this.pawn = pawn;
            }

          

            public void AddSymbiophoreThoughts()
            {
                
            }
        }
    }

}
