using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ksef.pdf.generator.api.client.net
{
  public class KSeFPdfClient
  {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiToken;


    public KSeFPdfClient(string baseUrl, string apiToken, HttpClient httpClient = null)
    {
      _baseUrl = baseUrl.TrimEnd('/');
      _apiToken = apiToken;
      _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<byte[]> GetInvoicePdfAsync(string xmlContent, string ksefNumber, string qrCode)
    {
      if (string.IsNullOrWhiteSpace(xmlContent)) throw new ArgumentException(nameof(xmlContent));
      if (string.IsNullOrWhiteSpace(ksefNumber)) throw new ArgumentException(nameof(ksefNumber));
      if (string.IsNullOrWhiteSpace(qrCode)) throw new ArgumentException(nameof(qrCode));

      var endpoint = $"/api/invoice/pdf?nrKSeF={Uri.EscapeDataString(ksefNumber)}&qrCode={Uri.EscapeDataString(qrCode)}";
      var fullUrl = _baseUrl + endpoint;

      return await SendRequestAsync(fullUrl, xmlContent);
    }

    public async Task<byte[]> GetUpoPdfAsync(string xmlContent)
    {
      if (string.IsNullOrWhiteSpace(xmlContent)) throw new ArgumentException(nameof(xmlContent));

      var endpoint = "/api/upo/pdf";
      var fullUrl = _baseUrl + endpoint;

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