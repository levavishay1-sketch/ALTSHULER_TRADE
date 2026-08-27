
namespace Alt.DataModel.ExernalServices.ANVIL
{
    public class AnvilGeneralRequest: ExternalEntityBase
    {
        public string title { get; set; }
        public int? fontSize { get; set; }
        public string fontFamily { get; set; }
        public string textColor { get; set; }
        public dynamic data { get; set; }
    }
}
