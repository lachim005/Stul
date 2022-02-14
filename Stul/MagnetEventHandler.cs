using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StulKnihovna
{
    /// <summary>
    /// Delegát pro <seealso cref="Stul.MagnetEvent"/>
    /// </summary>
    public delegate void MagnetEventHandler(object sender, MagnetEventArgs e);

    /// <summary>
    /// Obsahuje data o pixelu, který detekoval magnet
    /// </summary>
    public class MagnetEventArgs : EventArgs
    {
        public Pixel Pixel { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Deska { get; set; }
        public int PixelNaDesce { get; set; }

        internal MagnetEventArgs()
        {
            
        }

        internal MagnetEventArgs(byte b, Stul stul)
        {
            (int, int, int) data = Stul.DekodovatVstup(b);
            Deska = data.Item1;
            PixelNaDesce = data.Item2;
            Pixel = stul.VratPixelInterni(Deska, PixelNaDesce);
            X = Pixel.X;
            X = Pixel.Y;
        }
    }
}
