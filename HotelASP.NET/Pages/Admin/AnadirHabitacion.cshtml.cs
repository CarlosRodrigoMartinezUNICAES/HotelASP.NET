using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using HotelASP.NET.Models;

namespace HotelASP.NET.Pages.Admin
{
    public class AnadirHabitacionModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public AnadirHabitacionModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public HabitacionModel Habitacion { get; set; }

        public List<SelectListItem> TiposHabitacion { get; set; } = new List<SelectListItem>();

        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public async Task OnGetAsync()
        {
            await CargarTiposHabitacion();
        }

        private async Task CargarTiposHabitacion()
        {
            string connectionString = _configuration.GetConnectionString("Login_HotelConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string sql = "SELECT IdTipoHabitacion, NombreTipo FROM TiposHabitacion";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TiposHabitacion.Add(new SelectListItem
                            {
                                Value = reader["IdTipoHabitacion"].ToString(),
                                Text = reader["NombreTipo"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error al cargar tipos de habitación: {ex.Message}";
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
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Verificar si ya existe el número de habitación
                    string checkSql = "SELECT COUNT(*) FROM Habitaciones WHERE NumeroHabitacion = @Numero";
                    using (var checkCmd = new SqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Numero", Habitacion.NumeroHabitacion);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists > 0)
                        {
                            Message = "Error: Ya existe una habitación con este número";
                            IsSuccess = false;
                            await CargarTiposHabitacion();
                            return Page();
                        }
                    }

                    // Insertar nueva habitación
                    string insertSql = @"
                        INSERT INTO Habitaciones 
                            (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles) 
                        VALUES 
                            (@NumeroHabitacion, @IdTipoHabitacion, @Estado, @Piso, @Detalles)";

                    using (var command = new SqlCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@NumeroHabitacion", Habitacion.NumeroHabitacion);
                        command.Parameters.AddWithValue("@IdTipoHabitacion", Habitacion.IdTipoHabitacion);
                        command.Parameters.AddWithValue("@Estado", "disponible");
                        command.Parameters.AddWithValue("@Piso", Habitacion.Piso);
                        command.Parameters.AddWithValue("@Detalles", Habitacion.Detalles ?? string.Empty);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                Message = "¡Habitación añadida con éxito!";
                IsSuccess = true;
                return RedirectToPage("/Admin/VerHabitacion");
            }
            catch (Exception ex)
            {
                Message = $"Error al guardar la habitación: {ex.Message}";
                IsSuccess = false;
                await CargarTiposHabitacion();
                return Page();
            }
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}