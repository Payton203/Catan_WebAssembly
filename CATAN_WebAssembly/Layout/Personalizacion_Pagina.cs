using CATAN_WebAssembly.Pages.Partida;

namespace CATAN_WebAssembly.Layout
{
    public class Personalizacion_Pagina
    {
        //─────────────────────────────────────────────────────────────────────Switches────────────────────────────────────────────────────────────────────//

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
        //─────────────────────────────────────────────────────────────────────Checkboxes────────────────────────────────────────────────────────────────────//

        private bool player_panel = true;

        public bool Player_Panel
        {
            get { return player_panel; }
            set
            {
                if (player_panel != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    player_panel = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private bool history_panel = true;

        public bool History_Panel
        {
            get { return history_panel; }
            set
            {
                if (history_panel != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    history_panel = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private bool dice_panel = true;

        public bool Dice_Panel
        {
            get { return dice_panel; }
            set
            {
                if (dice_panel != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    dice_panel = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private StructureType structure = StructureType.None;

        public StructureType Structure
        {
            get { return structure; }
            set
            {
                if (structure != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    structure = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private bool has_road;

        public bool Has_Road
        {
            get { return has_road; }
            set
            {
                if (has_road != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    has_road = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        //─────────────────────────────────────────────────────────────────────Sliders────────────────────────────────────────────────────────────────────//
        private double valor_gapratio = 0.15;

        /// <summary>
        /// propiedad, cambia el espacion entre casillas, pero visualmente se entiende como que controla el tamaño de las casillas
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
        /// Atributo, propiedad que cambia el escalado del tablero
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

        /// <summary>
        ///  Atributo, Se usa para corregir la posicion del CatanOverlay y "acoplarlo" al HexGrid
        /// </summary>
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

        /// <summary>
        /// atributo, se usa para centrar pelotudeces en el eje X, por ejemplo los Iconos en los puertos, uso exclusivo de debugeo
        /// </summary>
        private double offsetX_debug = 0.45;

        /// <summary>
        /// propiedad, se usa para centrar pelotudeces en el eje X, por ejemplo los Iconos en los puertos, uso exclusivo de debugeo
        /// </summary>
        public double OffsetX_Debug
        {
            get { return offsetX_debug; }
            set
            {
                if (offsetX_debug != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    offsetX_debug = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }


        /// <summary>
        /// atributo, se usa para corregir la discrepancia causada por el GapRate en el eje Y
        /// </summary>
        private double offsetY = 0;

        /// <summary>
        /// propiedad, se usa para corregir la discrepancia causada por el GapRate en el eje Y
        /// </summary>
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

        /// <summary>
        /// se usa para centrar pelotudeces en el eje Y, por ejemplo los Iconos en los puertos, uso exclusivo de debugeo
        /// </summary>
        private double offsetY_debug = 0.45;

        public double OffsetY_Debug
        {
            get { return offsetY_debug; }
            set
            {
                if (offsetY_debug != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    offsetY_debug = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        /// <summary>
        /// se usa para centrar pelotudeces en el eje Y, por ejemplo los Iconos en los puertos, uso exclusivo de debugeo
        /// </summary>
        private double scale_debug = 0.9;

        public double Scale_Debug
        {
            get { return scale_debug; }
            set
            {
                if (scale_debug != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    scale_debug = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private double angle_debug = 0;

        public double Angle_Debug
        {
            get { return angle_debug; }
            set
            {
                if (angle_debug != value) //si no cambia el valor, no invoca el metodo de cambio
                {
                    angle_debug = value;
                    NotifyStateChanged(); // avisa que algo cambió
                }
            }
        }

        private double overlayScale = 1;

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
