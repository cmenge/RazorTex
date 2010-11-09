using System;
using System.Collections.Generic;
using RazorTex;

namespace Sample
{
    public class Address
    {
        public string AddressLine1 { get; set; }
        public string HouseNumber  { get; set; }
        public string Street  { get; set; }
        public string City  { get; set; }  
        public string State  { get; set; }
        public string ZipCode { get; set; }
    }

    public class ItineraryItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Itinerary
    {
        public Itinerary()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string SenderName { get; set; }
        public string RecipientFullName { get; set; }
        public string RecipientName { get; set; }

        public Address RecipientAddress { get; set; }
        public Address SenderAddress { get; set; }

        public DateTime Date { get; set; }

        public IEnumerable<ItineraryItem> Items { get; set; }
    }

    public class DivisionCalculation
    {
        private static Random _random = new System.Random();
        public double Denominator { get; set; }
        public double Numerator { get; set; }
        public double Result { get; set; }

        public static DivisionCalculation Random()
        {
            DivisionCalculation newCalc = new DivisionCalculation();
            newCalc.Denominator = (double)_random.Next() + 1.0;
            newCalc.Numerator = (double)_random.Next();
            newCalc.Result = newCalc.Numerator / newCalc.Denominator;
            return newCalc;
        }
    }

    public class SampleReport
    {
        public SampleReport()
        {
            Id = Guid.NewGuid();
            Calculations = new List<DivisionCalculation>();
        }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public List<DivisionCalculation> Calculations { get; set; }
    }

    class Program
    {
        private static readonly string _templatePath = "../../LaTeX/";

        static void Main(string[] args)
        {
            CreateSimpleReport();
        }

        static void CreateItinerary()
        {
            using (LatexRazorRenderer renderer = new RazorTex.LatexRazorRenderer())
            {
                var itinerary = CreateBogusItinerary();
               
                List<string> additionalNamespaces = new List<string>();
                additionalNamespaces.Add("Sample");
                renderer.TemplatePath = _templatePath;
                string latexString = renderer.Render(itinerary, additionalNamespaces);

                PdfRenderer pdfRenderer = new PdfRenderer();
                pdfRenderer.RenderPdf(itinerary.Id, latexString);
                Console.WriteLine("Created sample itinerary w/ id {0}", itinerary.Id);
            }
        }

        static void CreateSimpleReport()
        {
            using (LatexRazorRenderer renderer = new RazorTex.LatexRazorRenderer())
            {
                var report = CreateBogusReport();

                List<string> additionalNamespaces = new List<string>();
                additionalNamespaces.Add("Sample");
                renderer.TemplatePath = _templatePath;
                string latexString = renderer.Render(report, additionalNamespaces);

                PdfRenderer pdfRenderer = new PdfRenderer();
                pdfRenderer.RenderPdf(report.Id, latexString);
                Console.WriteLine("Created sample report w/ id {0}", report.Id);
            }
        }

        #region Bogus Content Creators
        private static Itinerary CreateBogusItinerary()
        {
            Itinerary itinerary = new Itinerary();
            itinerary.RecipientName = "Prof. März";
            itinerary.RecipientFullName = "Prof. Dr. Jan-Jürgen März van Œlesand";

            itinerary.RecipientAddress = new Address { AddressLine1 = "The White House", ZipCode = "20500", State = "DC", City = "Washington", HouseNumber = "1600", Street = "Pennsylvania Avenue NW" };
            itinerary.SenderName = "John Smith";

            itinerary.SenderAddress = new Address { AddressLine1 = "Governor's Office", City = "Sacramento", State = "CA", Street = "State Capitol Building", ZipCode = "92814" };
            itinerary.Date = DateTime.Now;

            List<ItineraryItem> itineraryItemList = new List<ItineraryItem>();
            itineraryItemList.Add(new ItineraryItem { Name = "Intro", Description = "Lorem Ipsum dolor sit amet." });
            itineraryItemList.Add(new ItineraryItem { Name = "Historical Perspective", Description = "consectetur adipiscing elit. Vestibulum sed mollis lacus. Curabitur egestas lacinia iaculis." });
            itineraryItemList.Add(new ItineraryItem { Name = "Useless Remarks", Description = "Mauris faucibus quam non leo tristique ut porta leo tincidunt. Morbi nec massa sed libero sollicitudin congue. Donec nulla sapien, porta ut pretium sit amet, laoreet ut velit." });
            itinerary.Items = itineraryItemList;
            return itinerary;
        }

        private static SampleReport CreateBogusReport()
        {
            SampleReport report = new SampleReport();
            report.Author = "UTF8-Jürgen";
            report.Title = "RazorTex Sample Report";

            for (int i = 0; i < 10; ++i)
            {
                report.Calculations.Add(DivisionCalculation.Random());
            }

            return report;
        }
        #endregion
    }
}
