using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace HotelASP.NET.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
            public string? Username { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            public string? Password { get; set; }
        }

        public void OnGet()
        {
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"].ToString();
                TempData.Remove("SuccessMessage");
            }

            if (TempData["ErrorMessage"] != null)
            {
                ErrorMessage = TempData["ErrorMessage"].ToString();
                TempData.Remove("ErrorMessage");
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(
                    _configuration.GetConnectionString("Login_HotelConnection")))
                {
                    connection.Open();

                    // 1. Primero obtenemos el salt y hash almacenado
                    string getSaltSql = "SELECT Salt, Contraseña FROM Usuarios WHERE NombreUsuario = @Username";

                    string salt = "";
                    string storedHash = "";

                    using (SqlCommand saltCommand = new SqlCommand(getSaltSql, connection))
                    {
                        saltCommand.Parameters.AddWithValue("@Username", Input.Username);

                        using (SqlDataReader reader = saltCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                salt = reader["Salt"].ToString();
                                storedHash = reader["Contraseña"].ToString();
                            }
                            else
                            {
                                ErrorMessage = "Usuario no encontrado";
                                return Page();
                            }
                        }
                    }

                    // 2. Calculamos el hash de la contraseña ingresada
                    string computedHash = PasswordHasher.ComputeHash(Input.Password, salt);
                    Console.WriteLine($"Hash generado: {computedHash}");

                    // 3. Comparamos los hashes
                    if (computedHash != storedHash)
                    {
                        ErrorMessage = "Contraseña incorrecta";
                        return Page();
                    }

                    // 4. Si coincide, obtenemos los datos del usuario
                    string userDataSql = "SELECT IdUsuario, NombreUsuario, RolUsuario FROM Usuarios WHERE NombreUsuario = @Username";

                    using (SqlCommand userCommand = new SqlCommand(userDataSql, connection))
                    {
                        userCommand.Parameters.AddWithValue("@Username", Input.Username);

                        using (SqlDataReader reader = userCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0);
                                string username = reader.GetString(1);
                                string role = reader.GetString(2);

                                // Guardar en sesión
                                HttpContext.Session.SetInt32("UserId", userId);
                                HttpContext.Session.SetString("Username", username);
                                HttpContext.Session.SetString("UserRole", role);

                                // Redirigir según rol
                                if (role == "admin")
                                {
                                    return RedirectToPage("/Admin/Dashboard");
                                }
                                else if (role == "cliente")
                                {
                                    return RedirectToPage("/Cliente/Index");
                                }
                            }
                        }
                    }

                    return RedirectToPage("/Index");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al iniciar sesión: " + ex.Message;
                return Page();
            }
        }
    }
}