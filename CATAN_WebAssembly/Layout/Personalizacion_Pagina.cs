namespace CATAN_WebAssembly.Layout
{
    public class Personalizacion_Pagina
    {

        private double valor_gapratio = 0.15;

        /// <summary>
        /// propiedad que cambia el espacion entre casillas, pero visualmente se entiende como que controla el tamaño de las casillas
        /// </summary>
        public double Valor_Gapratio
        {
            get { return valor_gapratio; } //se le agrega 1, porque el slider va de -1 a 0
            set
            {
                if (valor_gapratio != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                   valor_gapratio = value;
                   NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private double valor_sizeScale = 0.7;

        /// <summary>
        /// propiedad que cambia el escalado del tablero
        /// </summary>

        public double Valor_SizeScale
        {
            get { return valor_sizeScale; }
            set
            {
                if (valor_sizeScale != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    valor_sizeScale = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }
        
        private bool dark_mode = false;

        public bool Dark_Mode
        {
            get { return dark_mode; }
            set
            {
                if (dark_mode != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    dark_mode = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private double offsetX = 0;

        public double OffsetX
        {
            get { return offsetX; }
            set
            {
                if (offsetX != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    offsetX = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private double offsetY = 0;

        public double OffsetY
        {
            get { return offsetY; }
            set
            {
                if (offsetY != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    offsetY = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }
        private double overlayScale = 0.9;

        public double OverlayScale
        {
            get { return overlayScale; }
            set
            {
                if (overlayScale != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    overlayScale = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        public event Action? OnChange;
        /// <summary>
        /// Metodo que notifica que algún valor cambio en los parametros de configuracion del tablero
        /// </summary>

        private void NotifyStateChanged()
        {
            OnChange?.Invoke(); 
        }
    }
}
