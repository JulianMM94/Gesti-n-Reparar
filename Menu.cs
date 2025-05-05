using System;
using System.Collections.Generic;
using System.Linq;

#region Clases de Empleados y Registros

#region Metodo Empleado
public abstract class Empleado
{
    public uint Legajo { get; }
    public string Apellidos { get; set; }
    public string Nombres { get; set; }
    public float Sueldo { get; set; }

    public Empleado(uint legajo, string apellidos, string nombres, float sueldo)
    {
        Legajo = legajo;
        Apellidos = apellidos;
        Nombres = nombres;
        Sueldo = sueldo;
    }

    public override string ToString()
    {
        return $"{Legajo}: {Apellidos}, {Nombres}, {Sueldo} - Tipo: {GetType().Name}";
    }
}
#endregion

#region Metodo Obrero : Empleado
public class Obrero : Empleado
{
    public string Oficio { get; set; }
    public string Categoria { get; set; }
    public decimal HaberMensual { get; private set; }

    public Obrero(uint legajo, string apellidos, string nombres, float sueldo, string oficio, string categoria)
        : base(legajo, apellidos, nombres, sueldo)
    {
        Oficio = oficio;
        Categoria = categoria;
        HaberMensual = (decimal)sueldo; // El sueldo ya viene calculado externamente.
    }

    public override string ToString()
    {
        return $"\n{Legajo}: {Apellidos}, {Nombres} \n- {Categoria} de {Oficio} \n- Sueldo: ${HaberMensual}";
    }
}
#endregion

#region Metodo Profesional : Empleado
public class Profesional : Empleado
{
    private const float MONTO_REFERENCIA = 10000f; // Sueldo base de un obrero oficial
    private const float CANON_MATRICULA = 2000f;   // Canon fijo por matrícula

    public string TituloHabilitante { get; set; }
    public ulong NumeroMatricula { get; set; }
    public string ConsejoProfesional { get; set; }
    public float PorcentajeAumento { get; set; }
    public float HaberMensual { get; private set; }

    public Profesional(uint legajo, string apellidos, string nombres, float porcentajeAumento,
                       string tituloHabilitante, ulong numeroMatricula, string consejoProfesional)
        : base(legajo, apellidos, nombres, CalcularHaberMensual(porcentajeAumento, 0)) // Inicialmente sin obras asignadas
    {
        TituloHabilitante = tituloHabilitante;
        NumeroMatricula = numeroMatricula;
        ConsejoProfesional = consejoProfesional;
        PorcentajeAumento = porcentajeAumento;
    }

    public static float CalcularHaberMensual(float porcentajeAumento, int obrasSupervisadas)
    {
        float sueldoBase = MONTO_REFERENCIA; // 🔹 Ahora siempre parte de $10,000
        // Aplicamos el porcentaje de aumento sobre el monto de referencia
        sueldoBase *= (1 + (porcentajeAumento / 100));
        // Si supervisa al menos una obra, se suma el canon de matrícula
        if (obrasSupervisadas > 0)
        {
            sueldoBase += CANON_MATRICULA;
        }
        return sueldoBase;
    }
    public int ContarObrasSupervisadas(List<Obra> obras)
    {
        return obras.Count(o => o.Supervisor == this);
    }

    public void ActualizarHaberMensual(List<Obra> obras)
    {
        HaberMensual = CalcularHaberMensual(PorcentajeAumento, ContarObrasSupervisadas(obras));

        // 🔹 Asegurar que el mínimo sea $10,000
        if (HaberMensual < MONTO_REFERENCIA)
        {
            HaberMensual = MONTO_REFERENCIA;
        }
    }

    public override string ToString()
    {
        return $"{Legajo}: {Apellidos}, {Nombres} - {TituloHabilitante} - Matrícula: {NumeroMatricula} " +
               $"Consejo: {ConsejoProfesional} - Obras supervisadas: {ContarObrasSupervisadas(RegistroObras.obras)} ";
               /*$"- Haber Mensual: ${HaberMensual}"*/
    }
}
#endregion

#region Metodo Registro Empleados
public class RegistroEmpleados
{
    private List<Empleado> empleados = new List<Empleado>();
    public bool AgregarEmpleado(Empleado nuevoEmpleado)
    {
        if (empleados.Any(e => e.Legajo == nuevoEmpleado.Legajo))
        {
            Console.WriteLine($"\nError: El legajo {nuevoEmpleado.Legajo} ya está registrado.\n");
            return false;
        }

        empleados.Add(nuevoEmpleado);
        Console.WriteLine($"\nEmpleado registrado: {nuevoEmpleado}\n");
        return true;
    }
    public void MostrarEmpleados()
    {
        Console.WriteLine("\nLista de empleados registrados (ordenados por apellidos):");

        var empleadosOrdenados = empleados.OrderBy(e => e.Apellidos);
        foreach (Empleado emp in empleadosOrdenados)
        {
            if (emp is Obrero obrero)
            {
                Console.WriteLine($"{obrero.Legajo}: {obrero.Apellidos}, {obrero.Nombres} - {obrero.Categoria} de {obrero.Oficio} - Haber Mensual: ${obrero.HaberMensual}");
            }
            else if (emp is Profesional profesional)
            {
                profesional.ActualizarHaberMensual(RegistroObras.obras); // 🔹 Se actualiza correctamente

                Console.WriteLine($"{profesional.Legajo}: {profesional.Apellidos}, {profesional.Nombres} - {profesional.TituloHabilitante} " +
                                  $"- Obras Supervisadas: {profesional.ContarObrasSupervisadas(RegistroObras.obras)} - Haber Mensual: ${profesional.HaberMensual.ToString("F2")}");
            }
            else
            {
                Console.WriteLine(emp.ToString());
            }
        }
    }
    public List<Profesional> ObtenerProfesionales()
    {
        return empleados.OfType<Profesional>().ToList(); // Filtra solo los profesionales
    }

    public List<Obrero> ObtenerObreros()
    {
        return empleados.OfType<Obrero>().ToList(); // Filtra solo los obreros registrados
    }

    public void EliminarProfesional(RegistroObras registroObras)
    {
        Console.Write("Ingrese el legajo del profesional a eliminar: ");
        uint legajoEliminar;
        while (!uint.TryParse(Console.ReadLine(), out legajoEliminar))
        {
            Console.WriteLine("Legajo inválido. Intente nuevamente:");
        }

        // Buscar profesional en la lista
        Profesional? profesional = empleados.OfType<Profesional>().FirstOrDefault(p => p.Legajo == legajoEliminar);

        if (profesional == null)
        {
            Console.WriteLine($"No se encontró un profesional con el legajo {legajoEliminar}.");
            return;
        }

        // Verificar si supervisa alguna obra
        if (registroObras.ObraSupervisadaPor(profesional))
        {
            Console.WriteLine($"El profesional {profesional.Apellidos}, {profesional.Nombres} NO puede ser eliminado porque está supervisando una obra.");
            return;
        }

        // Eliminar profesional
        empleados.Remove(profesional);
        Console.WriteLine($"El profesional {profesional.Apellidos}, {profesional.Nombres} ha sido eliminado correctamente.");
    }
}
#endregion

#region Metodo Registrar Obrero
public class RegistroObrero
{
    public static Obrero RegistrarObrero()
    {
        // Solicitar legajo, apellidos y nombres (igual que antes)
        Console.Write("\tIngrese legajo (número entero sin signo): ");
        uint legajo;
        while (!uint.TryParse(Console.ReadLine(), out legajo))
            Console.Write("\t\tLegajo inválido, ingrese un número válido: ");

        Console.Write("\tIngrese apellidos: ");
        string apellidos = Console.ReadLine() ?? "Desconocido";

        Console.Write("\tIngrese nombres: ");
        string nombres = Console.ReadLine() ?? "Desconocido";

        // Selección de oficio mediante switch
        Console.WriteLine("\tSeleccione el oficio: ");
        Console.WriteLine("\t\t1. Albañil");
        Console.WriteLine("\t\t2. Pintor");
        Console.WriteLine("\t\t3. Plomero");
        Console.WriteLine("\t\t4. Yesero");
        Console.WriteLine("\t\t5. Electricista");

        int opcionOficio;
        while (!int.TryParse(Console.ReadLine(), out opcionOficio) || opcionOficio < 1 || opcionOficio > 5)
        {
            Console.WriteLine("\t\tOpción inválida. Seleccione una opción válida (1-5):");
        }
        string oficio = opcionOficio switch
        {
            1 => "Albañil",
            2 => "Pintor",
            3 => "Plomero",
            4 => "Yesero",
            5 => "Electricista",
            _ => "Desconocido"
        };

        // Selección de categoría mediante switch
        Console.WriteLine("\tSeleccione la categoría: ");
        Console.WriteLine("\t\t1. Oficial");
        Console.WriteLine("\t\t2. Medio-Oficial");
        Console.WriteLine("\t\t3. Aprendiz");

        int opcionCategoria;
        while (!int.TryParse(Console.ReadLine(), out opcionCategoria) || opcionCategoria < 1 || opcionCategoria > 3)
        {
            Console.WriteLine("\t\tOpción inválida. Seleccione una opción válida (1-3):");
        }
        string categoria = opcionCategoria switch
        {
            1 => "Oficial",
            2 => "Medio-Oficial",
            3 => "Aprendiz",
            _ => "Desconocido"
        };

        // Se calcula el sueldo para el obrero basado en la categoría.
        float sueldo = categoria switch
        {
            "Oficial" => 10000f,
            "Medio-Oficial" => 10000f * 0.65f,
            "Aprendiz" => 10000f * 0.25f,
            _ => 0f // Valor por defecto en caso de error
        };

        return new Obrero(legajo, apellidos, nombres, sueldo, oficio, categoria);
    }
}
#endregion

#region Metodo Registro Profesional
public class RegistroProfesional
{
    public static Profesional RegistrarProfesional()
    {
        Console.Write("\tIngrese legajo (número entero sin signo): ");
        uint legajo;
        while (!uint.TryParse(Console.ReadLine(), out legajo))
            Console.Write("\t\tLegajo inválido, ingrese un número válido: ");

        Console.Write("\tIngrese apellidos: ");
        string apellidos = Console.ReadLine() ?? "Desconocido";

        Console.Write("\tIngrese nombres: ");
        string nombres = Console.ReadLine() ?? "Desconocido";

        // Selección del título habilitante mediante switch
        Console.WriteLine("\tSeleccione el título habilitante:");
        Console.WriteLine("\t\t1. Arquitecto");
        Console.WriteLine("\t\t2. Ingeniero");
        Console.WriteLine("\t\t3. Técnico Constructor");
        Console.WriteLine("\t\t4. Maestro Mayor de Obra");

        int opcionTitulo;
        while (!int.TryParse(Console.ReadLine(), out opcionTitulo) || opcionTitulo < 1 || opcionTitulo > 4)
        {
            Console.WriteLine("\t\tOpción inválida. Seleccione una opción válida (1-4): ");
        }
        string tituloHabilitante = opcionTitulo switch
        {
            1 => "Arquitecto",
            2 => "Ingeniero",
            3 => "Técnico Constructor",
            4 => "Maestro Mayor de Obra",
            _ => "Sin Título" // Esto nunca ocurrirá por la validación anterior.
        };

        Console.Write("\tIngrese el número de matrícula (número entero largo sin signo): ");
        ulong numeroMatricula;
        while (!ulong.TryParse(Console.ReadLine(), out numeroMatricula))
            Console.Write("\t\tMatrícula inválida, ingrese un número válido: ");

        // Selección del consejo habilitante mediante switch
        Console.WriteLine("\tSeleccione el consejo habilitante:");
        Console.WriteLine("\t\t1. Colegio de Arquitectos de la Provincia de Buenos Aires");
        Console.WriteLine("\t\t2. Consejo Profesional de Ingenieria Civil");
        Console.WriteLine("\t\t3. Colegio de Tecnicos Constructores de Argentina");
        Console.WriteLine("\t\t4. Colegio de Ingenieros en Construccion");
        Console.WriteLine("\t\t5. Consejo Profesional de Arquitectura y Urbanismo");
        Console.WriteLine("\t\t6. Colegio de Tecnicos de la Provincia de Buenos Aires");
        Console.WriteLine("\t\t7. Consejo de Ingenieros Estructurales");
        Console.WriteLine("\t\t8. Colegio de Especialistas en Construccion y Obras Civiles");

        int opcionConsejo;
        while (!int.TryParse(Console.ReadLine(), out opcionConsejo) || opcionConsejo < 1 || opcionConsejo > 8)
        {
            Console.WriteLine("Opción inválida. Seleccione una opción válida (1-8): ");
        }
        string consejoHabilitante = opcionConsejo switch
        {
            1 => "Colegio de Arquitectos de la Provincia de Buenos Aires",
            2 => "Consejo Profesional de Ingenieria Civil",
            3 => "Colegio de Tecnicos Constructores de Argentina",
            4 => "Colegio de Ingenieros en Construccion",
            5 => "Consejo Profesional de Arquitectura y Urbanismo",
            6 => "Colegio de Tecnicos de la Provincia de Buenos Aires",
            7 => "Consejo de Ingenieros Estructurales",
            8 => "Colegio de Especialistas en Construccion y Obras Civiles",
            _ => "Sin Título" // Esto nunca ocurrirá por la validación anterior.
        };

        Console.Write("\tIngrese porcentaje de aumento negociado (Ej: 10 para 10%): ");
        float porcentajeAumento;
        while (!float.TryParse(Console.ReadLine(), out porcentajeAumento) || porcentajeAumento < 0)
            Console.Write("\t\tPorcentaje inválido, ingrese un número válido: ");

        return new Profesional(legajo, apellidos, nombres, porcentajeAumento,
                              tituloHabilitante, numeroMatricula, consejoHabilitante);
    }
}
#endregion

#endregion

#region Clases de Obras
public class Obra
{
    public string Codigo { get; }
    public string Direccion { get; set; }
    public Profesional Supervisor { get; private set; }
    public List<Obrero> ObrerosAsignados { get; set; } = new List<Obrero>();

    public Obra(string codigo, string direccion, Profesional supervisor)
    {
        Codigo = codigo;
        Direccion = direccion;
        Supervisor = supervisor;
    }
    public bool AsignarObrero(Obrero obrero)
    {
        if (ObrerosAsignados.Contains(obrero))
        {
            Console.WriteLine($"Error: El obrero {obrero.Apellidos}, {obrero.Nombres} ya está asignado a esta obra.");
            return false;
        }

        ObrerosAsignados.Add(obrero);
        Console.WriteLine($"Obrero {obrero.Apellidos}, {obrero.Nombres} ha sido asignado a la obra {Codigo}.");
        return true;
    }
    public void CambiarSupervisor(Profesional nuevoSupervisor)
    {
        Supervisor = nuevoSupervisor;
        Console.WriteLine($"Supervisor actualizado: {nuevoSupervisor.Apellidos}, {nuevoSupervisor.Nombres}");
    }
    public override string ToString()
    {
        return $"Código: {Codigo} - Dirección: {Direccion} - Supervisor: {Supervisor.Apellidos}, {Supervisor.Nombres}, {Supervisor.TituloHabilitante}" +
               $"\nObreros Asignados: {string.Join(", ", ObrerosAsignados.Select(o => $"{o.Apellidos} {o.Nombres}"))}";
    }
}

    #region Metodo Registro Obras
public class RegistroObras
{
    public static List<Obra> obras { get; private set; } = new List<Obra>();
    private RegistroEmpleados registroEmpleados;

    public RegistroObras(RegistroEmpleados registro)
    {
        registroEmpleados = registro;
    }
    public bool ObreroYaAsignado(Obrero obrero)
    {
        return obras.Any(o => o.ObrerosAsignados.Contains(obrero));
    }
    public void RegistrarObra()
    {
        Console.Write("Ingrese código único alfanumérico de la obra: ");
        string codigo = Console.ReadLine() ?? "Sin Código";

        Console.Write("Ingrese dirección de la obra: ");
        string direccion = Console.ReadLine() ?? "Sin Dirección";

        var profesionales = registroEmpleados.ObtenerProfesionales();
        if (profesionales.Count == 0)
        {
            Console.WriteLine("No hay profesionales registrados para supervisar la obra.");
            return;
        }

        Console.WriteLine("Seleccione un profesional para supervisar la obra:");
        for (int i = 0; i < profesionales.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {profesionales[i].Apellidos}, {profesionales[i].Nombres} - {profesionales[i].TituloHabilitante}");
        }

        int opcion;
        while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > profesionales.Count)
        {
            Console.WriteLine("Opción inválida. Seleccione una opción correcta:");
        }

        Profesional supervisorSeleccionado = profesionales[opcion - 1];

        Obra nuevaObra = new Obra(codigo, direccion, supervisorSeleccionado);
        obras.Add(nuevaObra);

        // 🔹 Actualizamos el haber mensual del profesional
        supervisorSeleccionado.ActualizarHaberMensual(obras);

        Console.WriteLine($"Obra registrada con código {codigo} bajo supervisión de {supervisorSeleccionado.Apellidos}, {supervisorSeleccionado.Nombres}.");
    }
    public bool ObraSupervisadaPor(Profesional profesional)
    {
        return obras.Any(o => o.Supervisor == profesional);
    }
    #endregion

    #region Metodo Modificar Profesional de Obra
    public void ModificarSupervisor()
    {
        Console.Write("Ingrese el código de la obra a modificar: ");
        string codigoObra = Console.ReadLine() ?? "";

        Obra ? obra = obras.FirstOrDefault(o => o.Codigo == codigoObra);
        if (obra == null)
        {
            Console.WriteLine($"No se encontró una obra con el código '{codigoObra}'.");
            return;
        }

        // Obtener lista de profesionales disponibles
        var profesionales = registroEmpleados.ObtenerProfesionales();
        if (profesionales.Count == 0)
        {
            Console.WriteLine("No hay profesionales registrados para supervisar la obra.");
            return;
        }

        Console.WriteLine("Seleccione un nuevo profesional para supervisar la obra:");
        for (int i = 0; i < profesionales.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {profesionales[i].Apellidos}, {profesionales[i].Nombres} - {profesionales[i].TituloHabilitante}");
        }

        int opcion;
        while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > profesionales.Count)
        {
            Console.WriteLine("Opción inválida. Seleccione una opción correcta:");
        }

        Profesional nuevoSupervisor = profesionales[opcion - 1];
        obra.CambiarSupervisor(nuevoSupervisor);

        Console.WriteLine($"El supervisor de la obra '{obra.Codigo}' ha sido actualizado a {nuevoSupervisor.Apellidos}, {nuevoSupervisor.Nombres}.");
    }
    #endregion

    #region Metodo Asignar Obreros a Obras
    public void AsignarObreroAObra()
    {
        Console.Write("Ingrese el código de la obra donde quiere asignar un obrero: ");
        string codigoObra = Console.ReadLine() ?? "";

        Obra? obra = obras.FirstOrDefault(o => o.Codigo == codigoObra);
        if (obra == null)
        {
            Console.WriteLine($"No se encontró una obra con el código '{codigoObra}'.");
            return;
        }

        // Obtener obreros disponibles
        var obrerosDisponibles = registroEmpleados.ObtenerObreros();
        if (obrerosDisponibles.Count == 0)
        {
            Console.WriteLine("No hay obreros disponibles para asignar.");
            return;
        }

        // Filtrar solo obreros que aún NO están asignados en ninguna obra
        var obrerosSinAsignar = obrerosDisponibles.Where(o => !ObreroYaAsignado(o)).ToList();

        if (obrerosSinAsignar.Count == 0)
        {
            Console.WriteLine("Todos los obreros ya están asignados a una obra.");
            return;
        }

        Console.WriteLine("Seleccione un obrero para asignar a la obra:");
        for (int i = 0; i < obrerosSinAsignar.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {obrerosSinAsignar[i].Apellidos}, {obrerosSinAsignar[i].Nombres} - {obrerosSinAsignar[i].Oficio}");
        }

        int opcion;
        while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > obrerosSinAsignar.Count)
        {
            Console.WriteLine("Opción inválida. Seleccione una opción correcta:");
        }

        Obrero obreroSeleccionado = obrerosSinAsignar[opcion - 1];
        obra.AsignarObrero(obreroSeleccionado);
    }
    #endregion

    #region Metodo Mostrar Obras
    public void MostrarObras()
    {
        Console.WriteLine("\nLista de obras registradas:");
        foreach (var obra in obras)
        {
            Console.WriteLine(obra);
        }
    }
    #endregion 
}
#endregion

#region Menú de Opciones
namespace GestionReparar
{
    public class Menu
    {
        // Instanciamos un registro global para almacenar los empleados.
        private RegistroEmpleados registro = new RegistroEmpleados();
        private RegistroObras registroObras;
        public void EliminarProfesional(RegistroObras registroObras)
        {
            registro.EliminarProfesional(registroObras);
        }
        public Menu()
        {
            registroObras = new RegistroObras(registro);
        }
        public void ShowMenu()
        {
            bool continuar = true;
            while (continuar)
            {
                Console.Clear();
                Console.WriteLine("=== Menú de Gestión de Empleados ===");
                Console.WriteLine("1. Registrar Obrero");
                Console.WriteLine("2. Registrar Profesional");
                Console.WriteLine("3. Mostrar empleados");
                Console.WriteLine("4. Registrar Obra");
                Console.WriteLine("5. Mostrar obras");
                Console.WriteLine("6. Modificar supervisor de obra");
                Console.WriteLine("7. Asignar obrero a una obra");
                Console.WriteLine("8. Eliminar profesional de la empresa");
                Console.WriteLine("0. Salir");
                Console.Write("\tSeleccione una opción: ");

                if (!int.TryParse(Console.ReadLine(), out int opcion))
                {
                    Console.WriteLine("\n\tOpción inválida. Intente nuevamente.");
                    Console.WriteLine("\n\tPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                    continue;
                }

                switch (opcion)
                {
                    case 1:
                        Console.WriteLine("\nRegistrando Obrero...");
                        var obrero = RegistroObrero.RegistrarObrero();
                        registro.AgregarEmpleado(obrero);
                        break;
                    case 2:
                        Console.WriteLine("\nRegistrando Profesional...");
                        var profesional = RegistroProfesional.RegistrarProfesional();
                        registro.AgregarEmpleado(profesional);
                        break;
                    case 3:
                        registro.MostrarEmpleados();
                        break;
                    case 4:
                        registroObras.RegistrarObra();
                        break;
                    case 5:
                        registroObras.MostrarObras();
                        break;
                    case 6:
                        registroObras.ModificarSupervisor();
                        break;
                    case 7:
                        registroObras.AsignarObreroAObra();
                        break;
                    case 8:
                        registro.EliminarProfesional(registroObras);
                        break;
                    case 0:
                        Console.WriteLine("\nSaliendo del menú.");
                        continuar = false;
                        break;
                    default:
                        Console.WriteLine("\nOpción no reconocida. Intente nuevamente.");
                        break;
                }

                if (continuar)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }
    }
}

#endregion