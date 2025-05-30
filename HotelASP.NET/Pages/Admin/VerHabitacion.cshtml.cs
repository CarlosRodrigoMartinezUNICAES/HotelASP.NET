using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using HotelASP.NET.Models;

namespace HotelASP.NET.Pages.Admin
{
    public class VerHabitacionModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public VerHabitacionModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<HabitacionViewModel> Habitaciones { get; set; } = new();
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public async Task OnGetAsync(string search = "")
        {
            await CargarHabitaciones(search);
        }

        private async Task CargarHabitaciones(string searchTerm = "")
        {
            Habitaciones = new List<HabitacionViewModel>();
            string connectionString = _configuration.GetConnectionString("Login_HotelConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                string sql = @"
                    SELECT 
                        h.IdHabitacion, 
                        h.NumeroHabitacion, 
                        t.NombreTipo AS TipoHabitacion,  
                        h.Estado, 
                        h.Piso
                    FROM Habitaciones h
                    INNER JOIN TiposHabitacion t ON h.IdTipoHabitacion = t.IdTipoHabitacion
                    WHERE @SearchTerm = '' OR 
                          h.NumeroHabitacion LIKE '%' + @SearchTerm + '%' OR 
                          t.NombreTipo LIKE '%' + @SearchTerm + '%'
                    ORDER BY h.Piso, h.NumeroHabitacion";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@SearchTerm", string.IsNullOrEmpty(searchTerm) ? "" : searchTerm);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Habitaciones.Add(new HabitacionViewModel
                    {
                        IdHabitacion = reader.GetInt32(0),
                        NumeroHabitacion = reader.GetString(1),
                        TipoHabitacion = reader.GetString(2),
                        Estado = reader.GetString(3),
                        Piso = reader.GetInt32(4)
                    });
                }
            }
            catch (Exception ex)
            {
                Message = $"Error al cargar habitaciones: {ex.Message}";
                IsSuccess = false;
            }
        }

        public async Task<IActionResult> OnPostEliminar(int id)
        {
            string connectionString = _configuration.GetConnectionString("Login_HotelConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Verificar reservas asociadas
                string checkSql = "SELECT COUNT(*) FROM Reservas WHERE IdHabitacion = @IdHabitacion";
                using var checkCmd = new SqlCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@IdHabitacion", id);
                int reservationCount = (int)await checkCmd.ExecuteScalarAsync();

                if (reservationCount > 0)
                {
                    Message = "No se puede eliminar: Tiene reservas asociadas";
                    IsSuccess = false;
                    await CargarHabitaciones();
                    return Page();
                }

                // Eliminar habitación
                string deleteSql = "DELETE FROM Habitaciones WHERE IdHabitacion = @IdHabitacion";
                using var command = new SqlCommand(deleteSql, connection);
                command.Parameters.AddWithValue("@IdHabitacion", id);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                Message = rowsAffected > 0
                    ? "¡Habitación eliminada con éxito!"
                    : "No se encontró la habitación";
                IsSuccess = rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Message = $"Error al eliminar: {ex.Message}";
                IsSuccess = false;
            }

            await CargarHabitaciones();
            return Page();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }

        public class HabitacionViewModel
        {
            public int IdHabitacion { get; set; }
            public string NumeroHabitacion { get; set; } = string.Empty;
            public string TipoHabitacion { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public int Piso { get; set; }
        }
    }
}