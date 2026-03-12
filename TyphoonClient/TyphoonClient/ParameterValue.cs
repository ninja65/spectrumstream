using Waters.Control.Message;

namespace Waters.Control.Client.Interface
{
    /// <summary>
    /// ParameterValue
    /// </summary>
    public class ParameterValue
    {
        public string Name { get; set; }
        public VariantValue Value { get; set; }
    }
}
