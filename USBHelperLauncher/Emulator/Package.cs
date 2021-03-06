﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace USBHelperLauncher.Emulator
{
    public abstract class Package
    {
        private const string Format = "{0} {1}";

        private Uri uri;
        private string name, fileName, version, installPath;
        private Dictionary<string, string> metadata = new Dictionary<string, string>();
        protected FileInfo packageFile;

        public Package(Uri uri, string name, string version)
        {
            this.uri = uri;
            this.name = name;
            this.version = version;
            this.installPath = "";
        }

        public Package(Uri uri, string name, string version, string installPath)
        {
            this.uri = uri;
            this.name = name;
            this.version = version;
            this.installPath = installPath;
        }

        public Uri GetURI()
        {
            return uri;
        }

        public string GetName()
        {
            return name;
        }

        public async Task<string> GetFileName()
        {
            if (fileName != null)
            {
                return fileName;
            }
            fileName = Path.GetFileName(uri.LocalPath);
            HttpClient client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Head, uri);
            var resp = await client.SendAsync(req);
            IEnumerable<string> headerValues;
            if (resp.Content.Headers.TryGetValues("Content-Disposition", out headerValues))
            {
                var headerValue = headerValues.FirstOrDefault();
                if (headerValue != null)
                {
                    fileName = new ContentDisposition(headerValue).FileName;
                }
            }
            return fileName;
        }

        public string GetVersion()
        {
            return version;
        }

        public string GetInstallPath()
        {
            return installPath;
        }

        public void SetMeta(string key, string value)
        {
            metadata.Add(key, value);
        }

        public string GetMeta(string key)
        {
            return metadata[key];
        }

        public Dictionary<string, string> GetMeta()
        {
            return metadata;
        }

        public async Task Download(WebClient client, string path)
        {
            string fileName = await GetFileName();
            string file = Path.Combine(path, fileName);
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2;)");
            await client.DownloadFileTaskAsync(uri, file);
            packageFile = new FileInfo(file);
        }

        public abstract Task<DirectoryInfo> Unpack();

        public override string ToString()
        {
            return String.Format(Format, name, version);
        }
    }
}
