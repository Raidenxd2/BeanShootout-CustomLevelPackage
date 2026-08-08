namespace KillItMyself.Runtime
{
    public static class CommandLineArgs
    {
        public static bool SetLoadLevelLocalBuild;
        public static bool FastLoad;
        public static bool VerboseLoggingEnabled;
#if UNITY_EDITOR || KILLITMYSELF_DEBUG
        public static bool DebugEnabled;
        public static bool UpdateLevelImages;
        public static bool SetLoadBenchmarkScene;
#endif
    }
}