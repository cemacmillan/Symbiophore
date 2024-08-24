using System.Collections.Generic;
using Verse;

namespace DIL_Symbiophore
{
    public static class MoodProxyManager
    {
        private static Dictionary<HediffComp, float> moodProxies = new Dictionary<HediffComp, float>();

        public static float GetMoodProxy(HediffComp comp)
        {
            if (moodProxies.TryGetValue(comp, out float moodProxy))
            {
                return moodProxy;
            }
            return 6.0f; // Default value
        }

        public static void SetMoodProxy(HediffComp comp, float moodProxy)
        {
            moodProxies[comp] = moodProxy;
        }
    }
}