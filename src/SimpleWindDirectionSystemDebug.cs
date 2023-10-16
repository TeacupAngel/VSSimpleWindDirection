#if DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;
using SkiaSharp;

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
				.WithArgs(
					sapi.ChatCommands.Parsers.OptionalFloat("hoursBetween"),
					sapi.ChatCommands.Parsers.OptionalInt("mapCount")
				)
			.EndSubCommand();
		}

		private TextCommandResult CommandTestNoise(TextCommandCallingArgs args)
		{
			int size = 1024;
			BitmapExternal bitmap = new BitmapExternal(size, size);

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    double xValue = (xNoise.Noise(x, 0, z) * (float)wrapTimes) % 1f;
					double zValue = (zNoise.Noise(x, 0, z) * (float)wrapTimes) % 1f;

                    byte red = (byte)(xValue * 255);
					byte green = (byte)(zValue * 255);
					bitmap.bmp.SetPixel(x, z, new SKColor(255, red, green));
                }
            }

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x_swd_noisetest.png");
			//string filepath = "swd_noisetest.png";

			bitmap.Save(filepath);

			return TextCommandResult.Success($"Noise map saved to {filepath}");
		}

		private TextCommandResult CommandTriangleNoise(TextCommandCallingArgs args)
		{
			int size = 1024;
            BitmapExternal bitmap = new BitmapExternal(size, size);

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    double xValue = Math.Abs((xNoise.Noise(x, 0, z) * wrapTimes) % 2d - 1);

					//double xNoiseVal = xNoise.Noise(x, 0, z);
					//double xNoiseWrapTimes = xNoiseVal * wrapTimes;
					//double xNoiseWrapRemainder = xNoiseWrapTimes % 2d;
					//double xNoiseWrapMinus = xNoiseWrapRemainder - 1d;
					//double xValue = Math.Abs(xNoiseWrapMinus);

					double zValue = Math.Abs((zNoise.Noise(x, 0, z) * wrapTimes) % 2d - 1);

					//double zNoiseVal = xNoise.Noise(x, 0, z);
					//double zNoiseWrapTimes = zNoiseVal * wrapTimes;
					//double zNoiseWrapRemainder = zNoiseWrapTimes % 2d;
					//double zNoiseWrapMinus = zNoiseWrapRemainder - 1d;
					//double zValue = Math.Abs(zNoiseWrapMinus);

                    byte red = (byte)(xValue * 255);
					byte green = (byte)(zValue * 255);
                    bitmap.bmp.SetPixel(x, z, new SKColor(255, red, green));
                }
            }

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x_swd_noisetriangle.png");
            bitmap.Save(filepath);

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
            BitmapExternal bitmap = new BitmapExternal(size, size);

			BlockPos playerPos = args.Caller.Player.Entity.SidedPos.AsBlockPos;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vec3d windspeed = sapi.World.BlockAccessor.GetWindSpeedAt(new Vec3d(x + playerPos.X - (size / 2), sapi.World.SeaLevel, y + playerPos.Z - (size / 2)));
					windspeed.Normalize();

					SKColor color;

					if (windspeed.LengthSq() > 0.000001) 
					{
						double direction = Math.Atan2(windspeed.X, windspeed.Z) + GameMath.PI;

						float hue = (float)(direction * 360f / (GameMath.PI * 2f));

						color = SKColor.FromHsv(hue, 100f, 100f);
					}
					else
					{
						color = new SKColor(0, 0, 0);
					}

					bitmap.bmp.SetPixel(x, y, color);
                }
            }

			// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
			string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x_swd_winddir.png");
            bitmap.Save(filepath);

			return TextCommandResult.Success($"Wind map saved to {filepath}");
		}

		private TextCommandResult CommandWindMapSeries(TextCommandCallingArgs args)
		{
			float hoursPerMap = !args.Parsers[0].IsMissing ? (float)args[0] : 6;
			int mapCount = !args.Parsers[1].IsMissing ? (int)args[1] : 8;
			int size = 512;

			BlockPos playerPos = args.Caller.Player.Entity.SidedPos.AsBlockPos;

			for (int mapI = 0; mapI < mapCount; mapI++)
			{
				BitmapExternal bitmap = new BitmapExternal(size, size);

				for (int y = 0; y < size; y++)
				{
					for (int x = 0; x < size; x++)
					{
						Vec3d windspeed = sapi.World.BlockAccessor.GetWindSpeedAt(new Vec3d(x + playerPos.X - (size / 2), sapi.World.SeaLevel, y + playerPos.Z - (size / 2)));
						windspeed.Normalize();

						SKColor color;

						if (windspeed.LengthSq() > 0.000001) 
						{
							double direction = Math.Atan2(windspeed.X, windspeed.Z) + GameMath.PI;

							float hue = (float)(direction * 360f / (GameMath.PI * 2f));

							color = SKColor.FromHsv(hue, 100f, 100f);
						}
						else
						{
							color = new SKColor(0, 0, 0);
						}

						bitmap.bmp.SetPixel(x, y, color);
					}
				}

				// Ensures the file gets put into the VS directory no matter where the mod is run from (VS Code likes to hide files)
				string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"x_swd_winddirs_{mapI}.png");
				bitmap.Save(filepath);

				sapi.World.Calendar.Add(hoursPerMap);
			}

			return TextCommandResult.Success($"Wind maps saved");
		}
	}
}

#endif