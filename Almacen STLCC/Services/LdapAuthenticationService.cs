using System.DirectoryServices.AccountManagement;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Services
{
    public class LdapAuthenticationService
    {
        private readonly string _ldapDomain;
        private readonly ApplicationDbContext _dbContext;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public LdapAuthenticationService(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _ldapDomain = configuration["LDAP:Domain"];
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public bool ValidateUser(string username, string password)
        {
            if (ValidateLdapUser(username, password))
                return true;

            return ValidateLocalUser(username, password);
        }

        private bool ValidateLdapUser(string username, string password)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _ldapDomain))
                {
                    return context.ValidateCredentials(username, password);
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateLocalUser(string username, string password)
        {
            var user = _dbContext.Usuarios.AsNoTracking().FirstOrDefault(u => u.NombreUsuario == username);
            if (user == null)
                return false;

            var result = _passwordHasher.VerifyHashedPassword(user, user.Contraseña, password);
            return result == PasswordVerificationResult.Success;
        }

        public bool RegisterLocalUser(string username, string password)
        {
            if (_dbContext.Usuarios.Any(u => u.NombreUsuario == username))
                return false;

            var user = new Usuario
            {
                NombreUsuario = username,
                Contraseña = _passwordHasher.HashPassword(null, password)
            };

            _dbContext.Usuarios.Add(user);
            _dbContext.SaveChanges();
            return true;
        }

        public bool IsUserInAlmacenGroup(string username)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _ldapDomain))
                {
                    var user = UserPrincipal.FindByIdentity(context, username);
                    var group = GroupPrincipal.FindByIdentity(context, "GRUPO_ALMACEN");
                    return user != null && group != null && user.IsMemberOf(group);
                }
            }
            catch { return false; }
        }
    }
}
