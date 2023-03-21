#if DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;

namespace SimpleWindDirection
{
	public class SimpleWindDirectionSystemDebug : ModSystem
	{
		float frequency = 0.01f;
		int wrapTimes = 3;
		int wrapTimesFrequencyDiv = 1;

		NormalizedSimplexNoise xNoise;
		NormalizedSimplexNoise zNoise;

		ICoreServerAPI sapi;

		public override void StartServerSide(ICoreServerAPI sapi)
		{
			this.sapi = sapi;

			xNoise = NormalizedSimplexNoise.FromDefaultOctaves(2, frequency / wrapTimesFrequencyDiv, 0.5, 26795);
			zNoise = NormalizedSimplexNoise.FromDefaultOctaves(2, frequency / wrapTimesFrequencyDiv, 0.5, 978243);

			sapi.ChatCommands
			.GetOrCreate("swddbg")
			.RequiresPrivilege(Privilege.controlserver)
			.BeginSubCommand("testnoise")
				.HandleWith(CommandTestNoise)
			.EndSubCommand()
			.BeginSubCommand("testbias")
				.HandleWith(CommandTestBias)
			.EndSubCommand()
			.BeginSubCommand("trianglenoise")
				.HandleWith(CommandTriangleNoise)
			.EndSubCommand()
			.BeginSubCommand("trianglebias")
				.HandleWith(CommandTriangleBias)
			.EndSubCommand()
			.BeginSubCommand("wind")
				.RequiresPlayer()
				.HandleWith(CommandWind)
			.EndSubCommand()
			.BeginSubCommand("windmap")
				.RequiresPlayer()
				.HandleWith(CommandWindMap)
			.EndSubCommand()
			.BeginSubCommand("windmaps")
				.RequiresPlayer()
				.HandleWith(CommandWindMapSeries)
			.EndSubCommand();
		}

		private TextCommandResult CommandTestNoise(TextCommandCallingArgs args)
		{
			int size = 1024;
            Bitmap bmp = new Bitmap(size, size);

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    double xValue = (xNoise.Noise(x, 0, z) * (float)wrapTimes) % 1f;
					double zValue = (zNoise.Noise(x, 0, z) * (float)wrapTimes) % 1f;

                    int red = (int)(xValue * 255);
					int green = (int)(zValue * 255);
                    bmp.SetPixel(x, z, Color.FromArgb(255, red, green, 0));
                }
            }

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "noisemap.png");
            bmp.Save(filepath);

			return TextCommandResult.Success($"Noise map saved to {filepath}");
		}

		private TextCommandResult CommandTriangleNoise(TextCommandCallingArgs args)
		{
			int size = 1024;
            Bitmap bmp = new Bitmap(size, size);

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    double xValue = Math.Abs((xNoise.Noise(x, 0, z) * wrapTimes) % 2d - 1);

					/*double xNoiseVal = xNoise.Noise(x, 0, z);
					double xNoiseWrapTimes = xNoiseVal * wrapTimes;
					double xNoiseWrapRemainder = xNoiseWrapTimes % 2d;
					double xNoiseWrapMinus = xNoiseWrapRemainder - 1d;
					double xValue = Math.Abs(xNoiseWrapMinus);*/

					double zValue = Math.Abs((zNoise.Noise(x, 0, z) * wrapTimes) % 2d - 1);

					/*double zNoiseVal = xNoise.Noise(x, 0, z);
					double zNoiseWrapTimes = zNoiseVal * wrapTimes;
					double zNoiseWrapRemainder = zNoiseWrapTimes % 2d;
					double zNoiseWrapMinus = zNoiseWrapRemainder - 1d;
					double zValue = Math.Abs(zNoiseWrapMinus);*/

                    int red = (int)(xValue * 255);
					int green = (int)(zValue * 255);
                    bmp.SetPixel(x, z, Color.FromArgb(255, red, green, 0));
                }
            }

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trianglemap.png");
            bmp.Save(filepath);

			return TextCommandResult.Success($"Triangle noise map saved to {filepath}");
		}

		private TextCommandResult CommandTestBias(TextCommandCallingArgs args)
		{
			int size = 4096;

			int biasSteps = 100;
			int[] biasVals = new int[biasSteps];

            int underflows = 0;
            int overflows = 0;
            float min = float.MaxValue;
            float max = float.MinValue;
			double average = 0f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double value = xNoise.Noise((double)x, 0, (double)y);
                    if (value < 0)
                    {
                        underflows++;
                        value = 0;
                    }
                    if (value > 1)
                    {
                        overflows++;
                        value = 1;
                    }

					average += value;

                    min = Math.Min((float)value, min);
                    max = Math.Max((float)value, max);

                    biasVals[(int)((value * biasSteps * wrapTimes) % biasSteps)]++;
                }
            }

			average /= (size * size);

			List<string> entries = new List<string>();

			entries.Add($"{String.Join(",", biasVals)};");

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "noisebias.csv");
            File.WriteAllText(filepath, string.Join("\r\n", entries));

			return TextCommandResult.Success($"Noise saved to {filepath}. Min: {min} Max: {max} Average: {average} Uf: {underflows}, Of: {overflows}");
		}

		private TextCommandResult CommandTriangleBias(TextCommandCallingArgs args)
		{
			int size = 4096;

			int biasSteps = 100;
			int[] biasVals = new int[biasSteps];

            int underflows = 0;
            int overflows = 0;
            float min = float.MaxValue;
            float max = float.MinValue;
			double average = 0f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
					double value = Math.Abs((xNoise.Noise(x, 0, y) * wrapTimes) % 2d - 1);
                    if (value < 0)
                    {
                        underflows++;
                        value = 0;
                    }
                    if (value > 1)
                    {
                        overflows++;
                        value = 1;
                    }

					average += value;

                    min = Math.Min((float)value, min);
                    max = Math.Max((float)value, max);

                    biasVals[(int)((value * biasSteps * wrapTimes) % biasSteps)]++;
                }
            }

			average /= (size * size);

			List<string> entries = new List<string>();

			entries.Add($"{String.Join(",", biasVals)};");

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trianglebias.csv");
            File.WriteAllText(filepath, string.Join("\r\n", entries));

			return TextCommandResult.Success($"Noise saved to {filepath}. Min: {min} Max: {max} Average: {average} Uf: {underflows}, Of: {overflows}");
		}

		private TextCommandResult CommandWind(TextCommandCallingArgs args)
		{
			Vec3d wind = sapi.World.BlockAccessor.GetWindSpeedAt(args.Caller.Player.Entity.SidedPos.AsBlockPos);
			Vec3d windNormal = wind.Clone().Normalize();

			return TextCommandResult.Success($"Current wind: X={windNormal.X:0.#####}, Y={windNormal.Y:0.#####}, Z={windNormal.Z:0.#####}, strength={wind.Length()}");
		}

		private TextCommandResult CommandWindMap(TextCommandCallingArgs args)
		{
			int size = 1024;
            Bitmap bmp = new Bitmap(size, size);

			BlockPos playerPos = args.Caller.Player.Entity.SidedPos.AsBlockPos;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vec3d windspeed = sapi.World.BlockAccessor.GetWindSpeedAt(new Vec3d(x + playerPos.X - (size / 2), sapi.World.SeaLevel, y + playerPos.Z - (size / 2)));
					windspeed.Normalize();

					double direction = Math.Atan2(windspeed.X, windspeed.Z) + GameMath.PI;

					int color = ColorUtil.HsvToRgb((int)(direction * 255f / (GameMath.PI * 2f)), 255, 255);

                    bmp.SetPixel(x, y, Color.FromArgb(255, ColorUtil.ColorR(color), ColorUtil.ColorG(color), ColorUtil.ColorB(color)));
                }
            }

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "winddir.png");
            bmp.Save(filepath);

			return TextCommandResult.Success($"Noise saved to {filepath}");
		}

		private TextCommandResult CommandWindMapSeries(TextCommandCallingArgs args)
		{
			int mapCount = 8;
			int hoursPerMap = 6;
			int size = 512;

			BlockPos playerPos = args.Caller.Player.Entity.SidedPos.AsBlockPos;

			for (int mapI = 0; mapI < mapCount; mapI++)
			{
				Bitmap bmp = new Bitmap(size, size);

				for (int y = 0; y < size; y++)
				{
					for (int x = 0; x < size; x++)
					{
						Vec3d windspeed = sapi.World.BlockAccessor.GetWindSpeedAt(new Vec3d(x + playerPos.X - (size / 2), sapi.World.SeaLevel, y + playerPos.Z - (size / 2)));
						windspeed.Normalize();

						double direction = Math.Atan2(windspeed.X, windspeed.Z) + GameMath.PI;

						int color = ColorUtil.HsvToRgb((int)(direction * 255f / (GameMath.PI * 2f)), 255, 255);

						bmp.SetPixel(x, y, Color.FromArgb(255, ColorUtil.ColorR(color), ColorUtil.ColorG(color), ColorUtil.ColorB(color)));
					}
				}

				// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
				string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"winddir_{mapI}.png");
				bmp.Save(filepath);

				sapi.World.Calendar.Add(hoursPerMap);
			}

			return TextCommandResult.Success($"Windmaps saved");
		}
	}
}

#endif