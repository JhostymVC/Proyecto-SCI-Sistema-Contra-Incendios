using System;
using SistemaContraIncendios.Core;

namespace SistemaContraIncendios.Prototipo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SCI - Prototipo de Interfaz";
            Console.WriteLine("SCI - Prototipo de Sistema Contra Incendios\n");
            Console.WriteLine("Este proyecto es un prototipo visual y simplificado del sistema.\n");

            // Crear sensores de demo
            var s1 = new Sensor(1);
            var s2 = new Sensor(2);
            var s3 = new Sensor(3);

            s1.TemperaturaActual = 22.0;
            s2.TemperaturaActual = 30.5;
            s3.TemperaturaActual = 75.5; // forzar crítica para demo

            MostrarSensor(s1);
            MostrarSensor(s2);
            MostrarSensor(s3);

            Console.WriteLine("\nPrototipo: presione cualquier tecla para simular activación de aspersores en el piso crítico...");
            Console.ReadKey();

            Console.WriteLine("Activando aspersores en Piso 3...");
            s3.EnfriarPiso();
            MostrarSensor(s3);

            Console.WriteLine("\nFin de la demo. Presione cualquier tecla para salir.");
            Console.ReadKey();
        }

        static void MostrarSensor(Sensor s)
        {
            Console.WriteLine($"Piso {s.Piso}: {s.TemperaturaActual:0.0}°C - Estado: {s.ObtenerEstado()}");
        }
    }
}
