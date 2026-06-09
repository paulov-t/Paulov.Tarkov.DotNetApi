using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers.api.v1
{
    public class SecurityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public class HashPasswordRequest
        {
            public string Password { get; set; }
        }

        public class VerifyPasswordRequest
        {
            public string Password { get; set; }
            public byte[] Hash { get; set; }
        }

        [Route("/api/v1/Security/HashPassword/")]
        [HttpPost]
        public JsonResult HashPassword([FromBody] HashPasswordRequest request)
        {
            var hashedPassword = SecurityController.Hash(request.Password);
            return Json(new { HashedPassword = Encoding.UTF8.GetString(hashedPassword) });
        }

        [Route("/api/v1/Security/VerifyPassword/")]
        [HttpPost]
        public JsonResult VerifyPassword([FromBody] VerifyPasswordRequest request)
        {
            return Json(new { Result = SecurityController.Verify(request.Password, request.Hash) });
        }

        static byte[] GenerateSalt()
        {
            // In a real application, you should use a secure random generator to create a unique salt for each user.
            // TODO: Implement a secure random salt generator.
            return Encoding.UTF8.GetBytes("SecurityController-SaltValue");
        }

        public static byte[] Hash(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var salt = GenerateSalt();
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];

                // Concatenate password and salt
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                // Hash the concatenated password and salt
                byte[] hashedBytes = sha256.ComputeHash(saltedPassword);

                // Concatenate the salt and hashed password for storage
                byte[] hashedPasswordWithSalt = new byte[hashedBytes.Length + salt.Length];
                Buffer.BlockCopy(salt, 0, hashedPasswordWithSalt, 0, salt.Length);
                Buffer.BlockCopy(hashedBytes, 0, hashedPasswordWithSalt, salt.Length, hashedBytes.Length);

                return Encoding.UTF8.GetBytes(Convert.ToBase64String(hashedPasswordWithSalt));
            }
        }

        public static bool Verify(string entered, byte[] hash)
        {
            string storedHashedPassword = Encoding.UTF8.GetString(hash);

            byte[] storedSaltBytes = GenerateSalt();
            string enteredPassword = entered;

            // Convert the stored salt and entered password to byte arrays
            byte[] enteredPasswordBytes = Encoding.UTF8.GetBytes(enteredPassword);

            // Concatenate entered password and stored salt
            byte[] saltedPassword = new byte[enteredPasswordBytes.Length + storedSaltBytes.Length];
            Buffer.BlockCopy(enteredPasswordBytes, 0, saltedPassword, 0, enteredPasswordBytes.Length);
            Buffer.BlockCopy(storedSaltBytes, 0, saltedPassword, enteredPasswordBytes.Length, storedSaltBytes.Length);

            // Hash the concatenated value
            string enteredPasswordHash = Encoding.UTF8.GetString(Hash(enteredPassword));

            // Compare the entered password hash with the stored hash
            return (enteredPasswordHash == storedHashedPassword);
        }
    }
}
