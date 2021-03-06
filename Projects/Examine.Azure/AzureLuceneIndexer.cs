﻿using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Examine.Azure
{
    public abstract class AzureLuceneIndexer : LuceneIndexer, IAzureCatalogue
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzureLuceneIndexer()
        {
            AzureExtensions.EnsureAzureConfig();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        protected AzureLuceneIndexer()
            : base() { }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        protected AzureLuceneIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, analyzer, async)
        {

        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);
            this.SetOptimizationThresholdOnInit(config);
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;
        }

        /// <summary>
        /// The blob storage catalogue name to store the index in
        /// </summary>
        public string Catalogue { get; private set; }

        private Lucene.Net.Store.Directory _directory;

		[SecuritySafeCritical]
        public override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            //always return one instance.
            return _directory ?? (_directory = this.GetAzureDirectory());
        }

        public override Lucene.Net.Index.IndexWriter GetIndexWriter()
        {
            return this.GetAzureIndexWriter();
        }

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            AzureExtensions.LogExceptionFile(Name, e);
            base.OnIndexingError(e);
        }

    }
}
