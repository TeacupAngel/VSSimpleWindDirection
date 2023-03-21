#if DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;

namespace SimpleWindDirection
{
	public class SimpleWindDirectionSystem : ModSystem
	{
		float frequency = 0.005f;
		int wrapTimes = 3;

		int timeScale = 25;

		NormalizedSimplexNoise xNoise;
		NormalizedSimplexNoise zNoise;

		ICoreServerAPI sapi;
		ICoreClientAPI capi;

		public override void Start(ICoreAPI api)
		{
			xNoise = NormalizedSimplexNoise.FromDefaultOctaves(4, frequency / wrapTimes, 0.5, 26795);
			zNoise = NormalizedSimplexNoise.FromDefaultOctaves(4, frequency / wrapTimes, 0.5, 978243);

			if (api is ICoreServerAPI) sapi = api as ICoreServerAPI;
			if (api is ICoreClientAPI) capi = api as ICoreClientAPI;

			api.Event.OnGetWindSpeed += (api is ICoreServerAPI) ? ServerEvent_OnGetWindSpeed : ClientEvent_OnGetWindSpeed;
        }

        private void ServerEvent_OnGetWindSpeed(Vec3d pos, ref Vec3d windSpeed)
        {
            OnGetWindSpeed(sapi, pos, ref windSpeed);
        }

		private void ClientEvent_OnGetWindSpeed(Vec3d pos, ref Vec3d windSpeed)
        {
            OnGetWindSpeed(capi, pos, ref windSpeed);
        }

		private void OnGetWindSpeed(ICoreAPI api, Vec3d pos, ref Vec3d windSpeed)
        {
            double windMagnitude = windSpeed.X;

			/*double direction = (testNoise.Noise(pos.X, 0, pos.Z) * wrapTimes) % wrapTimes * GameMath.PI * 2.0;

			windSpeed.X = windMagnitude * GameMath.Sin(direction);
			windSpeed.Z = windMagnitude * GameMath.Cos(direction);*/

			//windSpeed.X = ((xNoise.Noise(pos.X, api.World.Calendar.TotalHours, pos.Z) * (float)wrapTimes) % 1f) * 2f - 1f;
			//windSpeed.Z = ((zNoise.Noise(pos.X, api.World.Calendar.TotalHours, pos.Z) * (float)wrapTimes) % 1f) * 2f - 1f;
			
			windSpeed.X = Math.Abs((xNoise.Noise(pos.X, api.World.Calendar.TotalHours * timeScale, pos.Z) * wrapTimes) % 2d - 1) * 2d - 1d;
			windSpeed.Z = Math.Abs((zNoise.Noise(pos.X, api.World.Calendar.TotalHours * timeScale, pos.Z) * wrapTimes) % 2d - 1) * 2d - 1d;
			
			windSpeed.Normalize();
			windSpeed *= windMagnitude;
        }
	}
}

#endif