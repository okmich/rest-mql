/** 
Copyright(c) <2019> <https://github.com/cyrus13>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
OR OTHER DEALINGS IN THE SOFTWARE. 
**/

using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;

namespace Cyrus.Mql5
{
    public class RestMQL
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };


        [DllExport("Get", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static string Get([In, MarshalAs(UnmanagedType.LPWStr)] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "Error|URL cannot be null or empty";
            
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string result = httpClient.GetStringAsync(new Uri(url)).GetAwaiter().GetResult();
                    return Marshal.StringToHGlobalUni(result);
                }
            }
            catch (Exception ex)
            {
                return Marshal.StringToHGlobalUni($"Error: {ex.Message}");
            }
        }

        [DllExport("Post", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static string Post([In, MarshalAs(UnmanagedType.LPWStr)] string url, [In, MarshalAs(UnmanagedType.LPWStr)] string data)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "Error|URL cannot be null or empty";

            if (data == null)
                return "Error|Data cannot be null";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                    var response = httpClient.PostAsync(new Uri(url), content).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                catch (UriFormatException ex)
                {
                    return $"Error|Invalid URL: {ex.Message}";
                }
                catch (HttpRequestException ex)
                {
                    return $"Error|HTTP Error: {ex.Message}";
                }
                catch (Exception ex)
                {
                    return $"Error|Unexpected Error: {ex.Message}";
                }
            }
        }
        
        public static void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
