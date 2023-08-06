using System.Reflection;

namespace UltralightNet.Platform;

public static class Resources
{
	private static Assembly Assembly => typeof(Resources).Assembly;

	public static Stream? Cacertpem => Assembly.GetManifestResourceStream("UltralightNet.resources.cacert.pem");
	public static Stream? Icudt67ldat => Assembly.GetManifestResourceStream("UltralightNet.resources.icudt67l.dat");
}
