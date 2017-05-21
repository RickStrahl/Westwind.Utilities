
namespace Westwind.Utilities.Test
{
	public class TestConfigurationSettings
	{
		public static string WestwindToolkitConnectionString { get; set; } =
			"server=.;database=WestwindToolkitSamples;integrated security=true;MultipleActiveResultSets=true;";
		public static string WebStoreConnectionString { get; set; } =
			"server=.;database=Webstore;integrated security=true;MultipleActiveResultSets=true;";
	}
}
