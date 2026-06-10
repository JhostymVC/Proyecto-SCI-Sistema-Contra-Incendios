using System;
using System.Collections.Generic;
using System.Threading;
using SistemaContraIncendios.Core;

namespace SistemaContraIncendios.UI
{
    public static class Menus
    {
        public static List<Sensor> sensoresEdificio = new List<Sensor>();
        public static bool monitoreoActivo = false;
        public static List<string> bitacoraEventos = new List<string>();

        public static void Run()
        {
            // crear los 3 sensores (piso 1, 2 y 3)
            sensoresEdificio.Add(new Sensor(1));
            sensoresEdificio.Add(new Sensor(2));
            sensoresEdificio.Add(new Sensor(3));
            RegistrarEvento("Sistema inicializado correctamente.");
            // menú principal
            bool ejecutar = true;
            while (ejecutar)
            {
                Console.Clear();
                MostrarEncabezado("PANEL DE CONTROL DE INCENDIOS", null, ConsoleColor.Cyan);
                Console.WriteLine("                                               ");
                Console.WriteLine(" [1] Ver estado actual");
                Console.WriteLine(" [2] Forzar Temperatura Crítica");
                Console.WriteLine(" [3] Ver Historial de Alertas");
                Console.WriteLine(" [4] Salir del Sistema");
                Console.WriteLine("                                               ");
                Console.WriteLine("--------------------------------------------------");
                Console.Write("Seleccione una opción: ");
                string opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        MenuMonitoreo();
                        break;
                    case "2":
                        // Opción no programada aún
                        break;
                    case "3":
                        // Opción no programada aún
                        break;
                    case "4":
                        ejecutar = false;
                        Console.WriteLine("Apagando panel de control...");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar.");
                        Console.ResetColor();
                        Console.ReadKey();
                        break;
                }
            }
        }
        static void MenuMonitoreo()
        {
            monitoreoActivo = true;
            // Activar modo que aumenta la probabilidad de lecturas altas
            Sensor.ModoMonitoreoAgressivo = true;
            Console.Clear();
            while (monitoreoActivo)
            {
                Console.Clear();
                MostrarEncabezado("MONITOREO DE TEMPERATURA EN VIVO", null, ConsoleColor.DarkCyan);
                List<int> pisosCriticos = new List<int>();
                // Encabezados de columnas estáticas para mantener separación fija
                Console.WriteLine();
                Console.WriteLine("Piso     | Temperatura | Estado   ");
                Console.WriteLine("---------+-------------+----------");

                // Actualizar y mostrar el estado de cada piso (columnas de ancho fijo)
                foreach (var sensor in sensoresEdificio)
                {
                    sensor.ActualizarTemperatura();
                    sensor.MostrarEstadoEnConsola();

                    if (sensor.ObtenerEstado() == "CRÍTICA")
                    {
                        pisosCriticos.Add(sensor.Piso);
                    }
                }

                // Si se detecta al menos un piso en estado CRÍTICA, iniciar protocolo
                if (pisosCriticos.Count > 0)
                {
                    RegistrarEvento($"ALERTA: Temperatura CRÍTICA detectada en Piso(s): {string.Join(",", pisosCriticos)}.");
                    ProtocoloEmergencia(pisosCriticos);
                }

                // Pausa ligeramente aumentada para que las lecturas numéricas tarden un poco más
                Thread.Sleep(800);

                // Detectar si el usuario quiere salir al menú principal
                if (Console.KeyAvailable)
                {
                    var tecla = Console.ReadKey(true);
                    if (tecla.Key == ConsoleKey.M)
                    {
                        monitoreoActivo = false;
                    }
                }
            }

            // Desactivar el modo agresivo al salir del monitoreo
            Sensor.ModoMonitoreoAgressivo = false;
        }
        static void ProtocoloEmergencia(List<int> pisosAfectados)
        {
            bool emergenciaResuelta = false;
            while (!emergenciaResuelta)
            {
                // Emitir sonido de alerta MUY FUERTE: alternar tonos extremos durante varios ciclos
                // Frecuencias altas y bajas para máxima percepción
                for (int _i = 0; _i < 6; _i++)
                {
                    Console.Beep(3200, 200);
                    Thread.Sleep(20);
                    Console.Beep(400, 200);
                    Thread.Sleep(20);
                }
                // Pulso sostenido final intenso, algo más corto
                Console.Beep(3200, 500);

                // Si hay 3 pisos afectados, forzar activación en todos
                if (pisosAfectados.Count == 3)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    MostrarEncabezado("ASPERSORES ACTIVADOS EN TODOS LOS PISOS", null, ConsoleColor.Blue);
                    Console.WriteLine("[ÉXITO]: Extinguiendo fuego...");
                    RegistrarEvento("Aspersores activados en TODOS los pisos debido a múltiples alarmas críticas.");
                    Thread.Sleep(2000);
                    Console.ResetColor();
                    // Enfriar todos los pisos
                    foreach (var s in sensoresEdificio) s.EnfriarPiso();
                    monitoreoActivo = false;
                    return;
                }

                // Mostrar encabezado con los pisos afectados
                Console.Clear();
                MostrarEncabezado($"ALERTA: INCENDIO EN PISO(S) {string.Join(",", pisosAfectados)}", "SISTEMA EN ESTADO DE CRISIS", ConsoleColor.Red);
                Console.WriteLine("                                               ");
                Console.WriteLine(" [1] Activar Aspersores");
                Console.WriteLine(" [2] Evacuación");
                Console.WriteLine(" [3] Llamar a Bomberos");
                Console.WriteLine(" [4] Restablecer Sistema / Silenciar Alarma");
                Console.WriteLine("--------------------------------------------------");
                Console.Write("Seleccione una acción de mitigación urgente: ");

                string opcionEmergencia = Console.ReadLine();

                switch (opcionEmergencia)
                {
                    case "1":
                        // Si son 2 pisos, requerir activación uno a uno
                        if (pisosAfectados.Count == 2)
                        {
                            bool todasActivadas = true;
                            foreach (var piso in pisosAfectados)
                            {
                                // llamar al submenu para cada piso sin detener el monitoreo hasta el final
                                if (!SubmenuAspersores(piso, false))
                                {
                                    todasActivadas = false;
                                    break;
                                }
                            }

                            if (todasActivadas)
                            {
                                RegistrarEvento($"Aspersores activados en pisos {string.Join(",", pisosAfectados)}.");
                                // después de activar ambos, finalizar emergencia
                                monitoreoActivo = false;
                                emergenciaResuelta = true;
                            }
                        }
                        else if (pisosAfectados.Count == 1)
                        {
                            // caso único: usar el flujo normal y detener monitoreo al éxito
                            if (SubmenuAspersores(pisosAfectados[0], true))
                            {
                                emergenciaResuelta = true;
                            }
                        }
                        break;
                    case "2":
                        // Opción no programada aún
                        break;
                    case "3":
                        // Opción no programada aún
                        break;
                    case "4":
                        // Mostrar alerta roja de intento de restablecer durante unos segundos
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[SISTEMA]: Intentando restablecer / silenciar alarma...");
                        // Mantener el texto rojo visible antes de evaluar
                        Thread.Sleep(3000);
                        Console.ResetColor();

                        // Evaluar si es posible restablecer (solo permitido si hay un único piso afectado)
                        int pisoParaRestablecer = pisosAfectados.Count == 1 ? pisosAfectados[0] : -1;
                        if (pisoParaRestablecer != -1)
                        {
                            if (sensoresEdificio[pisoParaRestablecer - 1].TemperaturaActual < 55.0)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\n[SISTEMA]: Temperatura normalizada. Alarma restablecida.");
                                RegistrarEvento("Alarma restablecida manualmente. Sensores estables.");
                                Console.ResetColor();
                                // Mantener el mensaje de éxito visible brevemente y salir del protocolo
                                Thread.Sleep(2000);
                                emergenciaResuelta = true;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("\n[ERROR]: No se puede restablecer. La temperatura sigue en nivel CRÍTICO.");
                                Console.ResetColor();
                                // Mantener el mensaje de error visible unos segundos y luego volver al menú
                                Thread.Sleep(3000);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("\n[ERROR]: Restablecer no permitido cuando hay múltiples pisos en estado CRÍTICA.");
                            Console.ResetColor();
                            Thread.Sleep(3000);
                        }
                        break;
                }
            }
        }

        static bool SubmenuAspersores(int pisoConFuego, bool detenerMonitoreoOnSuccess)
        {
            while (true)
            {
                Console.Clear();
                if (pisoConFuego > 0)
                {
                    MostrarEncabezado($"ALERTA: INCENDIO EN PISO {pisoConFuego}", null, ConsoleColor.Red);
                }
                else
                {
                    MostrarEncabezado("ACTIVACIÓN DE ASPERSORES", null, ConsoleColor.Cyan);
                }
                Console.WriteLine("                                               ");
                Console.WriteLine("=== ACTIVACIÓN DE ASPERSORES ===");
                Console.WriteLine(" [1] Activar en Piso 1");
                Console.WriteLine(" [2] Activar en Piso 2");
                Console.WriteLine(" [3] Activar en Piso 3");
                Console.WriteLine(" [4] Volver al menú");
                Console.WriteLine("                                               ");
                Console.Write("Seleccione una opción: ");

                int seleccionPiso = 0;
                string opc = Console.ReadLine();

                // Aceptar entradas con espacios y validar
                if (int.TryParse((opc ?? string.Empty).Trim(), out int valor))
                {
                    if (valor >= 1 && valor <= 3) seleccionPiso = valor;
                    else if (valor == 4) return false; // Volver al menú
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nEntrada no válida.");
                    Console.ResetColor();
                    Thread.Sleep(2000);
                    continue;
                }

                // Si estamos en modo emergencia, validar que sea el piso correcto
                if (pisoConFuego > 0 && seleccionPiso != pisoConFuego)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[RESTRICCIÓN]: Activación denegada en Piso {seleccionPiso}.");
                    Console.WriteLine($"El sensor reporta estado NORMAL. Solo permitido en el piso afectado ({pisoConFuego}).");
                    Console.ResetColor();
                    Thread.Sleep(3000);
                    continue;
                }

                // Activar aspersores
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"\n[ÉXITO]: Aspersores ACTIVADOS en el Piso {seleccionPiso}. Extinguiendo fuego...");
                RegistrarEvento($"Aspersores activados con éxito en el Piso {seleccionPiso}.");
                Thread.Sleep(2000);
                Console.ResetColor();
                
                // Enfriar el piso si estamos en modo emergencia
                if (pisoConFuego > 0)
                {
                    sensoresEdificio[seleccionPiso - 1].EnfriarPiso();
                    if (detenerMonitoreoOnSuccess)
                    {
                        monitoreoActivo = false;
                    }
                }
                
                return true;
            }
        }

        public static void RegistrarEvento(string mensaje)
        {
            bitacoraEventos.Add($"[{DateTime.Now.ToString("HH:mm:ss")}] {mensaje}");
        }

        public static void MostrarEncabezado(string titulo, string subtitulo, ConsoleColor color)
        {
            Console.Clear();
            var ancho = 50;
            var borde = new string('=', ancho);
            Console.ForegroundColor = color;
            Console.WriteLine(borde);
            Console.WriteLine($"   {titulo.PadLeft((ancho + titulo.Length) / 2).PadRight(ancho)}");
            if (!string.IsNullOrEmpty(subtitulo))
            {
                Console.WriteLine($"   {subtitulo.PadLeft((ancho + subtitulo.Length) / 2).PadRight(ancho)}");
            }
            Console.WriteLine(borde);
            Console.ResetColor();
        }
    }
}
