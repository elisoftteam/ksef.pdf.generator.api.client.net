using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ksef.pdf.generator.api.client.net
{
  public class KSeFPdfClient
  {
    private const string InvoicePdfUrlFormat = "{0}/api/invoice/pdf?nrKSeF={1}&qrCode={2}";
    private const string UpoPdfUrlFormat = "{0}/api/upo/pdf";

    private readonly HttpClient _httpClient;
    private readonly string _apiToken;

    public KSeFPdfClient(string apiToken, HttpClient httpClient = null)
    {
      _apiToken = apiToken;
      _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<byte[]> GetInvoicePdfAsync(string domain, string xmlContent, string ksefNumber, string qrCode)
    {
      if (string.IsNullOrWhiteSpace(domain)) throw new ArgumentException(nameof(domain));
      if (string.IsNullOrWhiteSpace(xmlContent)) throw new ArgumentException(nameof(xmlContent));
      if (string.IsNullOrWhiteSpace(ksefNumber)) throw new ArgumentException(nameof(ksefNumber));
      if (string.IsNullOrWhiteSpace(qrCode)) throw new ArgumentException(nameof(qrCode));

      domain = domain.TrimEnd('/');

      var fullUrl = string.Format(InvoicePdfUrlFormat,
          domain,
          Uri.EscapeDataString(ksefNumber),
          Uri.EscapeDataString(qrCode));

      return await SendRequestAsync(fullUrl, xmlContent);
    }

    public async Task<byte[]> GetUpoPdfAsync(string domain, string xmlContent)
    {
      if (string.IsNullOrWhiteSpace(domain)) throw new ArgumentException(nameof(domain));
      if (string.IsNullOrWhiteSpace(xmlContent)) throw new ArgumentException(nameof(xmlContent));

      domain = domain.TrimEnd('/');

      var fullUrl = string.Format(UpoPdfUrlFormat, domain);

      return await SendRequestAsync(fullUrl, xmlContent);
    }

    private async Task<byte[]> SendRequestAsync(string url, string xmlBody)
    {
      using (var content = new StringContent(xmlBody, Encoding.UTF8, "application/xml"))
      using (var request = new HttpRequestMessage(HttpMethod.Post, url))
      {
        request.Content = content;
        request.Headers.Add("x-api-token", _apiToken);

        using (var response = await _httpClient.SendAsync(request))
        {
          if (!response.IsSuccessStatusCode)
          {
            string errorMsg = "Error generating PDF.";
            try
            {
              string serverResponse = await response.Content.ReadAsStringAsync();
              if (!string.IsNullOrEmpty(serverResponse))
              {
                errorMsg += $" Server response: {serverResponse}";
              }
            }
            catch { }

            throw new HttpRequestException($"HTTP Error {response.StatusCode}: {errorMsg}");
          }

          return await response.Content.ReadAsByteArrayAsync();
        }
      }
    }
  }
}