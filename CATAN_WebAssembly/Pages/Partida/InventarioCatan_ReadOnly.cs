namespace CATAN_WebAssembly.Pages.Partida
{
    // Un tipo de recurso: su clave interna (el mismo ResourceType que usás en
    // Jugador.Recursos), el icono que se muestra, y el nombre completo (para el tooltip).
    public record RecursoDefinicion(ResourceType Clave, string Nombre);

    public record CartasDesarrolloDefinicion(TiposCartasDesarrollo Clave, string Nombre);
    public static class InventarioCatan_ReadOnly
    {
        // Para agregar, quitar o reordenar un recurso, tocás ÚNICAMENTE esta lista
        // (y el enum ResourceType en Jugador.cs si es un recurso nuevo).
        // El panel de jugadores la recorre automáticamente, así que no hay que tocar
        // nada más en el componente.
        //
        // El icono es texto (emoji) por ahora. Si más adelante querés usar tus SVG
        // propios de recursos, cambiás "Icono" por la ruta del archivo (ej: "img/madera.svg")
        // y en JugadoresPanel.razor reemplazás el <span> del icono por un <img src="@recurso.Icono" />.
        public static readonly List<RecursoDefinicion> Recursos =
        [
            new(ResourceType.Wood, "Madera"),
            new(ResourceType.Brick, "Ladrillo"),
            new(ResourceType.Sheep, "Oveja"),
            new(ResourceType.Wheat, "Trigo"),
            new(ResourceType.Rock, "Mineral"),
        ];
        public static readonly List<CartasDesarrolloDefinicion> Cartas_Desarrollo =
        [
            new(TiposCartasDesarrollo.Caballero, "Madera"),
            new(TiposCartasDesarrollo.Carreteras, "Ladrillo"),
            new(TiposCartasDesarrollo.Invento, "Oveja"),
            new(TiposCartasDesarrollo.Monopolio, "Trigo"),
            new(TiposCartasDesarrollo.Punto_Victoria, "Mineral"),
        ];
    }
}
