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

		float timeScaleConstant = 25;

		NormalizedSimplexNoise xNoise;
		NormalizedSimplexNoise zNoise;

		ICoreServerAPI sapi;
		ICoreClientAPI capi;

		SimpleWindDirectionConfigSystem configSystem;

		public override void Start(ICoreAPI api)
		{
			xNoise = NormalizedSimplexNoise.FromDefaultOctaves(4, frequency / wrapTimes, 0.5, 26795);
			zNoise = NormalizedSimplexNoise.FromDefaultOctaves(4, frequency / wrapTimes, 0.5, 978243);

			if (api is ICoreServerAPI) sapi = api as ICoreServerAPI;
			if (api is ICoreClientAPI) capi = api as ICoreClientAPI;

			configSystem = api.ModLoader.GetModSystem<SimpleWindDirectionConfigSystem>();

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
			
			float areaScale = configSystem.SWDConfig.WindAreaScale;
			float timeScale = configSystem.SWDConfig.WindTimeScale;

			windSpeed.X = Math.Abs((xNoise.Noise(pos.X / areaScale, api.World.Calendar.TotalHours * timeScaleConstant / timeScale, pos.Z / areaScale) * wrapTimes) % 2d - 1) * 2d - 1d;
			windSpeed.Z = Math.Abs((zNoise.Noise(pos.X / areaScale, api.World.Calendar.TotalHours * timeScaleConstant / timeScale, pos.Z / areaScale) * wrapTimes) % 2d - 1) * 2d - 1d;
			
			windSpeed.Normalize();
			windSpeed *= windMagnitude;
        }

		public override void Dispose()
		{
			configSystem = null;
		}
	}
}