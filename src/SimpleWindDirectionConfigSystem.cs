using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;

using TeaLib.TeaConfig;

namespace SimpleWindDirection
{
	public class SimpleWindDirectionConfigSystem : TeaConfigSystemBase
	{
		public SimpleWindDirectionServerConfig SWDConfig => (SimpleWindDirectionServerConfig)ServerConfig;

		public override void LoadConfigs(ICoreAPI api)
		{
			ServerConfig = LoadConfig<SimpleWindDirectionServerConfig>(api);
		}
	}
}