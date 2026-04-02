namespace AttendanceSystem.Core.DTOs
{
    public sealed class ConfigPresetDto
    {
        public string Clave       { get; init; }
        public string ValorDefault{ get; init; }
        public string TipoDato    { get; init; }
        public string Descripcion { get; init; }
        public string Icono       { get; init; }
    }

    public sealed class ConfiguracionFilaDto
    {
        public int    Id          { get; set; }
        public string Clave       { get; set; }
        public string Valor       { get; set; }
        public string TipoDato    { get; set; }
        public string Descripcion { get; set; }

        public string TipoDatoAmigable => TipoDato switch
        {
            "int"     => "Número",
            "bool"    => "Sí / No",
            "decimal" => "Decimal",
            _         => "Texto"
        };

        public string IconoTipo => TipoDato switch
        {
            "int"     => "Numeric",
            "bool"    => "ToggleSwitch",
            "decimal" => "Decimal",
            _         => "FormatText"
        };

        public string ValorAmigable => TipoDato == "bool"
            ? (ValorBool ? "Sí (activado)" : "No (desactivado)")
            : Valor;

        public bool EsBool => TipoDato == "bool";

        public bool ValorBool => TipoDato == "bool" &&
            (Valor == "true" || Valor == "True" || Valor == "1" || Valor == "si" || Valor == "Sí");
    }
}
