using Orleans.Runtime;

namespace Orleans.Telemetry.ApplicationInsights
{
    public static class GrainExtensions
    {
        public static object GetGrainId(this IAddressable grain)
        {
            switch (grain)
            {
                case IGrainWithStringKey _:
                    return grain.GetPrimaryKeyString();
                case IGrainWithGuidKey _:
                    return grain.GetPrimaryKey();
                case IGrainWithIntegerKey _:
                    return grain.GetPrimaryKeyLong();
                case IGrainWithGuidCompoundKey _:
                    var guidKey = grain.GetPrimaryKey(out var guidExt);
                    return $"{guidKey}.{guidExt}";
                case IGrainWithIntegerCompoundKey _:
                    var intKey = grain.GetPrimaryKeyLong(out var intExt);
                    return $"{intKey}.{intExt}";
                default:
                    return grain.ToString();
            }
        }
    }
}