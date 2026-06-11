using System;

namespace SistemaContraIncendios.Core
{
    public class Sensor
    {
        // Random compartido — evita patrones raros si se crean sensores muy juntos
        private static Random _globalRnd = new Random();
        // Modo que hace las lecturas tengan mas probabilidad de subir
        public static bool ModoMonitoreoAgressivo { get; set; } = false;
        public int Piso { get; set; }
        public double TemperaturaActual { get; set; }
        public Sensor(int piso)
        {
            Piso = piso;
            TemperaturaActual = 22.0; // Temperatura inicial ambiente uniforme
        }
        public void ActualizarTemperatura()
        {
            double cambio;
            if (ModoMonitoreoAgressivo)
            {
                // Sesgo hacia aumentos: rango aproximado -1.0 .. +5.0
                cambio = (_globalRnd.NextDouble() * 6 - 1);
                // Pequeña probabilidad de un pico súbito para simular eventos reales
                if (_globalRnd.NextDouble() < 0.1)
                {
                    cambio += _globalRnd.NextDouble() * 10; // hasta +10°C extra en un salto
                }
            }
            else
            {
                // Oscilación estándar -2 .. +2
                cambio = (_globalRnd.NextDouble() * 4 - 2);
            }

            TemperaturaActual = Math.Round(TemperaturaActual + cambio, 1);

            if (TemperaturaActual < 15) TemperaturaActual = 15; // Límite mínimo lógico
        }
        public void ForzarIncendio()
        {
            TemperaturaActual = 75.5; // Salta directo al umbral crítico
        }

        public void EnfriarPiso()
        {
            TemperaturaActual = 24.0; // Restablece el valor post-mitigación
        }
        public string ObtenerEstado()
        {
            // Evaluar el estado usando la temperatura redondeada a entero
            int tempEntera = (int)Math.Round(TemperaturaActual);
            if (tempEntera <= 38) return "NORMAL";
            if (tempEntera <= 56) return "ELEVADA";
            return "CRÍTICA";
        }
        public void MostrarEstadoEnConsola()
        {
            string estado = ObtenerEstado();

            // Partes estáticas en color fijo
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(string.Format("  {0,-7}| ", "Piso " + Piso));

            // Color dinámico según estado para el número (temperatura)
            if (estado == "NORMAL") Console.ForegroundColor = ConsoleColor.Green;
            else if (estado == "ELEVADA") Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.Red;

            Console.Write(string.Format("{0,9:0.0}°C", TemperaturaActual));

            // Separador estático
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" | ");

            // Estado (coloreado) — añadir etiqueta de peligro cuando aplique
            if (estado == "CRÍTICA")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(estado + " (PELIGRO)");
            }
            else
            {
                if (estado == "NORMAL") Console.ForegroundColor = ConsoleColor.Green;
                else Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(estado);
            }
            Console.ResetColor();
        }
    }
}
