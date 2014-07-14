using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml.Linq;
using System.IO;
using System.ServiceModel.Web;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace WCFXmlService
{
    [ServiceContract]
    public interface IWCFXmlService
    {

        [OperationContract(AsyncPattern = false)]
        IEnumerable<XElement> GetXmlData(string xmlUrl);

    }

    [ServiceContract]
    public interface IPolicyRetriever
    {
        [OperationContract(AsyncPattern=false), WebGet(UriTemplate = "/clientaccesspolicy.xml")]
        Stream GetSilverlightPolicy();
    }

    public class WCFXmlService : IWCFXmlService, IPolicyRetriever
    {
        private IEnumerable<XElement> fullAPI;
        private readonly object m_lock = new object();

        public IEnumerable<XElement> GetXmlData(string xmlUrl)
        {
            WebClient client = new WebClient();
            lock (m_lock)
            {
                client.DownloadStringCompleted += RequestCompleted;
                client.DownloadStringAsync(new Uri(xmlUrl));
                Monitor.Wait(m_lock);
            }
            return fullAPI;
        }

        private void RequestCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                System.Diagnostics.Debug.WriteLine("there was no error");
                XDocument xmlDoc = XDocument.Parse(e.Result);
                fullAPI = xmlDoc.Descendants();
                lock (m_lock)
                    Monitor.Pulse(m_lock);
            }
        }

        private Stream StringToStream(string result)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }


        public Stream GetSilverlightPolicy()
        {
            string result = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <access-policy>
                <cross-domain-access>
                    <policy>
                        <allow-from http-request-headers=""*"">
                            <domain uri=""*""/>
                        </allow-from>
                        <grant-to>
                            <resource path=""/"" include-subpaths=""true""/>
                        </grant-to>
                    </policy>
                </cross-domain-access>
            </access-policy>";
            return StringToStream(result);
        }

    }
    
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:80"; 
            ServiceHost host = new ServiceHost(typeof(WCFXmlService),new Uri(baseAddress));
            try
            {
                host.AddServiceEndpoint(typeof(IPolicyRetriever), new WebHttpBinding(), "").Behaviors.Add(new WebHttpBehavior());
                host.AddServiceEndpoint(typeof(IWCFXmlService), new BasicHttpBinding(), "basic");//.Behaviors.Add(new BasicHttpBinding());
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                host.Description.Behaviors.Add(smb);
                host.Open();
                Console.WriteLine("Host is now open");
                Console.WriteLine("\nPress ENTER to close");
                Console.ReadLine();
                host.Close();
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe);
                Console.ReadLine();
            }
        }
    }
}
