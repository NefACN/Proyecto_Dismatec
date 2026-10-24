using System.Security.Cryptography;

namespace proyect.application.Security
{
    public static class GeneradorPasswords
    {
        public static string Generar()
        {
            const string mayus = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string minus = "abcdefghijkmnpqrstuvwxyz";
            const string digitos = "23456789";
            const string simbolos = "!@#$%&*";
            const string todos = mayus + minus + digitos + simbolos;

            var resultado = new char[12];
            resultado[0] = mayus[RandomNumberGenerator.GetInt32(mayus.Length)];
            resultado[1] = minus[RandomNumberGenerator.GetInt32(minus.Length)];
            resultado[2] = digitos[RandomNumberGenerator.GetInt32(digitos.Length)];
            resultado[3] = simbolos[RandomNumberGenerator.GetInt32(simbolos.Length)];
 
            for (var i = 4; i<resultado.Length; i++)
                resultado[i] = todos[RandomNumberGenerator.GetInt32(todos.Length)];
 
            for (var i = resultado.Length - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
            (resultado[i], resultado[j]) = (resultado[j], resultado[i]);
            }
 
            return new string (resultado);

        }
    }
}
