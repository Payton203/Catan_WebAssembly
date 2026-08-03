namespace CATAN_WebAssembly.Pages.Partida
{
    public enum ResourceType { Wood, Brick, Sheep, Wheat, Rock }
    public class Player
    {
        public int[] Recursos = new int[Enum.GetValues<ResourceType>().Length];
        //public int[] Resources = new int{0,0,0,0,0};
    }
}
