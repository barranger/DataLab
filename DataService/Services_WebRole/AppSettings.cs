﻿using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ogdi.Config;

namespace Ogdi.DataServices
{
    public static class AppSettings
    {
        public static readonly CloudStorageAccount Account;

        static AppSettings()
        {
            Account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));
            RefreshAvailableEndpoints();
        }

        internal static Dictionary<string, AvailableEndpoint> RefreshAvailableEndpoints()
        {
            var ogdiConfigContext = new OgdiConfigDataServiceContext(Account.TableEndpoint.AbsoluteUri, Account.Credentials);
            var availableEndpoints = ogdiConfigContext.AvailableEndpoints.ToDictionary(item => item.alias);

            HttpContext.Current.Cache[OgdiConfigDataServiceContext.EndpointsTableName] = availableEndpoints;

            return availableEndpoints;
        }

        public static string RootServiceNamespace
        {
            get { return RoleEnvironment.GetConfigurationSettingValue("RootServiceNamespace"); }
        }

        public static string OgdiConfigTableStorageAccountName
        {
            get { return ParseFromConnectionString(ConnectionStringElement.AccountName); }
        }

        public static string OgdiConfigTableStorageAccountKey
        {
            get { return ParseFromConnectionString(ConnectionStringElement.AccountKey); }
        }

        private enum ConnectionStringElement { AccountName, AccountKey }
        private static string ParseFromConnectionString(ConnectionStringElement element)
        {
            var s = (element == ConnectionStringElement.AccountName) ? "AccountName=" : "AccountKey=";
            var cs = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");
            cs.Replace(" ", string.Empty);
            var index = cs.IndexOf(s) + s.Length;
            var length = cs.IndexOf(';', index) - index;
            return (length > 0) ? cs.Substring(index, length) : cs.Substring(index);
        }

        private static string _tableStorageBaseUrl;
        public static string TableStorageBaseUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tableStorageBaseUrl))
                {
                    _tableStorageBaseUrl = Account.TableEndpoint.AbsoluteUri;
                }
                return _tableStorageBaseUrl;
            }
        }

        private static string _blobStorageBaseUrl;
        public static string BlobStorageBaseUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_blobStorageBaseUrl))
                {
                    _blobStorageBaseUrl = Account.BlobEndpoint.AbsoluteUri;
                }
                return _blobStorageBaseUrl;
            }
        }

        public static Dictionary<string, AvailableEndpoint> EnabledStorageAccounts
        {
            get
            {
                var enabledStorageAccounts
                    = (Dictionary<string, AvailableEndpoint>)HttpContext.Current.Cache[OgdiConfigDataServiceContext.EndpointsTableName]
                    ?? RefreshAvailableEndpoints();

                return enabledStorageAccounts;
            }
        }

        public static AvailableEndpoint GetAvailableEndpointByAccountName(string accountName)
        {
            return EnabledStorageAccounts.Values.FirstOrDefault(r => r.storageaccountname == accountName);
        }
    }
}