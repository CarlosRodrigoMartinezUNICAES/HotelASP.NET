using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

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
            public string Username { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            public string Password { get; set; }
        }

        public void OnGet()
        {
            // Si hay un mensaje en TempData, lo mostramos y luego lo limpiamos
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

            // Verificar credenciales en la base de datos
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Login_HotelConnection")))
                {
                    connection.Open();
                    string sql = "SELECT IdUsuario, NombreUsuario, RolUsuario FROM Usuarios WHERE NombreUsuario = @Username AND Contraseña = @Password";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Username", Input.Username);
                        command.Parameters.AddWithValue("@Password", Input.Password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Usuario autenticado correctamente
                                int userId = reader.GetInt32(0);
                                string username = reader.GetString(1);
                                string role = reader.GetString(2);

                                // Guardar información del usuario en la sesión
                                HttpContext.Session.SetInt32("UserId", userId);
                                HttpContext.Session.SetString("Username", username);
                                HttpContext.Session.SetString("UserRole", role);

                                // Redirigir según el rol
                                if (role == "admin")
                                {
                                    return RedirectToPage("/Admin/Dashboard");
                                }
                                else
                                {
                                    return RedirectToPage("/Index");
                                }
                            }
                            else
                            {
                                // Usuario o contraseña incorrectos
                                ErrorMessage = "Usuario o contraseña incorrectos. Por favor, inténtelo de nuevo.";
                                return Page();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al iniciar sesión: " + ex.Message;
                return Page();
            }
        }
    }
