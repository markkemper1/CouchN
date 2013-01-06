using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CouchN.Test
{
    [TestFixture, Explicit]
    public class FetchingDestinations
    {
        [Test]
        public void run()
        {
            WebRequest.DefaultWebProxy = null;
            ServicePointManager.DefaultConnectionLimit = 80;

            using (
                var session = TheCouch.CreateSession("cruiseme", new Uri("http://127.0.0.1:5984"))
                )
            {
                var stopWatch = new Stopwatch();

                using (var webClient = new WebClientEx())
                {
                   // webClient.DownloadString("http://127.0.0.1:5984/cruiseme/_design/destinations/_view/by_id");
                    webClient.Proxy = null;
                    stopWatch.Reset();
                    stopWatch.Start();
                    webClient.DownloadString("http://127.0.0.1:5984/cruiseme/_design/destinations/_view/by_id");
                    stopWatch.Stop();

                    Console.WriteLine("Total time taken: {0}ms to fetch raw", stopWatch.ElapsedMilliseconds);

                    stopWatch.Reset();
                    stopWatch.Start();
                    webClient.DownloadString("http://127.0.0.1:5984/cruiseme/_design/destinations/_view/by_id");
                    stopWatch.Stop();

                    Console.WriteLine("Total time taken: {0}ms to fetch raw", stopWatch.ElapsedMilliseconds);
                }

                using (var webClient = new WebClientEx())
                {
                    // webClient.DownloadString("http://127.0.0.1:5984/cruiseme/_design/destinations/_view/by_id");

                    stopWatch.Reset();
                    stopWatch.Start();
                    webClient.DownloadString("http://127.0.0.1:5984/cruiseme/_design/destinations/_view/by_id");
                    stopWatch.Stop();

                    Console.WriteLine("Total time taken: {0}ms to fetch raw", stopWatch.ElapsedMilliseconds);

                    stopWatch.Reset();
                    stopWatch.Start();
                    webClient.DownloadString("http://127.0.0.1:5984/cruiseme/_design/destinations/_view/by_id");
                    stopWatch.Stop();

                    Console.WriteLine("Total time taken: {0}ms to fetch raw", stopWatch.ElapsedMilliseconds);
                }

            }
        }

        [Test]
        public void run2()
        {

            var stopWatch1 = new Stopwatch();

            stopWatch1.Start();
            Console.WriteLine("Total time taken: {0}ms to fetch raw", stopWatch1.ElapsedMilliseconds);

            stopWatch1.Stop();

            using (
                var session = TheCouch.CreateSession("cruiseme", new Uri("http://127.0.0.1:5984"))
                )
            {
                var stopWatch = new Stopwatch();

                stopWatch.Start();

                var destinations =
                    session.Design("destinations").ViewDocs<Destination>("by_slug", new ViewQuery() ).Documents;

                stopWatch.Stop();

                Console.WriteLine("Total time taken: {0}ms to fetch {1} destinations", stopWatch.ElapsedMilliseconds,
                    destinations.Length);

                stopWatch.Reset();
                stopWatch.Start();

                var x = session.Design("destinations").ViewDocs<Destination>("by_slug", new ViewQuery()).Documents;

                stopWatch.Stop();

                Console.WriteLine("Total time taken: {0}ms to fetch {1} destinations", stopWatch.ElapsedMilliseconds,
                    destinations.Length);
            }

            using (
               var session = TheCouch.CreateSession("cruiseme", new Uri("http://localhost:5984"))
               )
            {
                var stopWatch = new Stopwatch();

                stopWatch.Start();

                var destinations =
                    session.Design("destinations").ViewDocs<Destination>("by_slug", new ViewQuery()).Documents;

                stopWatch.Stop();

                Console.WriteLine("Total time taken: {0}ms to fetch {1} destinations", stopWatch.ElapsedMilliseconds,
                    destinations.Length);

                stopWatch.Reset();
                stopWatch.Start();

                var x = session.Design("destinations").ViewDocs<Destination>("by_slug", new ViewQuery() {}).Documents;

                stopWatch.Stop();

                Console.WriteLine("Total time taken: {0}ms to fetch {1} destinations", stopWatch.ElapsedMilliseconds,
                    x.Length);
            }
        }

        class WebClientEx : WebClient
        {
            public CookieContainer CookieContainer { get; private set; }

            public WebClientEx()
            {
                CookieContainer = new CookieContainer();

                ServicePointManager.Expect100Continue = false;
                Encoding = System.Text.Encoding.UTF8;

                WebRequest.DefaultWebProxy = null;
                Proxy = null;
            }

            public void ClearCookies()
            {
                CookieContainer = new CookieContainer();
            }

            protected override WebRequest GetWebRequest(Uri address)
            {

                var request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = CookieContainer;
                }
                request.Proxy = null;
                return request;
            }
        }
    }

    public class TravelBook
    {
        public TravelBook()
        {
            Gallery = new MediaGallery();
        }

        public string Id { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public MediaGallery Gallery { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedUtc { get; set; }
    }

    public class Destination 
    {
        public Destination()
        {
            Gallery = new MediaGallery();
            TravelBooks = new TravelBook[0];
            Locations = new Location[0];
        }

        public string Id { get; set; }

        public string Name { get; set; }

        //TODO : Remove
        public string ParentDestinationName { get; set; }

        public string ParentDestinationId { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public MediaGallery Gallery { get; set; }

        public TravelBook[] TravelBooks { get; set; }

        public Location[] Locations { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedUtc { get; set; }

        public int NumberOfPorts { get; set; }

        public bool IsAParent { get; set; }

      
    }

    public class Country
    {
        public string Slug { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }

    public class Location
    {
        public Location()
        {
            Gallery = new MediaGallery();
        }

        public string Id { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Country Country { get; set; }

        public MediaGallery Gallery { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdatedUtc { get; set; }
    }
    public class MediaGallery
    {
        public MediaGallery()
        {
            Images = new List<Image>();
            Tombstones = new Uri[0];
        }

        public Uri[] Tombstones { get; set; }
        public List<Image> Images { get; set; }

        public bool IsPathAvailable(string candiate)
        {
            foreach (var path in Tombstones.Union(Images.Select(x => x.Url)))
            {
                if (path.AbsolutePath.TrimStart('/') == candiate) return false;
            }
            return true;
        }
    }


    public enum ResizeType
    {
        Max,
        Pad,
        Crop,
        Strech
    }
    public interface IImage
    {
        Uri Url { get; }

        string Title { get; }

        string Credits { get; }

        int Width { get; }

        int Height { get; }

        ResizeType ResizeType { get; }

        IImage[] Resizes { get; }
    }

    public class Image : IImage
    {
        public Image()
        {
            Resizes = new Image[0];
        }

        public Uri Url { get; set; }

        public string Title { get; set; }

        public string Credits { get; set; }

        public bool Delete { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public ResizeType ResizeType { get; set; }

        public IImage[] Resizes { get; set; }

        public Image CreateResize(Uri url, int width, int height, ResizeType resizeType)
        {
            var newItem = new Image
            {
                Credits = this.Credits,
                Height = height,
                Title = this.Title,
                Url = url,
                Width = width,
                ResizeType = resizeType
            };
            return newItem;
        }

    }
}
