using CATAN_WebAssembly.Pages.Partida.Panel_Tablero;

namespace CATAN_WebAssembly.Services
{
    public class PartidaService
    {
        public event Action? OnChange; // igual que Personalizacion_Pagina, para que los componentes se re-rendericen

        /// <returns>true si el evento era válido y se aplicó</returns>
        public bool Aplicar(HistorialEvento evento)
        {
            switch (evento.Tipo)
            {
                case TipoEvento.Construccion_Estructura:
                    if (evento.Id_Posicion_construccion is not int id) return false;
                    if (evento.Estructura_Vertice is not StructureType tipo) return false;

                    var vertice = Vertices.ElementAtOrDefault(id);
                    if (vertice is null) return false;
                    // acá las reglas: poblado solo en vértice vacío, ciudad solo sobre un poblado del mismo jugador, etc.

                    vertice.Estructura = tipo;
                    vertice.Jugador = evento.Jugador;
                    break;

                case TipoEvento.Construccion_Carretera:
                    // lo mismo con la arista
                    break;

                case TipoEvento.Mover_Ladron:
                    // mover el ladrón al hexágono indicado
                    break;

                default:
                    break; // eventos que no tocan el tablero
            }

            OnChange?.Invoke();
            return true;
        }
    }
}
