using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v10.0", FrameworkDisplayName = ".NET 10.0")]
[assembly: AssemblyCompany("FrameLabsISLC")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyDescription("FrameLabs Background Service — ISLC, Timer Resolution, Game Detector, Process Priority Manager")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+189a7afb208a6fad64bab11d24a40fb960cae429")]
[assembly: AssemblyProduct("FrameLabsISLC")]
[assembly: AssemblyTitle("FrameLabsISLC")]
[assembly: TargetPlatform("Windows7.0")]
[assembly: SupportedOSPlatform("Windows7.0")]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
namespace FrameLabsISLC;

internal static class Program
{
	private struct MEMORYSTATUSEX
	{
		public uint dwLength;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;
	}

	private const int SystemMemoryListInformation = 80;

	private const int MemoryPurgeStandbyList = 4;

	private static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FrameLabs");

	private static readonly string ConfigFile = Path.Combine(ConfigDir, "islc_config.json");

	private static readonly string LogFile = Path.Combine(ConfigDir, "islc_log.txt");

	private static readonly string PriorityListFile = Path.Combine(ConfigDir, "priority_list.json");

	private static readonly string[] DefaultGamePatterns = new string[16]
	{
		"FiveM*", "RedM*", "GTA5*", "GTAProcess*", "RDR2*", "cs2", "VALORANT*", "RiotClientServices", "League of Legends*", "LeagueClient*",
		"FortniteClient*", "EpicGamesLauncher", "javaw", "Minecraft*", "RobloxPlayer*", "*.exe"
	};

	private static ISLCConfig _config = new ISLCConfig();

	private static bool _gameDetected = false;

	private static string _currentGame = "";

	private static int _originalTimerRes = 0;

	private static volatile bool _running = true;

	private static List<PriorityItem> _priorityList = new List<PriorityItem>();

	private static DateTime _priorityListMtime = DateTime.MinValue;

	private static readonly Dictionary<int, ProcessPriorityClass> _priorityOriginals = new Dictionary<int, ProcessPriorityClass>();

	private static readonly HashSet<int> _demotedProcessIds = new HashSet<int>();

	private static readonly HashSet<string> BackgroundProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"SearchUI", "SearchHost", "SearchApp", "SearchIndexer", "OneDrive", "OneDriveSetup", "Teams", "Slack", "Spotify", "SpotifyWebHelper",
		"MicrosoftEdgeUpdate", "GoogleUpdate", "AdobeARM", "AdobeIPCBroker", "YourPhone", "PhoneExperienceHost", "WidgetService", "Widgets", "GameBar", "GameBarPresenceWriter",
		"SecurityHealthService", "SecurityHealthSystray", "TabTip", "TextInputHost", "msedge", "chrome"
	};

	private static readonly HashSet<string> ProtectedProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"Discord", "DiscordCanary", "DiscordPTB", "DiscordDevelopment", "obs64", "obs32", "obs", "obs-browser-page", "obs-browser-helper", "obs-browser-page-64",
		"Streamlabs", "Streamlabs OBS", "streamlabs-obs", "Streamlabs Desktop", "Streamlabs.Cloudbot", "obs-streamlabs", "TikTok Studio", "TikTokStudio", "TikTokLiveStudio", "TikTok Live Studio",
		"LiveStudio", "XSplit", "XSplit.Core", "vMix", "vMix64", "NVIDIA Broadcast", "NVIDIABroadcast"
	};

	[DllImport("ntdll.dll")]
	private static extern int NtSetTimerResolution(int DesiredResolution, bool SetResolution, out int CurrentResolution);

	[DllImport("ntdll.dll")]
	private static extern int NtQueryTimerResolution(out int MinimumResolution, out int MaximumResolution, out int CurrentResolution);

	[DllImport("psapi.dll")]
	private static extern bool EmptyWorkingSet(nint hProcess);

	[DllImport("ntdll.dll")]
	private static extern int NtSetSystemInformation(int SystemInformationClass, ref int SystemInformation, int SystemInformationLength);

	private static async Task Main(string[] args)
	{
		bool createdNew;
		using (new Mutex(initiallyOwned: true, "FrameLabsISLC_SingleInstance", out createdNew))
		{
			if (!createdNew)
			{
				Log("Another instance already running. Exiting.");
				return;
			}
			Log("═══ FrameLabsISLC v1.0 started ═══");
			LoadConfig();
			SetHighTimerResolution();
			Console.CancelKeyPress += (object? s, ConsoleCancelEventArgs e) =>
			{
				e.Cancel = true;
				_running = false;
			};
			while (_running)
			{
				try
				{
					DetectGames();
					if (_gameDetected)
					{
						ApplyGamingOptimizations();
					}
					else
					{
						RestoreNormalPriorities();
					}
					LoadPriorityList();
					ApplyPriorityList();
					CheckAndPurgeStandbyRam();
				}
				catch (Exception ex)
				{
					Log("Main loop error: " + ex.Message);
				}
				await Task.Delay(_config.PollIntervalMs);
			}
			RestoreTimerResolution();
			Log("═══ FrameLabsISLC stopped ═══");
		}
	}

	private static void SetHighTimerResolution()
	{
		try
		{
			NtQueryTimerResolution(out var _, out var MaximumResolution, out _originalTimerRes);
			NtSetTimerResolution(Math.Max(5000, MaximumResolution), SetResolution: true, out var CurrentResolution);
			double value = (double)CurrentResolution / 10000.0;
			Log($"Timer Resolution set to {value:0.000}ms (requested 0.5ms, system max: {(double)MaximumResolution / 10000.0:0.000}ms)");
		}
		catch (Exception ex)
		{
			Log("Failed to set timer resolution: " + ex.Message);
		}
	}

	private static void RestoreTimerResolution()
	{
		try
		{
			if (_originalTimerRes > 0)
			{
				NtSetTimerResolution(_originalTimerRes, SetResolution: true, out var _);
				Log("Timer Resolution restored to original.");
			}
		}
		catch
		{
		}
	}

	private static void DetectGames()
	{
		bool gameDetected = _gameDetected;
		_gameDetected = false;
		_currentGame = "";
		try
		{
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				try
				{
					string processName = process.ProcessName;
					foreach (string gamePattern in _config.GamePatterns)
					{
						if (MatchesPattern(processName, gamePattern))
						{
							_gameDetected = true;
							_currentGame = processName;
							break;
						}
					}
					if (_gameDetected)
					{
						break;
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		if (_gameDetected && !gameDetected)
		{
			Log("\ud83c\udfae Game DETECTED: " + _currentGame + " — applying gaming mode");
		}
		else if (!_gameDetected & gameDetected)
		{
			Log("\ud83c\udfae Game EXITED — restoring normal priorities");
		}
	}

	private static bool MatchesPattern(string processName, string pattern)
	{
		if (pattern.EndsWith("*"))
		{
			return processName.StartsWith(pattern.TrimEnd('*'), StringComparison.OrdinalIgnoreCase);
		}
		return processName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
	}

	private static void ExcludeCpu0(Process proc)
	{
		try
		{
			int processorCount = Environment.ProcessorCount;
			if (processorCount >= 4)
			{
				long num = ((processorCount >= 64) ? (-1) : ((1L << processorCount) - 1)) & -2;
				if (proc.ProcessorAffinity != num)
				{
					proc.ProcessorAffinity = (nint)num;
				}
			}
		}
		catch
		{
		}
	}

	private static void ApplyGamingOptimizations()
	{
		try
		{
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				try
				{
					string processName = process.ProcessName;
					if (MatchesPattern(processName, _currentGame + "*") || processName.Equals(_currentGame, StringComparison.OrdinalIgnoreCase))
					{
						if (process.PriorityClass != ProcessPriorityClass.High)
						{
							process.PriorityClass = ProcessPriorityClass.High;
						}
						ExcludeCpu0(process);
					}
					else if (IsBackgroundProcess(processName) && process.PriorityClass == ProcessPriorityClass.Normal)
					{
						process.PriorityClass = ProcessPriorityClass.BelowNormal;
						_demotedProcessIds.Add(process.Id);
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private static void RestoreNormalPriorities()
	{
		if (_demotedProcessIds.Count == 0)
		{
			return;
		}
		try
		{
			int[] array = _demotedProcessIds.ToArray();
			foreach (int processId in array)
			{
				try
				{
					Process processById = Process.GetProcessById(processId);
					if (processById.PriorityClass == ProcessPriorityClass.BelowNormal)
					{
						processById.PriorityClass = ProcessPriorityClass.Normal;
					}
				}
				catch
				{
				}
			}
			Log($"\ud83d\udd04 Restored {_demotedProcessIds.Count} processes to Normal priority");
		}
		catch
		{
		}
		finally
		{
			_demotedProcessIds.Clear();
		}
	}

	private static bool IsBackgroundProcess(string name)
	{
		if (ProtectedProcesses.Contains(name))
		{
			return false;
		}
		foreach (PriorityItem priority in _priorityList)
		{
			if (priority.NameMatches(name))
			{
				return false;
			}
		}
		return BackgroundProcesses.Contains(name);
	}

	private static void CheckAndPurgeStandbyRam()
	{
		try
		{
			MEMORYSTATUSEX lpBuffer = new MEMORYSTATUSEX
			{
				dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
			};
			if (GlobalMemoryStatusEx(ref lpBuffer))
			{
				double num = (double)lpBuffer.ullAvailPhys / 1073741824.0;
				_ = (double)lpBuffer.ullTotalPhys / 1073741824.0;
				if (num < _config.PurgeThresholdGB)
				{
					PurgeStandbyList();
					Log($"\ud83e\uddf9 ISLC Purge — Free RAM was {num:0.0} GB (threshold: {_config.PurgeThresholdGB:0.0} GB)");
				}
			}
		}
		catch (Exception ex)
		{
			Log("ISLC check error: " + ex.Message);
		}
	}

	private static void PurgeStandbyList()
	{
		try
		{
			int SystemInformation = 4;
			NtSetSystemInformation(80, ref SystemInformation, 4);
		}
		catch (Exception ex)
		{
			Log("PurgeStandbyList failed: " + ex.Message);
		}
	}

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

	private static void LoadConfig()
	{
		try
		{
			if (!Directory.Exists(ConfigDir))
			{
				Directory.CreateDirectory(ConfigDir);
			}
			if (File.Exists(ConfigFile))
			{
				_config = JsonSerializer.Deserialize<ISLCConfig>(File.ReadAllText(ConfigFile)) ?? new ISLCConfig();
				Log($"Config loaded: {_config.GamePatterns.Count} game patterns, purge threshold: {_config.PurgeThresholdGB} GB");
			}
			else
			{
				_config = new ISLCConfig();
				SaveConfig();
				Log("Default config created.");
			}
		}
		catch (Exception ex)
		{
			Log("Config load error: " + ex.Message);
			_config = new ISLCConfig();
		}
	}

	private static void SaveConfig()
	{
		try
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			string contents = JsonSerializer.Serialize(_config, options);
			File.WriteAllText(ConfigFile, contents);
		}
		catch
		{
		}
	}

	private static void LoadPriorityList()
	{
		try
		{
			if (!File.Exists(PriorityListFile))
			{
				if (_priorityList.Count > 0)
				{
					_priorityList.Clear();
					Log("Priority list file removed — clearing internal state.");
				}
				_priorityListMtime = DateTime.MinValue;
				return;
			}
			DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(PriorityListFile);
			if (lastWriteTimeUtc == _priorityListMtime)
			{
				return;
			}
			string json = File.ReadAllText(PriorityListFile);
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			List<PriorityItem> list = JsonSerializer.Deserialize<PriorityListConfig>(json, options)?.Items ?? new List<PriorityItem>();
			foreach (PriorityItem item in list)
			{
				item.Canonicalize();
			}
			_priorityList = list;
			_priorityListMtime = lastWriteTimeUtc;
			Log($"Priority list updated: {_priorityList.Count} items.");
		}
		catch (Exception ex)
		{
			Log("LoadPriorityList error: " + ex.Message);
		}
	}

	private static ProcessPriorityClass ToWinPriority(string p)
	{
		if (!(p == "high"))
		{
			if (p == "below_normal")
			{
				return ProcessPriorityClass.BelowNormal;
			}
			return ProcessPriorityClass.Normal;
		}
		return ProcessPriorityClass.High;
	}

	private static void ApplyPriorityList()
	{
		if (_priorityList.Count == 0)
		{
			return;
		}
		try
		{
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				try
				{
					string processName = process.ProcessName;
					foreach (PriorityItem priority in _priorityList)
					{
						if (priority.NameMatches(processName))
						{
							ProcessPriorityClass processPriorityClass = ToWinPriority(priority.Priority);
							if (!_priorityOriginals.ContainsKey(process.Id))
							{
								_priorityOriginals[process.Id] = process.PriorityClass;
							}
							if (process.PriorityClass != processPriorityClass)
							{
								process.PriorityClass = processPriorityClass;
							}
							if (processPriorityClass == ProcessPriorityClass.High)
							{
								ExcludeCpu0(process);
							}
							break;
						}
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			Log("ApplyPriorityList error: " + ex.Message);
		}
	}

	private static void Log(string message)
	{
		string text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
		Console.WriteLine(text);
		try
		{
			if (File.Exists(LogFile))
			{
				string[] array = File.ReadAllLines(LogFile);
				if (array.Length > 500)
				{
					File.WriteAllLines(LogFile, array.Skip(array.Length - 400).ToArray());
				}
			}
			File.AppendAllText(LogFile, text + Environment.NewLine);
		}
		catch
		{
		}
	}
}
internal class ISLCConfig
{
	public List<string> GamePatterns { get; set; } = new List<string>
	{
		"FiveM*", "RedM*", "GTA5*", "GTAProcess*", "RDR2*", "cs2", "VALORANT*", "RiotClientServices", "League of Legends*", "LeagueClient*",
		"FortniteClient*", "javaw", "Minecraft*", "RobloxPlayer*", "samp", "gta_sa", "proxy_sa", "MTA*", "aces", "r5apex",
		"Overwatch*", "PUBG*", "RustClient", "ShooterGame", "DayZ*"
	};

	public double PurgeThresholdGB { get; set; } = 1.0;

	public int PollIntervalMs { get; set; } = 3000;
}
internal class PriorityListConfig
{
	public int Version { get; set; } = 1;

	public List<PriorityItem> Items { get; set; } = new List<PriorityItem>();
}
internal class PriorityItem
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("priority")]
	public string Priority { get; set; } = "normal";

	[JsonPropertyName("default")]
	public bool IsDefault { get; set; }

	[JsonIgnore]
	public string CanonicalName { get; set; } = "";

	public void Canonicalize()
	{
		string text = (Name ?? "").Trim().ToLowerInvariant();
		if (text.EndsWith(".exe"))
		{
			text = text.Substring(0, text.Length - 4);
		}
		CanonicalName = text;
	}

	public bool NameMatches(string processName)
	{
		if (string.IsNullOrEmpty(CanonicalName))
		{
			Canonicalize();
		}
		string text = (processName ?? "").Trim().ToLowerInvariant();
		if (text.EndsWith(".exe"))
		{
			text = text.Substring(0, text.Length - 4);
		}
		return string.Equals(text, CanonicalName, StringComparison.Ordinal);
	}
}
