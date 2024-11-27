using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cgmail.Common.Model;
public class SecureRequest
{
    public string EncryptedData { get; set; } = string.Empty;
    public string Hmac { get; set; } = string.Empty;
    public string Iv { get; set; } = string.Empty;
}