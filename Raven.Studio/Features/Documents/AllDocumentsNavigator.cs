﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Raven.Abstractions.Data;
using Raven.Studio.Infrastructure;
using Raven.Studio.Models;

namespace Raven.Studio.Features.Documents
{
    public class AllDocumentsNavigator : DocumentNavigator
    {
        private readonly string id;
        private readonly int itemIndex;

        public AllDocumentsNavigator(string id, int itemIndex)
        {
            this.id = id;
            this.itemIndex = itemIndex;
        }

        public override string GetUrl()
        {
            var builder = GetBaseUrl();

            builder.SetQueryParam("id", id);
            builder.SetQueryParam("navigationMode", "allDocs");
            builder.SetQueryParam("itemIndex", itemIndex);

            return builder.BuildUrl();
        }

        public override Task<DocumentAndNavigationInfo> GetDocument()
        {
            if (string.IsNullOrEmpty(id))
            {
                return DatabaseCommands.GetDocumentsAsync(itemIndex, 1)
                    .ContinueWith(t =>
                                  new DocumentAndNavigationInfo
                                      {
                                          TotalDocuments = GetTotalDocuments(),
                                          Index = itemIndex,
                                          Document = t.Result.Length > 0 ? t.Result[0] : null
                                      }
                    );
            }
            else
            {
                return DatabaseCommands.GetAsync(id).ContinueWith(t =>
                                  new DocumentAndNavigationInfo
                                  {
                                      TotalDocuments = GetTotalDocuments(),
                                      Index = itemIndex,
                                      Document = t.Result
                                  }
                    ); ;
            }
        }

        private static long GetTotalDocuments()
        {
            return ApplicationModel.Database.Value == null
                       ? 0
                       : ApplicationModel.Database.Value.Statistics == null
                             ? 0
                             : ApplicationModel.Database.Value.Statistics.Value.CountOfDocuments;
        }

        public override string GetUrlForNext()
        {
            var builder = GetBaseUrl();

            builder.SetQueryParam("navigationMode", "allDocs");
            builder.SetQueryParam("itemIndex", itemIndex + 1);

            return builder.BuildUrl();
        }

        public override string GetUrlForPrevious()
        {
            var builder = GetBaseUrl();

            builder.SetQueryParam("navigationMode", "allDocs");
            builder.SetQueryParam("itemIndex", itemIndex - 1);

            return builder.BuildUrl();
        }

        public static DocumentNavigator AllDocumentsFromUrl(UrlParser parser)
        {
            var id = parser.GetQueryParam("id");

            int itemIndex;
            int.TryParse(parser.GetQueryParam("itemIndex"), out itemIndex);

            return new AllDocumentsNavigator(id, itemIndex);
        }
    }
}