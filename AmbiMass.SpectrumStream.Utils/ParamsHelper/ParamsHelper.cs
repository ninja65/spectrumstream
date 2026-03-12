namespace AmbiMass.SpectrumStream.Utils.ParamsHelper
{
    public static class ParamsHelper
    {
        public static T? asObject<T>(this object[] args) where T : class
        {
            bool l = false;
            int i = -1;

            while (!l && i < args.Length - 1)
            {
                l = args[i + 1] is T;
                i++; ;
            }

            return l ? args[i] as T : null;
        }

        public static T? asObjectAt<T>(this object[] args, int at) where T : class
        {
            bool l = false;
            int i = -1;

            while (!l && i < args.Length - 1)
            {
                l = args[i + 1] is T && i + 1 == at;
                i++; ;
            }

            return l ? args[i] as T : null;
        }

        public static T? asPrimitive<T>(this object[] args, T? defaultValue) where T : struct
        {
            bool l = false;
            int i = -1;

            while (!l && i < args.Length - 1)
            {
                l = args[i + 1].GetType().IsPrimitive && args[i + 1].GetType() == typeof(T);
                i++; ;
            }

            return l ? (T)args[i] : defaultValue;
        }

        public static T asPrimitiveAt<T>(this object[] args, int at, T defaultValue) where T : class
        {
            bool l = false;
            int i = -1;

            while (!l && i < args.Length - 1)
            {
                l = args[i + 1] is T && i + 1 == at && args[i + 1].GetType().IsPrimitive && args[i + 1].GetType() == typeof(T);
                i++; ;
            }

            return l ? (T)args[i] : defaultValue;
        }
    }
}
