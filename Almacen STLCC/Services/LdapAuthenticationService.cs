using Novell.Directory.Ldap;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Services
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Rol { get; set; }
    }

    public class LdapAuthenticationService
    {
        private readonly string _ldapServer;
        private readonly int _ldapPort;
        private readonly string _ldapBaseDn;
        private readonly string _ldapDomain;
        private readonly ApplicationDbContext _dbContext;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public LdapAuthenticationService(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            var ldapPath = configuration["LDAP:Path"] ?? "";
            _ldapDomain = configuration["LDAP:Domain"] ?? "stlcc.local";

            ParseLdapPath(ldapPath, out _ldapServer, out _ldapPort, out _ldapBaseDn);

            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public ValidationResult ValidateUserDetailed(string username, string password)
        {
            var ldapResult = ValidateLdapUser(username, password);

            if (ldapResult.IsValid)
                return ldapResult;

            if (!string.IsNullOrEmpty(ldapResult.ErrorMessage) &&
                ldapResult.ErrorMessage != "Usuario o contraseña incorrectos")
            {
                return ldapResult;
            }

            var localResult = ValidateLocalUser(username, password);

            if (localResult.IsValid)
                return localResult;

            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Usuario o contraseña incorrectos"
            };
        }

        private ValidationResult ValidateLdapUser(string username, string password)
        {
            if (string.IsNullOrEmpty(_ldapServer) || string.IsNullOrEmpty(password))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Usuario o contraseña incorrectos"
                };
            }

            try
            {
                using var connection = new LdapConnection();
                connection.Connect(_ldapServer, _ldapPort);

                string[] userFormats =
                [
                    $"{username}@{_ldapDomain}",
                        $"{_ldapDomain}\\{username}",
                        $"CN={username},CN=Users,{_ldapBaseDn}",
                        username
                ];

                bool authenticated = false;
                foreach (var userDn in userFormats)
                {
                    try
                    {
                        connection.Bind(userDn, password);
                        if (connection.Bound)
                        {
                            authenticated = true;
                            break;
                        }
                    }
                    catch (LdapException)
                    {
                        continue;
                    }
                }

                if (!authenticated)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Usuario o contraseña incorrectos"
                    };
                }

                // Verificar grupo
                var groupCheck = IsUserInGroup(connection, username);

                if (!groupCheck.IsValid)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Acceso denegado: Su usuario no tiene permisos para acceder al sistema de almacen STLCC. Póngase en contacto con el administrador del sistema."
                    };
                }

                return new ValidationResult
                {
                    IsValid = true,
                    DisplayName = groupCheck.DisplayName ?? username,
                    Rol = "USUARIO"
                };
            }
            catch
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Usuario o contraseña incorrectos"
                };
            }
        }

        private ValidationResult IsUserInGroup(LdapConnection connection, string username)
        {
            try
            {
                string searchFilter = $"(sAMAccountName={username})";

                var searchResults = connection.Search(
                    _ldapBaseDn,
                    LdapConnection.ScopeSub,
                    searchFilter,
                    ["memberOf", "cn"],
                    false
                );

                if (!searchResults.HasMore())
                    return new ValidationResult { IsValid = false };

                var userEntry = searchResults.Next();

                string displayName = username;
                var cnAttribute = userEntry.GetAttribute("cn");
                if (cnAttribute != null)
                {
                    displayName = cnAttribute.StringValue;
                }

                var memberOfAttribute = userEntry.GetAttribute("memberOf");

                if (memberOfAttribute == null)
                    return new ValidationResult { IsValid = false };

                var groups = memberOfAttribute.StringValueArray;

                bool isInAlmacenGroup = groups.Any(g =>
                    g.Contains("CN=GRUPO_ALMACEN", StringComparison.OrdinalIgnoreCase));

                if (isInAlmacenGroup)
                {
                    return new ValidationResult
                    {
                        IsValid = true,
                        DisplayName = displayName
                    };
                }

                return new ValidationResult { IsValid = false };
            }
            catch
            {
                return new ValidationResult { IsValid = false };
            }
        }

        private ValidationResult ValidateLocalUser(string username, string password)
        {
            try
            {
                var user = _dbContext.Usuarios
                    .AsNoTracking()
                    .FirstOrDefault(u => u.NombreUsuario == username);

                if (user == null)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Usuario o contraseña incorrectos"
                    };
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.Contraseña, password);

                if (result == PasswordVerificationResult.Success)
                {
                    return new ValidationResult
                    {
                        IsValid = true,
                        DisplayName = user.NombreUsuario,
                        Rol = user.Rol
                    };
                }

                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Usuario o contraseña incorrectos"
                };
            }
            catch
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Usuario o contraseña incorrectos"
                };
            }
        }

        public Usuario? GetLocalUser(string username)
        {
            return _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefault(u => u.NombreUsuario == username);
        }

        public bool RegisterLocalUser(string username, string password)
        {
            if (_dbContext.Usuarios.Any(u => u.NombreUsuario == username))
                return false;

            var user = new Usuario
            {
                NombreUsuario = username,
                Contraseña = _passwordHasher.HashPassword(null!, password),
                Rol = "USUARIO"
            };

            _dbContext.Usuarios.Add(user);
            _dbContext.SaveChanges();
            return true;
        }

        private static void ParseLdapPath(string ldapPath, out string server, out int port, out string baseDn)
        {
            server = "";
            port = 389;
            baseDn = "";

            if (string.IsNullOrEmpty(ldapPath))
                return;

            try
            {
                var path = ldapPath.Replace("LDAP://", "").Replace("LDAPS://", "");

                if (ldapPath.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase))
                    port = 636;

                var parts = path.Split('/');

                if (parts.Length > 0)
                {
                    var serverPart = parts[0];
                    if (serverPart.Contains(':'))
                    {
                        var serverPortParts = serverPart.Split(':');
                        server = serverPortParts[0];
                        if (int.TryParse(serverPortParts[1], out int customPort))
                            port = customPort;
                    }
                    else
                    {
                        server = serverPart;
                    }
                }

                if (parts.Length > 1)
                    baseDn = parts[1];
            }
            catch
            {
            }
        }
    }
}