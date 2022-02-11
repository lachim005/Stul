using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StulKnihovna
{
    /// <summary>
    /// Třída zajišťující chování pixelů
    /// </summary>
    public class Pixel
    {
        private Stul stul;

        /// <summary>
        /// Vrátí nebo nastaví stav pixelu
        /// </summary>
        public StavPixelu Stav
        {
            get => _stav;
            set
            {
                _stav = value;
                stul.PortNapis(Deska, PixelNaDesce, (int)value);
            }
        }

        internal StavPixelu _stav;

        /// <summary>
        /// Šířková souřadnice na stole
        /// </summary>
        public int X { get; internal set; }
        /// <summary>
        /// Výšková souřadnice na stole
        /// </summary>
        public int Y { get; internal set; }
        /// <summary>
        /// Index desky v interním indexovacím systému
        /// </summary>
        public int Deska { get; internal set; }
        /// <summary>
        /// Index pixelu na desce v interním indexovacím systému
        /// </summary>
        public int PixelNaDesce { get; internal set; }

        /// <summary>
        /// Vytvoří novou instanci pixelu
        /// </summary>
        /// <param name="s">Stůl, na kterém se pixel nachází</param>
        /// <param name="deska">Index desky</param>
        /// <param name="pixelNaDesce">Index pixelu na desce</param>
        internal Pixel(Stul s, int deska, int pixelNaDesce, int x, int y)
        {
            stul = s;
            Deska = deska;
            PixelNaDesce = pixelNaDesce;
            X = x;
            Y = y;
        }
    }
}
