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

		public UserModel(string deviceId)
		{
			if (string.IsNullOrEmpty(deviceId)) return;
			DeviceId = deviceId;
		}

		public static UserModel FromUserClaims(ClaimsPrincipal user)
		{
			var result = new UserModel(null);
			if (!user.HasClaim(c => c.Type == "DeviceId")) return null;
			result.DeviceId = user.Claims.First(c => c.Type == "DeviceId").Value;

			return result;
		}

		public Claim[] ToClaims()
		{
			return new[]
			{
				new Claim("DeviceId" , DeviceId)
			};
		}
	}
}
