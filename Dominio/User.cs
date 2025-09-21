using System;

namespace Dominio
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Telefone { get; private set; } = string.Empty; // formato normalizado: +<pais><ddd><numero>
        public string PasswordHash { get; private set; } = string.Empty; // formato: iterations.salt.hash (base64)
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        protected User() { }

        private User(string nome, string email, string telefone)
        {
            Id = Guid.NewGuid();
            SetNome(nome);
            SetEmail(email);
            SetTelefone(telefone);
        }

        public static User Create(string nome, string email, string telefone)
            => new User(nome, email, telefone);

        public void SetNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome obrigatório", nameof(nome));
            Nome = nome.Trim();
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email obrigatório", nameof(email));
            email = email.Trim().ToLowerInvariant();
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Email inválido", nameof(email));
            Email = email;
        }

        public void SetTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone)) throw new ArgumentException("Telefone obrigatório", nameof(telefone));
            // Remove espaços, parênteses, hífens
            var digits = System.Text.RegularExpressions.Regex.Replace(telefone, @"[^0-9+]", "");
            // Normaliza para começar com +
            if (!digits.StartsWith("+"))
            {
                // se vier sem +, assumimos que começou com código do país sem + (ex: 55...)
                digits = "+" + digits;
            }
            // Validar comprimento mínimo (+ + país (1-3) + DDD (2) + numero (8+)) => >= 12 chars
            if (digits.Length < 12)
                throw new ArgumentException("Telefone em formato inválido. Use +<pais><DDD><numero>");
            Telefone = digits;
        }

        public void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Hash inválido", nameof(passwordHash));
            PasswordHash = passwordHash;
        }
    }
}
