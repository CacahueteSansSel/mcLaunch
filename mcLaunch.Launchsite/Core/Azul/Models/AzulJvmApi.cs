using mcLaunch.Launchsite.Http;

namespace mcLaunch.Launchsite.Core.Azul.Models;

public static class AzulJvmApi
{
    public static async Task<AzulJvmEntry[]> GetAsync(int javaVersion, string os, string arch, string packageType = "jre",
        bool javaFx = true, int page = 1, int pageSize = 100)
    {
        string url =
            $"https://api.azul.com/metadata/v1/zulu/packages/?java_version={javaVersion}&os={os}&arch={arch}&java_package_type={packageType}" +
            $"&javafx_bundled={javaFx}&release_status=ga&availability_types=CA&certifications=tck&page={page}&page_size={pageSize}";

        AzulJvmEntry[]? entries = await Api.GetAsync<AzulJvmEntry[]>(url);
        if (entries == null || entries.Length == 0) return [];

        return entries;
    }
}