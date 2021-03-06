﻿using System;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net.Config;
using NUnit.Framework;

namespace Terradue.OpenSearch.GeoJson
{

    [SetUpFixture()]
    public static class Util
    {
        public static string TestBaseDir;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestBaseDir = Path.Combine(baseDir, "../../..");
        }

    }
}

