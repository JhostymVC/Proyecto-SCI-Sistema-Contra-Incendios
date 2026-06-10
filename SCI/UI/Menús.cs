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
                        ForzarFalloSimulado();
                        break;
                    case "3":
                        MostrarBitacora();
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

                // Si hay 3 pisos afectados, solicitar confirmación manual para activar TODOS los aspersores
                if (pisosAfectados.Count == 3)
                {
                    while (true)
                    {
                        Console.Clear();
                        MostrarEncabezado($"ALERTA MÚLTIPLE: INCENDIO EN TODOS LOS PISOS", "SE REQUIERE ACCIÓN MANUAL", ConsoleColor.Red);
                        Console.WriteLine("                                               ");
                        Console.WriteLine(" [1] Activar aspersores en TODOS los pisos");
                        Console.WriteLine("                                               ");
                        Console.Write("Seleccione la acción (debe seleccionar 1 para proceder): ");

                        string entrada = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(entrada)) continue;
                        if (entrada.Trim() == "1")
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("\n[ÉXITO]: Aspersores ACTIVADOS en TODOS los pisos. Extinguiendo fuego...");
                            RegistrarEvento("Aspersores activados en TODOS los pisos por confirmación manual del operador.");
                            Thread.Sleep(2000);
                            Console.ResetColor();
                            // Enfriar todos los pisos
                            foreach (var s in sensoresEdificio) s.EnfriarPiso();
                            monitoreoActivo = false;
                            return;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Entrada no válida. Debe seleccionar '1' para activar los aspersores en TODOS los pisos.");
                            Console.ResetColor();
                            Thread.Sleep(2000);
                        }
                    }
                }

                // Mostrar encabezado con los pisos afectados
                Console.Clear();
                MostrarEncabezado($"ALERTA: INCENDIO EN PISO(S) {string.Join(",", pisosAfectados)}", "SISTEMA EN ESTADO DE CRISIS", ConsoleColor.Red);
                Console.WriteLine("                                               ");
                Console.WriteLine(" [1] Activar Aspersores");
                Console.WriteLine(" [2] Evacuar Edificio (Alerta General)");
                Console.WriteLine(" [3] Llamar al Cuerpo de Bomberos");
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
                        // Mostrar alerta en rojo y emitir sirenas durante 3s, luego volver al menú
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[SISTEMA]: Activando sirenas de evacuación por voz. Salga en orden.");
                        RegistrarEvento("Protocolo de evacuación general activado.");
                        // Emitir patrón de sirena (alternar tonos altos y bajos)
                        for (int i = 0; i < 6; i++)
                        {
                            Console.Beep(1200, 300);
                            Console.Beep(700, 300);
                        }
                        // Mantener mensaje en rojo visible unos segundos antes de regresar
                        Thread.Sleep(3000);
                        Console.ResetColor();
                        break;
                    case "3":
                        // Mostrar alerta en rojo y simular llamada telefónica, luego volver al menú
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[SISTEMA]: Transmitiendo coordenadas a Bomberos... [OK]");
                        RegistrarEvento("Señal de emergencia enviada a bomberos.");
                        // Simular sonido de teléfono (ring) con pulsos cortos
                        for (int i = 0; i < 3; i++)
                        {
                            Console.Beep(800, 200);
                            Thread.Sleep(250);
                        }
                        // Mantener el mensaje rojo visible unos segundos antes de regresar
                        Thread.Sleep(3000);
                        Console.ResetColor();
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
                MostrarEncabezado($"ALERTA: INCENDIO EN PISO {pisoConFuego}", null, ConsoleColor.Red);
                Console.WriteLine("                                               ");
                Console.WriteLine("=== ACTIVACIÓN DE ASPERSORES ===");
                Console.WriteLine(" [1] Activar en Piso 1");
                Console.WriteLine(" [2] Activar en Piso 2");
                Console.WriteLine(" [3] Activar en Piso 3");
                Console.WriteLine(" [4] Activar en TODOS los pisos");
                Console.WriteLine("                                               ");
                Console.Write("Seleccione una opción: ");

                int seleccionPiso = 0;
                string opc = Console.ReadLine();

                // Aceptar entradas con espacios y validar
                if (int.TryParse((opc ?? string.Empty).Trim(), out int valor))
                {
                    if (valor >= 1 && valor <= 3) seleccionPiso = valor;
                    else if (valor == 4) seleccionPiso = -1; // Código para todos
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nEntrada no válida.");
                    Console.ResetColor();
                    Thread.Sleep(2000);
                    continue; // volver a mostrar el menú
                }

                // Validar restricción crítica de protección por daño de agua
                if (seleccionPiso == -1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[BLOQUEADO]: No se pueden activar TODOS los aspersores.");
                    Console.WriteLine("Razón: Hay pisos seguros. Se causarían daños materiales severos innecesarios.");
                    Console.ResetColor();
                    Thread.Sleep(3000);
                    return false;
                }
                else if (seleccionPiso != pisoConFuego)
                {
                    // Mostrar restricción y regresar automáticamente al menú de selección
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[RESTRICCIÓN]: Activación denegada en Piso {seleccionPiso}.");
                    Console.WriteLine($"El sensor reporta estado NORMAL. Solo permitido en el piso afectado ({pisoConFuego}).");
                    Console.ResetColor();
                    Thread.Sleep(3000);
                    continue; // volver a mostrar el menú para que el usuario reintente
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\n[ÉXITO]: Aspersores ACTIVADOS en el Piso {pisoConFuego}. Extinguiendo fuego...");
                    RegistrarEvento($"Aspersores activados con éxito en el Piso {pisoConFuego}.");
                    // Mantener el mensaje azul visible unos segundos
                    Thread.Sleep(2000);
                    Console.ResetColor();
                    // Simular que el agua enfría el piso afectado para salir de la crisis
                    sensoresEdificio[pisoConFuego - 1].EnfriarPiso();
                    // Detener el monitoreo y regresar al menú principal solo si se solicitó
                    if (detenerMonitoreoOnSuccess)
                    {
                        monitoreoActivo = false;
                    }
                    return true;
                }
            }
        }

        static void ForzarFalloSimulado()
        {
            Console.Clear();
            Console.WriteLine("=== SIMULACIÓN: FORZAR TEMPERATURA CRÍTICA ===");
            Console.WriteLine("                                               ");
            Console.WriteLine("Introduzca el número de piso (1-3) o 'C' para cancelar.");

            while (true)
            {
                Console.Write("Piso (1-3) o C: ");
                string entrada = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(entrada)) continue;
                if (entrada.Trim().Equals("C", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Operación cancelada. Presione una tecla para regresar.");
                    Console.ReadKey();
                    return;
                }

                if (int.TryParse(entrada.Trim(), out int p) && p >= 1 && p <= 3)
                {
                    sensoresEdificio[p - 1].ForzarIncendio();
                    RegistrarEvento($"Simulación: incendio forzado en Piso {p}.");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Piso {p} configurado a 75°C. Inicie el monitoreo para ver la alerta.");
                    Console.ResetColor();

                    // Preguntar si desea forzar otro piso
                    while (true)
                    {
                        Console.Write("¿Desea forzar otro piso? (S/N): ");
                        string resp = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(resp)) continue;
                        resp = resp.Trim().ToUpper();
                        if (resp == "S")
                        {
                            // Volver al inicio del bucle principal para ingresar otro piso
                            break; // rompe el bucle interno y vuelve a solicitar piso
                        }
                        else if (resp == "N")
                        {
                            Console.WriteLine("Regresando al menú principal. Presione una tecla para continuar.");
                            Console.ReadKey();
                            return;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Entrada no válida. Introduzca 'S' o 'N'.");
                            Console.ResetColor();
                        }
                    }
                    // continuar para forzar otro piso
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Entrada no válida. Introduzca 1, 2, 3 o C para cancelar.");
                Console.ResetColor();
            }
        }

        public static void MostrarBitacora()
        {
            Console.Clear();
            MostrarEncabezado("HISTORIAL DE EVENTOS", null, ConsoleColor.Cyan);
            foreach (var evento in bitacoraEventos)
            {
                Console.WriteLine($" - {evento}");
            }
            Console.WriteLine("\nPresione cualquier tecla para regresar.");
            Console.ReadKey();
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
