namespace CATAN_WebAssembly.Pages.Partida
{
    public enum Asignacion_Numeros_Catan { Modo_Clasico, Modo_Random, Modo_Insano }
    public class Parametros_Iniciales
    {
        public const int CantidadJugadores = 4;
        public Asignacion_Numeros_Catan Asignacion_Numeros { get; set; } = Asignacion_Numeros_Catan.Modo_Clasico;
        public int Seed { get; set; } = 543456789;

        private List<string> _nombreJugadores = Enumerable.Repeat("", CantidadJugadores).ToList();

        /// <summary>
        /// Siempre tiene exactamente CantidadJugadores elementos:
        /// si llegan menos se completan con "", si llegan más se descartan.
        /// </summary>
        public List<string> Nombre_Jugadores
        {
            get => _nombreJugadores;
            set => _nombreJugadores = Enumerable.Range(0, CantidadJugadores)
                .Select(i => value is not null && i < value.Count ? (value[i] ?? "") : "")
                .ToList();
        }
    }
}
