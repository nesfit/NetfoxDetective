using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Queries;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.BL.Facades
{
    public class LoginFacade : FacadeBase
    {
        public Func<VerifyCredetialQuery> VerifyCredentialFactory { get; set; }

        public UserDTO VerifyCredetials(LoginDTO loginData)
        {
            using(var uow = this.UnitOfWorkProvider.Create())
            {
                var q = this.VerifyCredentialFactory();
                q.Username = loginData.Username;
                q.PasswordHash = this.SHA256Hash(loginData.Password);
                return q.Execute().SingleOrDefault();
            }
        }

        public ClaimsIdentity SignIn(LoginDTO loginData)
        {
            var user = this.VerifyCredetials(loginData);
            if (user != null)
            {
                if(user.IsEnable)
                {
                    return new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role.Name),
                    }, "ApplicationCookie");
                }
                throw new UIException("Account is not active!");
            }
            throw new UIException("Invalid username or password!");
        }

        public string SHA256Hash(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
