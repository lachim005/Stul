using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace StulKnihovna
{
    /// <summary>
    /// Třída starající se o ovládání stolu a sériovou komunikaci
    /// </summary>
    public class Stul : IDisposable
    {
        private bool disposed;
        private Thread cteciVlakno;

        /// <summary>
        /// Port pro připojení k arduinu
        /// </summary>
        private SerialPort port;

        /// <summary>
        /// Tabulka všech pixelů podle rozmístění na stole od levého horního rohu
        /// <list type="number">
        ///    <item>souřadnice - šířka</item>
        ///    <item>souřadnice - výška</item>
        /// </list>
        /// </summary>
        private Pixel[,] pixely = new Pixel[sirka, vyska];

        /// <summary>
        /// Tabulka všech pixelů podle interního indexovacího systému deska -> pixel
        /// </summary>
        private Pixel[][] pixelyInterne;


        /// <summary>
        /// Event, který se spustí při detekci magnetu některého z pixelů
        /// </summary>
        public event PixelEventHandler MagnetEvent;
        /// <summary>
        /// Event, který se spustí při změně barvy jakéhokoli pixelu
        /// </summary>
        public event PixelEventHandler ZmenaPixelu;


        #region Konstanty
        /// <summary>
        /// Šířka stolu
        /// </summary>
        public const int sirka = 9;
        /// <summary>
        /// Výška stolu
        /// </summary>
        public const int vyska = 6;
        /// <summary>
        /// Počet desek v interním indexovacím systému
        /// </summary>
        public const int desky = 7;
        /// <summary>
        /// Počet pixelů na desce v interním indexovacím systému
        /// </summary>
        public const int pixelyNaDesce = 8;
        /// <summary>
        /// Počet pixelů na poslední desce v intrením indexovacím systému
        /// </summary>
        public const int pixelyNaPosledniDesce = 6;
        #endregion


        /// <summary>
        /// Vytvoří nový stůl. Může chvíli trvat, než se arduino připojí
        /// </summary>
        /// <param name="portName">Jméno portu</param>
        /// <param name="timeout">Čas (ms), po kterém stůl přestane žkoušet připojování</param>
        public Stul(string portName, int timeout = 10000)
        {
            try
            {
                //Tohle by mohlo vyřešit odpojování kabelu
                port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                port.Open();
                port.Close();
                port.Dispose();
                Thread.Sleep(100);

                
                //Otevře port
                port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                port.Open();
                port.ReadTimeout = timeout;
            }
            catch
            {
                throw new Exception("Piči");
            }

            DateTime start = DateTime.Now;

            bool vraciJine = false;
            try
            {
                //Počká na odpověď arduina
                while (port.ReadByte() != 255)
                {
                    Thread.Sleep(1);
                    if ((DateTime.Now - start).TotalMilliseconds > timeout)
                    {
                        //Spustí se pouze, pokud port vrací neočekávané hodnoty
                        //a asi se nejedná o správný port
                        vraciJine = true;
                        throw new TimeoutException();
                    }
                }

                //Pošle ?kontrolní byte?
                PortNapis(7, 7, 3);
            }
            catch (Exception ex)
            {
                if (!vraciJine)
                {
                    //Pošle ping a pokud se stůl odpoví, úspěšně připojí stůl
                    PortNapis(7, 7, 2);
                    Thread.Sleep(100);
                    if (port.ReadByte() != 254)
                    {
                        throw new TimeoutException();
                    }
                } else
                {
                    throw new TimeoutException();
                }
            }

            VygenerujPixely();
            NastavInterniMapuPixelu();

            port.ReadTimeout = SerialPort.InfiniteTimeout;

            NastavVsechnyPixely(StavPixelu.Zadny);

            cteciVlakno = new Thread(CistMagnety);
            cteciVlakno.Start();

        }

        #region Generování pixelů
        /// <summary>
        /// Vygeneruje včechny pixely se správnými parametry
        /// </summary>
        private void VygenerujPixely()
        {
            //Automaticky vygenerováno z dat z originálního projektu
            pixely[5, 1] = new Pixel(this, 0, 0, 5, 1);
            pixely[5, 0] = new Pixel(this, 0, 1, 5, 0);
            pixely[4, 0] = new Pixel(this, 0, 2, 4, 0);
            pixely[3, 0] = new Pixel(this, 0, 3, 3, 0);
            pixely[4, 1] = new Pixel(this, 0, 4, 4, 1);
            pixely[3, 1] = new Pixel(this, 0, 5, 3, 1);
            pixely[3, 2] = new Pixel(this, 0, 6, 3, 2);
            pixely[4, 2] = new Pixel(this, 0, 7, 4, 2);
            pixely[1, 3] = new Pixel(this, 1, 0, 1, 3);
            pixely[0, 3] = new Pixel(this, 1, 1, 0, 3);
            pixely[0, 5] = new Pixel(this, 1, 2, 0, 5);
            pixely[0, 4] = new Pixel(this, 1, 3, 0, 4);
            pixely[1, 4] = new Pixel(this, 1, 4, 1, 4);
            pixely[1, 5] = new Pixel(this, 1, 5, 1, 5);
            pixely[2, 5] = new Pixel(this, 1, 6, 2, 5);
            pixely[2, 4] = new Pixel(this, 1, 7, 2, 4);
            pixely[2, 1] = new Pixel(this, 2, 0, 2, 1);
            pixely[2, 0] = new Pixel(this, 2, 1, 2, 0);
            pixely[1, 0] = new Pixel(this, 2, 2, 1, 0);
            pixely[0, 0] = new Pixel(this, 2, 3, 0, 0);
            pixely[1, 1] = new Pixel(this, 2, 4, 1, 1);
            pixely[0, 1] = new Pixel(this, 2, 5, 0, 1);
            pixely[0, 2] = new Pixel(this, 2, 6, 0, 2);
            pixely[1, 2] = new Pixel(this, 2, 7, 1, 2);
            pixely[4, 3] = new Pixel(this, 3, 0, 4, 3);
            pixely[3, 3] = new Pixel(this, 3, 1, 3, 3);
            pixely[3, 4] = new Pixel(this, 3, 2, 3, 4);
            pixely[4, 4] = new Pixel(this, 3, 3, 4, 4);
            pixely[3, 5] = new Pixel(this, 3, 4, 3, 5);
            pixely[4, 5] = new Pixel(this, 3, 5, 4, 5);
            pixely[5, 5] = new Pixel(this, 3, 6, 5, 5);
            pixely[5, 4] = new Pixel(this, 3, 7, 5, 4);
            pixely[7, 2] = new Pixel(this, 4, 0, 7, 2);
            pixely[8, 2] = new Pixel(this, 4, 1, 8, 2);
            pixely[8, 1] = new Pixel(this, 4, 2, 8, 1);
            pixely[7, 1] = new Pixel(this, 4, 3, 7, 1);
            pixely[8, 0] = new Pixel(this, 4, 4, 8, 0);
            pixely[7, 0] = new Pixel(this, 4, 5, 7, 0);
            pixely[6, 0] = new Pixel(this, 4, 6, 6, 0);
            pixely[6, 1] = new Pixel(this, 4, 7, 6, 1);
            pixely[6, 4] = new Pixel(this, 5, 0, 6, 4);
            pixely[6, 5] = new Pixel(this, 5, 1, 6, 5);
            pixely[7, 5] = new Pixel(this, 5, 2, 7, 5);
            pixely[8, 5] = new Pixel(this, 5, 3, 8, 5);
            pixely[7, 4] = new Pixel(this, 5, 4, 7, 4);
            pixely[8, 3] = new Pixel(this, 5, 5, 8, 3);
            pixely[8, 4] = new Pixel(this, 5, 6, 8, 4);
            pixely[7, 3] = new Pixel(this, 5, 7, 7, 3);
            pixely[5, 3] = new Pixel(this, 6, 0, 5, 3);
            pixely[2, 3] = new Pixel(this, 6, 1, 2, 3);
            pixely[2, 2] = new Pixel(this, 6, 2, 2, 2);
            pixely[6, 3] = new Pixel(this, 6, 3, 6, 3);
            pixely[6, 2] = new Pixel(this, 6, 4, 6, 2);
            pixely[5, 2] = new Pixel(this, 6, 5, 5, 2);

        }

        /// <summary>
        /// Vytvoří a naplní pole polí <see cref="pixelyInterne"/>
        /// </summary>
        private void NastavInterniMapuPixelu()
        {
            //Vygeneruje pole v poli polí
            pixelyInterne = new Pixel[desky][];
            for (int i = 0; i < desky - 1; i++)
            {
                pixelyInterne[i] = new Pixel[pixelyNaDesce];
            }
            pixelyInterne[desky - 1] = new Pixel[pixelyNaPosledniDesce];

            //Naplní pole v poli polí
            for (int x = 0; x < sirka; x++)
            {
                for (int y = 0; y < vyska; y++)
                {
                    Pixel akt = pixely[x, y];
                    pixelyInterne[akt.Deska][akt.PixelNaDesce] = akt;
                }
            }
        }
        #endregion

        #region Gettery
        /// <summary>
        /// Vrátí pixel podle rozmístění na stole od levého horního rohu
        /// </summary>
        /// <param name="x">Šířková souřadnice</param>
        /// <param name="y">Výšková souřadnice</param>
        /// <returns>Pixel na daných souřadnicích</returns>
        public Pixel VratPixel(int x, int y)
        {
            return pixely[x, y];
        }

        /// <summary>
        /// Vrátí pixel podle interního značení deska->pixel
        /// </summary>
        /// <param name="x">Index desky</param>
        /// <param name="y">Index pixelu na desce</param>
        /// <returns>Pixel na daných souřadnicích</returns>
        public Pixel VratPixelInterni(int deska, int pixel)
        {
            return pixelyInterne[deska][pixel];
        }

        //Indexer
        /// <summary>
        /// Vrátí pixel podle rozmístění na stole od levého horního rohu
        /// </summary>
        /// <param name="x">Šířková souřadnice</param>
        /// <param name="y">Výšková souřadnice</param>
        /// <returns>Pixel na daných souřadnicích</returns>
        public Pixel this[int x, int y]
        {
            get => pixely[x, y];
        }
        #endregion

        #region Prace s portem
        /// <summary>
        /// Zakóduje a napíše data do portu
        /// </summary>
        internal void PortNapis(int d, int p, int b)
        {
            PortNapis(ZakodovatVystup(d, p, b));
        }
        /// <summary>
        /// Napíše byte do portu
        /// </summary>
        internal void PortNapis(byte b)
        {
            port.Write(new byte[] { b }, 0, 1);
        }

        private void CistMagnety()
        {
            while (!disposed)
            {
                if (port.BytesToRead > 0)
                {
                    byte b = (byte)port.ReadByte();
                    if (PlantyPixel(b))
                    {
                        MagnetEvent?.Invoke(this, new PixelEventArgs(b, this));
                    }
                }

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Zakóduje data pro odeslání do portu
        /// </summary>
        internal static byte ZakodovatVystup(int d, int p, int b)
        {
            return (byte)((d << 5) + (p << 2) + b);
        }

        /// <summary>
        /// Dekóduje data přijatá z portu
        /// </summary>
        /// <param name="b">Příchozí byte</param>
        /// <returns>Tuple obsahující dekódovaná data</returns>
        internal static (int, int, int) DekodovatVstup(byte b)
        {
            (int, int, int) res = (
                (b >> 5) & 0b00000111,
                (b >> 2) & 0b00000111,
                b & 0b00000011);

            return res;
        }

        internal static bool PlantyPixel(byte b)
        {
            (int, int, int) d = DekodovatVstup(b);


            if (d.Item1 >= 0)
            {
                if (d.Item1 < desky - 1)
                {
                    return d.Item2 >= 0 && d.Item2 < pixelyNaDesce && d.Item3 >= 0 && d.Item3 < 4;
                }
                else if (d.Item1 == desky - 1)
                {
                    return d.Item2 >= 0 && d.Item2 < pixelyNaPosledniDesce && d.Item3 >= 0 && d.Item3 < 4;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Nastaví stav všech pixelů na stole
        /// </summary>
        /// <param name="stav">Stav pixelů</param>
        public void NastavVsechnyPixely(StavPixelu stav)
        {
            for (int x = 0; x < sirka; x++)
            {
                for (int y = 0; y < vyska; y++)
                {
                    pixely[x, y]._stav = stav;
                    PixelZmenen(pixely[x, y]);
                }
            }
            PortNapis(7, 0, (int)stav);
        }

        internal void PixelZmenen(Pixel pixel)
        {
            ZmenaPixelu?.Invoke(this, new PixelEventArgs(pixel));
        }
        #region Implementace IDisposable
        public void Dispose()
        {
            disposed = true;
            cteciVlakno.Join();

            port.Close();
            port.Dispose();
        }
        #endregion
    }
}
