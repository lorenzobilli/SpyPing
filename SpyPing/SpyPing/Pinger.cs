using System.Net;
using System.Net.NetworkInformation;
using Timer = System.Timers.Timer;

namespace SpyPing;

public class Pinger
{
	private readonly string _logFilename;

	private readonly string _rawIp;

	private readonly IPAddress _ip;

	private readonly int? _maxRtt;

	private readonly Timer _timer;

	private StreamWriter _fileWriter;

	private IPStatus? _currentStatus;

	public Pinger(string ip)
	{
		_rawIp = ip;
		_ip = IPAddress.TryParse(_rawIp, out var parsedIp) ? 
			parsedIp : throw new ArgumentException("Indirizzo IP non valido");
		_logFilename = _rawIp.Replace(".", "_") + ".log";
		_timer = new Timer(1000);
		_timer.Elapsed += async (sender, e) => await ExecutePing();
		_timer.AutoReset = true;
		_currentStatus = null;
	}

	public Pinger(string ip, string maxRtt) : this(ip)
	{
		_maxRtt = int.TryParse(maxRtt, out var parsedMaxRtt) ?
			parsedMaxRtt : throw new ArgumentException("RTT massimo non valido");
	}

	private async Task LogMessageAsync(string message)
	{
		var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
		var logMessage = $"{now} - {message}";
		await _fileWriter.WriteLineAsync(logMessage);
		Console.WriteLine(logMessage);
	}

	public async Task Start()
	{
		if (!File.Exists(_logFilename))
		{
			File.Create(_logFilename).Close();
		}
		_fileWriter = new StreamWriter(_logFilename, append: true);
		await LogMessageAsync($"Monitoraggio IP {_rawIp} iniziato");
		_timer.Start();
		await Task.CompletedTask;
	}

	public async Task Stop()
	{
		_timer.Stop();
		await LogMessageAsync($"Monitoraggio IP {_rawIp} terminato");
		await _fileWriter.DisposeAsync();
		await Task.CompletedTask;
	}

	private async Task ExecutePing()
	{
		using var ping = new Ping();
		var response = await ping.SendPingAsync(_ip, TimeSpan.FromSeconds(4));
		if (response.Status == IPStatus.Success)
		{
			if (_currentStatus != IPStatus.Success || response.RoundtripTime >= _maxRtt)
			{
				_currentStatus = IPStatus.Success;
				await LogMessageAsync($"Dispositivo raggiungibile: RTT {response.RoundtripTime} ms");
			}
		}
		else
		{
			if (_currentStatus != IPStatus.Unknown)
			{
				_currentStatus = IPStatus.Unknown;
				await LogMessageAsync("Dispositivo non raggiungibile");
			}
		}
	}
}
