using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace DIL_Symbiophore.Signals
{
    /// <summary>
    /// Soft query of Witness map humidity. No compile-time Witness reference.
    /// </summary>
    public static class WitnessHumidityReflection
    {
        private static bool resolved;
        private static bool available;
        private static MethodInfo getExperiencedHumidity;

        /// <summary>
        /// Tries Witness experienced humidity for the pawn's map.
        /// Returns false if Witness is inactive or the API cannot be resolved.
        /// </summary>
        public static bool TryGetHumidity(Pawn pawn, out float humidity01)
        {
            humidity01 = 0f;
            if (pawn == null || !ModsConfig.IsActive("cem.witness"))
            {
                return false;
            }

            if (!EnsureResolved() || getExperiencedHumidity == null)
            {
                return false;
            }

            try
            {
                object result = getExperiencedHumidity.Invoke(null, new object[] { pawn });
                if (result is float f)
                {
                    humidity01 = f;
                    return true;
                }
            }
            catch (TargetInvocationException)
            {
                // Soft dep: stay silent; caller keeps stub.
            }
            catch (ArgumentException)
            {
            }

            return false;
        }

        private static bool EnsureResolved()
        {
            if (resolved)
            {
                return available;
            }

            resolved = true;
            Type observationType = AccessTools.TypeByName("Witness.HumidityObservation");
            if (observationType == null)
            {
                available = false;
                return false;
            }

            getExperiencedHumidity = AccessTools.Method(observationType, "GetExperiencedHumidity", new[] { typeof(Pawn) });
            available = getExperiencedHumidity != null;
            return available;
        }
    }
}
