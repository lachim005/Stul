using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using StulKnihovna;
using ConsolePlus;

namespace SerialTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleP.ReadChoice(SerialPort.GetPortNames(), "Vyberte název portu", out string nazevPortu);

            using (Stul stul = new Stul(nazevPortu))
            {   
                stul.MagnetEvent += Stul_MagnetEvent;

                StavPixelu s = StavPixelu.Cervena;

                for (int a = 0; a < 1; a++)
                {
                    s = (StavPixelu)(a % 3 + 1);

                    string pismeno =
                        "xxx xxx" +
                        "x   x  " +
                        "xxx xxx" +
                        "  x   x" +
                        "xxx xxx";

                    for (int j = 0; j < 5; j++)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            stul[i, j].Stav = (pismeno[j * 7 + i] == ' ') ? StavPixelu.Zadny : s;
                        }
                    }
                    Thread.Sleep(1000);
                }

                Debug.WriteLine(stul[0, 0].Stav);

                Console.ReadKey();
            }

            Console.ReadKey();
        }

        private static void Stul_MagnetEvent(object sender, MagnetEventArgs e)
        {
            e.Pixel.Stav = StavPixelu.Zelena;

            Thread.Sleep(500);
        }
    }
}
