using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
namespace HotelASP.NET.Pages.Cliente;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _config;

    public List<HabitacionViewModel> HabitacionesDisponibles { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public IActionResult OnGet()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(role))
        {
            return RedirectToPage("/Login");
        }

        HabitacionesDisponibles = ObtenerHabitacionesDisponibles();
        return Page();
    }

    private List<HabitacionViewModel> ObtenerHabitacionesDisponibles()
    {
        var habitaciones = new List<HabitacionViewModel>();
        var connectionString = _config.GetConnectionString("Login_HotelConnection");

        using (var conn = new SqlConnection(connectionString))
        {
            conn.Open();
            var query = @"
                SELECT H.IdHabitacion, H.NumeroHabitacion, H.Piso, H.Detalles, T.NombreTipo, T.CapacidadMaxima, T.PrecioBase
                FROM dbo.Habitaciones H
                JOIN dbo.TiposHabitacion T ON H.IdTipoHabitacion = T.IdTipoHabitacion
                WHERE H.Estado = 'disponible'";

            using (var cmd = new SqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    habitaciones.Add(new HabitacionViewModel
                    {
                        Id = reader.GetInt32(0),
                        Numero = reader.GetString(1),
                        Piso = reader.GetInt32(2),
                        Detalles = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Tipo = reader.GetString(4),
                        Capacidad = reader.GetInt32(5),
                        Precio = reader.GetDecimal(6)
                    });
                }
            }
        }

        return habitaciones;
    }

    public class HabitacionViewModel
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public int Piso { get; set; }
        public string Detalles { get; set; }
        public string Tipo { get; set; }
        public int Capacidad { get; set; }
        public decimal Precio { get; set; }
    }
}
