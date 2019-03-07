/*
   Copyright 2014-2019 SourceGear, LLC

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

public static class gen
{
    public const string ROOT_NAME = "SQLitePCLRaw";

    public const int MAJOR_VERSION = 2;
    public const int MINOR_VERSION = 0;
    public const int PATCH_VERSION = 0;

    // a version string with a -pre-timestamp in it
    public static string NUSPEC_VERSION_PRE_TIMESTAMP = string.Format("{0}.{1}.{2}-pre{3}",
        MAJOR_VERSION,
        MINOR_VERSION,
        PATCH_VERSION,
        DateTime.Now.ToString("yyyyMMddHHmmss")
        );

    // a version string with -pre
    public static string NUSPEC_VERSION_PRE = string.Format("{0}.{1}.{2}-pre",
        MAJOR_VERSION,
        MINOR_VERSION,
        PATCH_VERSION
        );

    // a version string for release, with no -pre
    public static string NUSPEC_VERSION_RELEASE = string.Format("{0}.{1}.{2}",
        MAJOR_VERSION,
        MINOR_VERSION,
        PATCH_VERSION
        );

    // chg this to be the version string we want, one of the above
    public static string NUSPEC_VERSION = NUSPEC_VERSION_PRE;
    public static string ASSEMBLY_VERSION = string.Format("{0}.{1}.{2}.{3}",
        MAJOR_VERSION,
        MINOR_VERSION,
        PATCH_VERSION,
        (int)((DateTime.Now - new DateTime(2018, 1, 1)).TotalDays)
        );

    private const string NUSPEC_RELEASE_NOTES = "TODO url";

    const string COPYRIGHT = "Copyright 2014-2019 SourceGear, LLC";
    const string AUTHORS = "Eric Sink";
    const string SUMMARY = "SQLitePCLRaw is a Portable Class Library (PCL) for low-level (raw) access to SQLite";
    const string PACKAGE_TAGS = "sqlite;xamarin"; // TODO
                                                  // TODO	f.WriteElementString("tags", "sqlite pcl database xamarin monotouch ios monodroid android wp8 wpa netstandard uwp");

    private static void gen_directory_build_props(string root, string nupkgs_dir_name)
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.OmitXmlDeclaration = true;

        using (XmlWriter f = XmlWriter.Create(Path.Combine(root, "Directory.Build.props"), settings))
        {
            f.WriteStartDocument();

            f.WriteStartElement("Project");
            f.WriteStartElement("PropertyGroup");

            f.WriteElementString("Copyright", COPYRIGHT);
            f.WriteElementString("Company", "SourceGear");
            f.WriteElementString("Authors", AUTHORS);
            f.WriteElementString("Version", NUSPEC_VERSION);
            f.WriteElementString("AssemblyVersion", ASSEMBLY_VERSION);
            f.WriteElementString("FileVersion", ASSEMBLY_VERSION);
            f.WriteElementString("Description", SUMMARY);
            f.WriteElementString("GenerateAssemblyProductAttribute", "false");
            f.WriteElementString("PackageLicenseExpression", "Apache-2.0");
            f.WriteElementString("PackageRequireLicenseAcceptance", "false");
            f.WriteElementString("PackageTags", PACKAGE_TAGS);
            f.WriteElementString("RepositoryUrl", "https://github.com/ericsink/SQLitePCL.raw");
            f.WriteElementString("RepositoryType", "git");
            f.WriteElementString("PackageOutputPath", string.Format("$(MSBuildThisFileDirectory){0}", nupkgs_dir_name));
            f.WriteElementString("PackageVersionForTesting", "$(Version)");

            f.WriteElementString("depversion_xunit", "2.4.1");
            f.WriteElementString("depversion_xunit_runner_visualstudio", "2.4.1");
            f.WriteElementString("depversion_microsoft_net_test_sdk", "15.0.0");

            f.WriteEndElement(); // PropertyGroup
            f.WriteEndElement(); // project

            f.WriteEndDocument();
        }
    }

    public static void Main(string[] args)
    {
        string dir_root = Path.GetFullPath(args[0]);
        var nupkgs_dir_name = "nupkgs";
        var dir_nupkgs = Path.Combine(dir_root, nupkgs_dir_name);
        var dir_nuspecs = Path.Combine(dir_root, "nuspecs");


        Directory.CreateDirectory(dir_nupkgs);
        Directory.CreateDirectory(dir_nuspecs);

        gen_directory_build_props(dir_root, nupkgs_dir_name);
    }
}
