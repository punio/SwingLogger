using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwingDataViewer.Models
{
	public class UserModel
	{
		public string DeviceId { get; set; }
		public string Name { get; set; }

		public UserModel(string deviceId, string name)
		{
			if (string.IsNullOrEmpty(deviceId)) return;
			DeviceId = deviceId;
			Name = name;
		}

		public static UserModel FromUserClaims(ClaimsPrincipal user)
		{
			var result = new UserModel(null, null);
			if (!user.HasClaim(c => c.Type == "DeviceId")) return null;
			result.DeviceId = user.Claims.First(c => c.Type == "DeviceId").Value;
			result.Name = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

			return result;
		}

		public Claim[] ToClaims()
		{
			return new[]
			{
				new Claim("DeviceId" , DeviceId),
				new Claim(ClaimTypes.Name,Name)
		};
		}
	}
}
