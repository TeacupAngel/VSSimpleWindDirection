using System;

using TeaLib.TeaConfig;

namespace SimpleWindDirection
{
	public class SimpleWindDirectionServerConfig : TeaConfigBase
	{
		public override EnumTeaConfigApiSide ConfigType => EnumTeaConfigApiSide.Server;


		[TeaConfigSettingFloat(Category = "general", Min = 0.0001f, Max = 1000f)]
		public float WindAreaScale {get; set;} = 1f;

		[TeaConfigSettingFloat(Category = "general", Min = 0.0001f, Max = 1000f)]
		public float WindTimeScale {get; set;} = 1f;
	}
}