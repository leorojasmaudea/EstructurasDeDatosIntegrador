namespace EstructurasDeDatosIntegrador.Storage
{
    internal class User
    {
        public long Cc { get; }
        public string Name { get; }
        public string Email { get; }

        public User(long cc, string name, string email)
        {
            Cc = cc;
            Name = name;
            Email = email;
        }
    }
}
