namespace CATAN_WebAssembly.Pages.Partida
{
    public enum Asignacion_Numeros_Catan { Modo_Clasico, Modo_Random, Modo_Insano }
    public class Parametros_Iniciales
    {
        public Asignacion_Numeros_Catan Asignacion_Numeros = Asignacion_Numeros_Catan.Modo_Clasico;
        public int Seed { get; set; } = 10;

        public List<string> Nombre_Jugadores { get; set; } = ["", "", "", ""];
    }
}
