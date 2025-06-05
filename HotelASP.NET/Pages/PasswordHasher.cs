using System.Security.Cryptography;
using System.Text;

namespace HotelASP.NET.Pages
{
    public class PasswordHasher
    {
        public static string ComputeHash(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                // Combina la contraseña con el salt
                string saltedPassword = password + salt;

                // Calcula el hash
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                // Convierte a string Base64
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
