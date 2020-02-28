
namespace Westwind.Utilities.Test
{
	public class TestConfigurationSettings
	{
		public static string WestwindToolkitConnectionString { get; set; } =
			"server=.;database=WestwindToolkitSamples;integrated security=true;MultipleActiveResultSets=true;";
		public static string WebStoreConnectionString { get; set; } =
			"server=.;database=Webstore;integrated security=true;MultipleActiveResultSets=true;";

	    public static string SqliteConnectionString = "Data Source=./SupportFiles/AlbumViewerData.sqlite";

	    public static string MySqlConnectionString = "server=localhost;uid=testuser;pwd=super10seekrit;database=Localizations";

        public static string Mailserver { get; set;  } = "localhost";

        public static string MailServerUsername { get; set;  } 

        public static string MailServerPassword { get; set;  }

        public static bool MailServerUseSsl { get; set; }
    }
}
