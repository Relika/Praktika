
namespace CommonLibary.Poco
{
    public class SettingsBase
    {
        public bool IsValueExist { get; set; } // kui on eelnevalt eksisteeriv v''rtyus olemas
        public bool IsValueNew { get; set; } // kui uus v''rtus on olemas
        public bool RbNewValue { get; set; }
        public bool RbExistingValue { get; set; }


    }
}
