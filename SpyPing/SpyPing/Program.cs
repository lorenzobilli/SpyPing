namespace SpyPing;

public class Program
{
	private static void SplashScreen()
	{
		var asciiArt = "\n";
		asciiArt += "\t    ____              ____  _             \n";
		asciiArt += "\t   / ___| _ __  _   _|  _ \\(_)_ __   __ _ \n";
		asciiArt += "\t   \\___ \\| '_ \\| | | | |_) | | '_ \\ / _` |\n";
		asciiArt += "\t    ___) | |_) | |_| |  __/| | | | | (_| |\n";
		asciiArt += "\t   |____/| .__/ \\__, |_|   |_|_| |_|\\__, |\n";
		asciiArt += "\t         |_|    |___/               |___/ \n";
		Console.WriteLine(asciiArt + "\n\tSpyPing v.1.0 - Per terminare premere CTRL + C\n");
	}

	public static async Task Main(string[] args)
	{
		if (args.Length < 1 || args.Length > 2)
		{
			Console.WriteLine("Parametri di input non validi. Utilizzo: SpyPing.exe <indirizzo IP destinazione> <RTT massimo>");
			return;
		}

		SplashScreen();

		var exit = new ManualResetEvent(false);
		Console.CancelKeyPress += (sender, e) =>
		{
			e.Cancel = true;
			exit.Set();
		};

		try
		{
			var pinger = args.Length == 2 ? new Pinger(args[0], args[1]) : new Pinger(args[0]);
			await pinger.Start();
			exit.WaitOne();
			await pinger.Stop();
		}
		catch (ArgumentException ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
}
