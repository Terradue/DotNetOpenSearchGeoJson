//
//  ImportOptions.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;

namespace Terradue.OpenSearch.GeoJson.Import {
    public class ImportOptions {

        bool keepNamespaces = false;
        public bool KeepNamespaces {
            get {
                return keepNamespaces;
            }
            set {
                keepNamespaces = value;
            }
        }

        bool asMixed = false;
        public bool AsMixed {
            get {
                return asMixed;
            }
            set {
                asMixed = value;
            }
        }
    }
}

