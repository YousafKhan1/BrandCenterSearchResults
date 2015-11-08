using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Examine;
using umbraco;
using System.Globalization;
using System.IO;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Highlight;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using umbraco.cms.businesslogic.media;
using umbraco.presentation.nodeFactory;

namespace BrandCenterSearchResults
{

    public static class ExtensionMethods
    {

        private const string SearchDirectory = "ExamineIndexes";



        /// <summary>
        /// 
        /// </summary>
        /// <param name="IndexField"></param>
        /// <param name="LuceneIndex"></param>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        public static string GetHighlight(string IndexField, string LuceneIndex, string searchQuery, string highlightField)
        {
            string hightlightText = string.Empty;

            var formatter = new SimpleHTMLFormatter("<span class=\"umbSearchHighlight\">", "</span>");

            var highlighter = new Highlighter(formatter, FragmentScorer(searchQuery, highlightField, LuceneIndex));
            var tokenStream = new SnowballAnalyzer("English").TokenStream(highlightField, new StringReader(IndexField));

            string tmp = highlighter.GetBestFragments(tokenStream, IndexField, 3, "...");
            if (tmp.Length > 0)
                hightlightText = tmp + "...";

            return hightlightText;
        }

        private static QueryScorer FragmentScorer(string searchQuery, string highlightField, string Collection)
        {
            return new QueryScorer(GetLuceneQueryObject(searchQuery, highlightField).Rewrite(GetIndexSearcher(Collection).GetIndexReader()));
        }

        private static Query GetLuceneQueryObject(string q, string field)
        {

            var qt = new QueryParser(field, new SnowballAnalyzer("English"));
            return qt.Parse(q);

        }

        private static IndexSearcher GetIndexSearcher(string collectionName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_data", SearchDirectory,
                                       collectionName, "Index");
            return new IndexSearcher(path, true);
        }

    }

    public static class SearchResultExtensions
    {
        public static string FullUrl(this SearchResult sr)
        {
            return umbraco.library.NiceUrl(sr.Id);
        }
    }

    public partial class BrandCenterSearchResults : System.Web.UI.UserControl
    {
        protected const string GeneratedQuery = "+(+bodyText:{0}) +__IndexType:content";
        protected const string LuceneIndex = "BrandCenterSearch";

        protected string SearchTerm { get; private set; }

        protected IEnumerable<SearchResult> SearchResults { get; private set; }

        public BrandCenterSearchResults()
        {
            SearchTerm = string.Empty;
            SearchResults = new List<SearchResult>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SearchTerm = Request.QueryString["s"];

            if (string.IsNullOrEmpty(SearchTerm)) return;

            var criteria = ExamineManager.Instance
                    .SearchProviderCollection["BrandCenterSearchSearcher"]
                    .CreateSearchCriteria();

            // include fields to be searched
            var filter = criteria
                .GroupedOr(new string[] { "nodeName", "bodyText" , "title", "description", "hTMLContent", "copy" }, SearchTerm)
                .Not()
                .Field("umbracoNaviHide", "1")
                .Compile();

            // to limit search to one parent node, use ExamineIndex.config and add IndexParentID="" to IndexSet attribute

            // no stemming search is available

            //Searching and ordering the result by score, and we only want to get the results that has a minimum of 0.05(scale is up to 1.)
            SearchResults = ExamineManager.Instance.SearchProviderCollection["BrandCenterSearchSearcher"].Search(filter).OrderByDescending(x => x.Score).TakeWhile(x => x.Score > 0.05f);

            SearchResultListing.DataSource = SearchResults;
            SearchResultListing.DataBind();

        }

        protected string GetTitle(RepeaterItem rptItem)
        {
            string title = string.Empty;
            var sr = (SearchResult)rptItem.DataItem;

            if (sr.Fields.ContainsKey("title"))
            {
                title = sr.Fields["title"];
            }
            else
            {
                title = sr.Fields["nodeName"];
            }
            return title;
        }

        protected string GetSearchResultHighlight(RepeaterItem rptItem)
        {

            string searchHiglight = string.Empty;
            var sr = (SearchResult)rptItem.DataItem;

            if (sr.Fields.ContainsKey("hTMLContent"))
            {
                searchHiglight = ExtensionMethods.GetHighlight(sr.Fields["hTMLContent"], LuceneIndex,
                                                               string.Format(GeneratedQuery,SearchTerm), "hTMLContent");
            }
            else if (sr.Fields.ContainsKey("description"))
            {
                searchHiglight = ExtensionMethods.GetHighlight(sr.Fields["description"], LuceneIndex,
                                                               string.Format(GeneratedQuery, SearchTerm), "description");
            }
            else if (sr.Fields.ContainsKey("copy"))
            {
                searchHiglight = ExtensionMethods.GetHighlight(sr.Fields["copy"], LuceneIndex,
                                                               string.Format(GeneratedQuery, SearchTerm), "copy");
            }
            else if (sr.Fields.ContainsKey("bodyText"))
            {
                searchHiglight = ExtensionMethods.GetHighlight(sr.Fields["bodyText"], LuceneIndex,
                                                               string.Format(GeneratedQuery, SearchTerm), "bodyText");
            }

            return searchHiglight;
        }
    }
}