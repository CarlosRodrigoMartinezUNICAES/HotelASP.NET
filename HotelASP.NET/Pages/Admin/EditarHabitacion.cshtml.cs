using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using HotelASP.NET.Models;

namespace HotelASP.NET.Pages.Admin
{
    public class EditarHabitacionModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public EditarHabitacionModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public HabitacionModel Habitacion { get; set; }

        public List<SelectListItem> TiposHabitacion { get; set; } = new();
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await CargarTiposHabitacion();
            await CargarHabitacion(id);
            
            if (Habitacion == null)
            {
                return RedirectToPage("/Admin/VerHabitacion");
            }
            
            return Page();
        }

        private async Task CargarTiposHabitacion()
        {
            string connectionString = _configuration.GetConnectionString("Login_HotelConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                string sql = "SELECT IdTipoHabitacion, NombreTipo FROM TiposHabitacion";

                using var command = new SqlCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    TiposHabitacion.Add(new SelectListItem
                    {
                        Value = reader["IdTipoHabitacion"].ToString(),
                        Text = reader["NombreTipo"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Message = $"Error al cargar tipos: {ex.Message}";
                IsSuccess = false;
            }
        }

        private async Task CargarHabitacion(int id)
        {
            string connectionString = _configuration.GetConnectionString("Login_HotelConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                string sql = "SELECT * FROM Habitaciones WHERE IdHabitacion = @Id";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    Habitacion = new HabitacionModel
                    {
                        IdHabitacion = reader.GetInt32(0),
                        NumeroHabitacion = reader.GetString(1),
                        IdTipoHabitacion = reader.GetInt32(2),
                        Estado = reader.GetString(3),
                        Piso = reader.GetInt32(4),
                        Detalles = reader.IsDBNull(5) ? "" : reader.GetString(5)
                    };
                }
            }
            catch (Exception ex)
            {
                Message = $"Error al cargar habitación: {ex.Message}";
                IsSuccess = false;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarTiposHabitacion();
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("Login_HotelConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Verificar si el número de habitación ya existe (excluyendo la actual)
                string checkSql = @"
                    SELECT COUNT(*) 
                    FROM Habitaciones 
                    WHERE NumeroHabitacion = @Numero 
                    AND IdHabitacion != @Id";

                using var checkCmd = new SqlCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@Numero", Habitacion.NumeroHabitacion);
                checkCmd.Parameters.AddWithValue("@Id", Habitacion.IdHabitacion);
                
                int exists = (int)await checkCmd.ExecuteScalarAsync();
                if (exists > 0)
                {
                    Message = "Error: Ya existe otra habitación con este número";
                    IsSuccess = false;
                    await CargarTiposHabitacion();
                    return Page();
                }

                // Actualizar la habitación
                string updateSql = @"
                    UPDATE Habitaciones 
                    SET 
                        NumeroHabitacion = @Numero, 
                        IdTipoHabitacion = @TipoId, 
                        Estado = @Estado, 
                        Piso = @Piso, 
                        Detalles = @Detalles,
                        FechaUltimaActualizacion = GETDATE()
                    WHERE IdHabitacion = @Id";

                using var command = new SqlCommand(updateSql, connection);
                command.Parameters.AddWithValue("@Numero", Habitacion.NumeroHabitacion);
                command.Parameters.AddWithValue("@TipoId", Habitacion.IdTipoHabitacion);
                command.Parameters.AddWithValue("@Estado", Habitacion.Estado);
                command.Parameters.AddWithValue("@Piso", Habitacion.Piso);
                command.Parameters.AddWithValue("@Detalles", Habitacion.Detalles ?? "");
                command.Parameters.AddWithValue("@Id", Habitacion.IdHabitacion);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    return RedirectToPage("/Admin/VerHabitacion", new { success = true, message = "Habitación actualizada correctamente" });
                }
                
                Message = "No se encontró la habitación para actualizar";
                IsSuccess = false;
            }
            catch (Exception ex)
            {
                Message = $"Error al actualizar: {ex.Message}";
                IsSuccess = false;
            }
            
            await CargarTiposHabitacion();
            return Page();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}