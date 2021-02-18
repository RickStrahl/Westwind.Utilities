using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Westwind.Utilities
{
    public static class NetworkUtils
    {

        /// <summary>
        /// Retrieves a base domain name from a full domain name.
        /// For example: www.west-wind.com produces west-wind.com
        /// </summary>
        /// <param name="domainName">Dns Domain name as a string</param>
        /// <returns></returns>
        public static string GetBaseDomain(string domainName)
        {
                var tokens = domainName.Split('.');

                // only split 3 urls like www.west-wind.com
                if (tokens == null || tokens.Length != 3)
                    return domainName;

	            var tok  = new List<string>(tokens);
                var remove = tokens.Length - 2;
                tok.RemoveRange(0, remove);

                return tok[0] + "." + tok[1]; ;                                
        }
    
        /// <summary>
        /// Returns the base domain from a domain name
        /// Example: http://www.west-wind.com returns west-wind.com
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetBaseDomain(this Uri uri)
        {
            if (uri.HostNameType == UriHostNameType.Dns)            	        
                return GetBaseDomain(uri.DnsSafeHost);
            
            return uri.Host;
        }

        /// <summary>
        /// Checks to see if an IP Address or Domain is a local address
        /// </summary>
        /// <remarks>
        /// Do not use in high traffic situations, as this involves a
        /// DNS lookup. If no local hostname is found it goes out to a
        /// DNS server to retrieve IP Addresses.
        /// </remarks>
        /// <param name="hostOrIp">Hostname or IP Address</param>
        /// <returns>true or false</returns>
        public static bool IsLocalIpAddress(string hostOrIp)
        {
            if(string.IsNullOrEmpty(hostOrIp))
                return false;

            try
            {
                // get IP Mapped to passed host
                IPAddress[] hostIPs = Dns.GetHostAddresses(hostOrIp);
                
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // check host ip addresses against local ip addresses for matches
                foreach (IPAddress hostIP in hostIPs)
                {
                    // check for localhost/127.0.0.1
                    if (IPAddress.IsLoopback(hostIP)) 
                        return true;

                    // Check if IP Address matches a local IP
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) 
                            return true;
                    }
                }
            }
            catch {}

            return false;
        }

        /// <summary>
        /// Checks to see if an IP Address or Domain is a local address
        /// </summary>
        /// <remarks>
        /// Do not use in high traffic situations, as this involves a
        /// DNS lookup. If no local hostname is found it goes out to a
        /// DNS server to retrieve IP Addresses.
        /// </remarks>
        /// <param name="uri">Pass a full URL as an Uri</param>
        /// <returns></returns>
        public static bool IsLocalIpAddress(Uri uri)
        {
            if (uri == null)
                return false;

            var host = uri.Host;
            return IsLocalIpAddress(host);
        }

    }
}
