﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ResXResourceSetTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using KGySoft.Resources;

using NUnit.Framework;

#endregion

namespace KGySoft.CoreLibraries.UnitTests.Resources
{
    [TestFixture]
    public class ResXResourceSetTest : TestBase
    {
        #region Nested types

        #region Enumerations

        private enum TestEnum { X }

        #endregion

        #region Nested classes

        private class NonSerializableClass
        {
            #region Properties

            public int Prop { get; set; }

            #endregion

            #region Methods

            public override bool Equals(object obj) => MembersAndItemsEqual(this, obj);

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Tests whether the different kinds of objects can be deserialized.
        /// </summary>
        [Test]
        public void GetObject()
        {
            var path = Path.Combine(Files.GetExecutingPath(), "Resources\\TestResourceResX.resx");
            var rs = new ResXResourceSet(path, null);

            // string
            Assert.IsInstanceOf<string>(rs.GetObject("TestString"));
            Assert.IsInstanceOf<string>(rs.GetMetaObject("TestString"));
            Assert.AreNotEqual(rs.GetObject("TestString"), rs.GetMetaObject("TestString"));
            Assert.IsTrue(rs.GetString("MultilineString").Contains(Environment.NewLine), "MultilineString should contain the NewLine string");

            // WinForms.FileRef/string
            Assert.IsInstanceOf<string>(rs.GetObject("TestTextFile"));

            // byte array without mime
            Assert.IsInstanceOf<byte[]>(rs.GetObject("TestBytes"));

            // null stored in the compatible way
            Assert.IsNull(rs.GetObject("TestNull"));

            // no mime, parsed from string by type converter
            Assert.IsInstanceOf<Point>(rs.GetObject("TestPoint"));

            // mime, deserialized by BinaryFormatter
            Assert.IsInstanceOf<ImageListStreamer>(rs.GetObject("TestObjectEmbedded"));

            // mime, converted from byte array by type converter
            Assert.IsInstanceOf<Bitmap>(rs.GetObject("TestImageEmbedded"));

            // WinForms.FileRef/byte[]
            Assert.IsInstanceOf<byte[]>(rs.GetObject("TestBinFile"));

            // WinForms.FileRef/MemoryStream
            Assert.IsInstanceOf<MemoryStream>(rs.GetObject("TestSound"));

            // WinForms.FileRef/object created from stream
            Assert.IsInstanceOf<Bitmap>(rs.GetObject("TestImage"));
        }

        /// <summary>
        /// In safe mode, a string value can be always returned.
        /// </summary>
        [Test]
        public void GetStringSafe()
        {
            var path = Path.Combine(Files.GetExecutingPath(), "Resources\\TestResourceResX.resx");
            var rs = new ResXResourceSet(path, null) { SafeMode = true };
            object o;

            // when getting an object, result is always a ResXDataNode regardless of the object is a string
            Assert.IsInstanceOf<ResXDataNode>(o = rs.GetObject("TestString"));
            Assert.IsInstanceOf<ResXDataNode>(o = rs.GetObject("TestBytes"));

            // for a string, the string value is returned
            Assert.AreEqual("String invariant ResX", rs.GetString("TestString"));

            // for a non-string, the non-deserialized raw string value is returned
            Assert.AreEqual("576, 17", rs.GetString("TestPoint"));

            // for a file reference, the reference value is returned...
            Assert.IsTrue(rs.GetString("TestBinFile").StartsWith("TestBinFile.bin;System.Byte[], mscorlib", StringComparison.Ordinal));

            // ...even if it refers to a string...
            Assert.IsTrue(rs.GetString("TestTextFile").StartsWith("TestTextFile.txt;System.String, mscorlib", StringComparison.Ordinal));

            rs.SafeMode = false;
            o = rs.GetString("TestTextFile");
            o = rs.GetObject("TestBinFile");
            rs.SafeMode = true;

            // ...unless the string value is already obtained and cached
            Assert.IsFalse(rs.GetString("TestTextFile").StartsWith("TestTextFile.txt;System.String, mscorlib", StringComparison.Ordinal));

            // but a non-string file reference will still return the file reference even if the result is cached (with AutoCleanup, this time with full path)
            Assert.IsTrue(rs.GetString("TestBinFile").Contains("TestBinFile.bin;System.Byte[], mscorlib"));
        }

        [Test]
        public void CleanupAndRegenerate()
        {
            string path = Path.Combine(Files.GetExecutingPath(), "Resources\\TestResourceResX.resx");
            var rs = new ResXResourceSet(path, null);

            // in safe mode, raw value is expected
            rs.SafeMode = true;
            Assert.AreEqual("576, 17", rs.GetString("TestPoint"));

            // in non-safe mode, raw value is cleared once an object is generated
            rs.SafeMode = false;
            Assert.AreEqual(new Point(576, 17), rs.GetObject("TestPoint"));

            // when safe mode is turned on again, raw value is re-generated
            rs.SafeMode = true;
            Assert.AreEqual("576, 17", rs.GetString("TestPoint"));

            // for fileref, in safe mode, path/type is expected
            Assert.IsTrue(rs.GetString("TestBinFile").StartsWith("TestBinFile.bin;System.Byte[], mscorlib", StringComparison.Ordinal));

            // in non-safe mode, raw value is cleared once an object is generated
            rs.SafeMode = false;
            Assert.AreEqual(typeof(byte[]), rs.GetObject("TestBinFile").GetType());

            // when safe mode is turned on again, raw value is re-generated from fileRef
            rs.SafeMode = true;
            Assert.IsTrue(rs.GetString("TestBinFile").StartsWith("TestBinFile.bin;System.Byte[], mscorlib", StringComparison.Ordinal));
        }

        [Test]
        public void SetRemoveObject()
        {
            var path = Path.Combine(Files.GetExecutingPath(), "Resources\\TestResourceResX.resx");
            var rs = new ResXResourceSet(path, null);

            // replace
            Assert.IsTrue(rs.GetObject("TestString") is string);
            rs.SetObject("TestString", 1);
            Assert.AreEqual(1, rs.GetObject("TestString"));

            // add new
            Assert.IsNull(rs.GetObject("NotExist"));
            rs.SetObject("NotExist", 2);
            Assert.IsNotNull(rs.GetObject("NotExist"));

            // delete
            rs.RemoveObject("TestString");
            Assert.IsNull(rs.GetObject("TestString"));
            Assert.IsFalse(rs.GetEnumerator().ToEnumerable<string, object>().Any(e => e.Key == "TestString"));

            // nullifying
            rs.SetObject("NotExist", null);
            Assert.IsNull(rs.GetObject("TestString"));
            Assert.IsTrue(rs.GetEnumerator().ToEnumerable<string, object>().Any(e => e.Key.ToString() == "NotExist"));

            // save and reload
            StringBuilder sb = new StringBuilder();
            rs.Save(new StringWriter(sb));
            var rsReloaded = new ResXResourceSet(new StringReader(sb.ToString()), Path.GetDirectoryName(path));
            AssertItemsEqual(rs, rsReloaded);
        }

        [Test]
        public void SetAlias()
        {
            const string aliasName = "custom alias";
            var path = Path.Combine(Files.GetExecutingPath(), "Resources\\TestRes.resx");
            var rs = new ResXResourceSet(path, null);

            rs.SetObject("enum", TestEnum.X);
            var asmName = typeof(TestEnum).Assembly.FullName;
            rs.SetAliasValue(aliasName, asmName);

            // save with preset alias
            StringBuilder sb = new StringBuilder();
            rs.Save(new StringWriter(sb));
            var rsReloaded = new ResXResourceSet(new StringReader(sb.ToString()), Path.GetDirectoryName(path));
            Assert.AreEqual(asmName, rsReloaded.GetAliasValue(aliasName));

            // remove alias: auto generate, alias is friendly name again
            rsReloaded.RemoveAliasValue(aliasName);
            sb = new StringBuilder();
            rsReloaded.Save(new StringWriter(sb));
            rsReloaded = new ResXResourceSet(new StringReader(sb.ToString()), Path.GetDirectoryName(path));
            Assert.IsNull(rsReloaded.GetAliasValue(aliasName));
            Assert.AreEqual(asmName, rsReloaded.GetAliasValue(typeof(TestEnum).Assembly.GetName().Name));
        }

        [Test]
        public void GenerateNodeInfo()
        {
            // in safe mode NodeInfo will be generated from value on GetString
            var rs = new ResXResourceSet { SafeMode = true };

            // native types (in non-compatible mode)
            rs.SetObject("string", "string");
            rs.SetObject("DateTime", DateTime.Now);
            rs.SetObject("DateTimeOffset", DateTimeOffset.Now);
            rs.SetObject("double", -0d);
            rs.SetObject("float", -0f);
            rs.SetObject("decimal", -0.0m);
            rs.SetObject("char", 'a');
            rs.SetObject("byte", (byte)1);
            rs.SetObject("sbyte", (sbyte)1);
            rs.SetObject("short", (short)1);
            rs.SetObject("ushort", (ushort)1);
            rs.SetObject("int", 1);
            rs.SetObject("uint", 1u);
            rs.SetObject("long", 1L);
            rs.SetObject("ulong", 1ul);
            rs.SetObject("bool", true);
            rs.SetObject("DBNull", DBNull.Value);
            rs.SetObject("Type", GetType());
            rs.SetObject("IntPtr", IntPtr.Zero);
            rs.SetObject("UIntPtr", UIntPtr.Zero);

            // special handlings
            rs.SetObject("byte[]", new byte[] { 1, 2, 3 });
            rs.SetObject("CultureInfo", CultureInfo.CurrentCulture);
            rs.SetObject("null", null);

            // by type converter
            rs.SetObject("TypeConverter/string", Point.Empty);
            rs.SetObject("TypeConverter/byte[]", SystemIcons.Application.ToBitmap());

            // binary serialization
            rs.SetObject("serialized", new Collection<byte> { 1, 2, 3 });

            // getting the elements as string in safe mode will create the NodeInfos in non-compatible mode
            foreach (DictionaryEntry item in rs)
            {
                Console.WriteLine("Key: {0}; Value: {1}", item.Key, rs.GetString(item.Key.ToString()));
            }
        }

        [Test]
        public void NonSerializableObject()
        {
            var rs = new ResXResourceSet();
            rs.SetObject("x", new NonSerializableClass { Prop = 1 });

            var sb = new StringBuilder();
            rs.Save(new StringWriter(sb), true);

            var rsCheck = new ResXResourceSet(new StringReader(sb.ToString()));
            Assert.AreEqual(rs.GetObject("x"), rsCheck.GetObject("x"));

            sb = new StringBuilder();
            rs.Save(new StringWriter(sb), false);

            rsCheck = new ResXResourceSet(new StringReader(sb.ToString()));
            Assert.AreEqual(rs.GetObject("x"), rsCheck.GetObject("x"));
        }

        [Test]
        public void Save()
        {
            var path = Path.GetTempPath();
            var rs = new ResXResourceSet(basePath: path) { SafeMode = true };
            var newFile = Path.GetTempFileName();
            rs.SetObject("fileref", new ResXFileRef(newFile, typeof(string)));
            var filerefRef = ((ResXDataNode)rs.GetObject("fileref")).FileRef;
            Assert.IsTrue(Path.IsPathRooted(filerefRef.FileName));

            var sb = new StringBuilder();
            rs.Save(new StringWriter(sb));

            // path does not change in original resource set after saving
            Assert.IsTrue(Path.IsPathRooted(filerefRef.FileName));

            // if BasePath was specified, the path turns relative on saving
            var rsReloaded = new ResXResourceSet(new StringReader(sb.ToString()), path) { SafeMode = true };
            var filerefCheck = ((ResXDataNode)rsReloaded.GetObject("fileref")).FileRef;
            Assert.IsFalse(Path.IsPathRooted(filerefCheck.FileName));

            // fileref paths are adjusted if BasePath is changed, even the original relative paths
            sb = new StringBuilder();
            string newPath = Path.Combine(path, "subdir");
            rsReloaded.Save(new StringWriter(sb), basePath: newPath);
            rsReloaded = new ResXResourceSet(new StringReader(sb.ToString())) { SafeMode = true };
            var filerefCheck2 = ((ResXDataNode)rsReloaded.GetObject("fileref")).FileRef;
            Assert.AreNotEqual(filerefCheck.FileName, filerefCheck2.FileName);

            // on forced embedding the fileRefs are gone
            sb = new StringBuilder();
            rs.Save(new StringWriter(sb), forceEmbeddedResources: true);
            rsReloaded = new ResXResourceSet(new StringReader(sb.ToString())) { SafeMode = true };
            Assert.IsNull(((ResXDataNode)rsReloaded.GetObject("fileref")).FileRef);

            // creating without basePath...
            rs = new ResXResourceSet { SafeMode = true };
            rs.SetObject("filerefFull", new ResXFileRef(newFile, typeof(string)));
            rs.SetObject("filerefRelative", new ResXFileRef(Path.GetFileName(newFile), typeof(string)));

            // neither original, nor new basePath: all paths remain as it is
            sb = new StringBuilder();
            rs.Save(new StringWriter(sb));
            rsReloaded = new ResXResourceSet(new StringReader(sb.ToString())) { SafeMode = true };
            Assert.AreEqual(((ResXDataNode)rs.GetObject("filerefFull")).FileRef.FileName, ((ResXDataNode)rsReloaded.GetObject("filerefFull")).FileRef.FileName);
            Assert.AreEqual(((ResXDataNode)rs.GetObject("filerefRelative")).FileRef.FileName, ((ResXDataNode)rsReloaded.GetObject("filerefRelative")).FileRef.FileName);

            // no original basePath just a new one: the relative paths are not changed, full paths become relative
            sb = new StringBuilder();
            rs.Save(new StringWriter(sb), basePath: path);
            rsReloaded = new ResXResourceSet(new StringReader(sb.ToString())) { SafeMode = true };
            Assert.AreNotEqual(((ResXDataNode)rs.GetObject("filerefFull")).FileRef.FileName, ((ResXDataNode)rsReloaded.GetObject("filerefFull")).FileRef.FileName);
            Assert.IsFalse(Path.IsPathRooted(((ResXDataNode)rsReloaded.GetObject("filerefFull")).FileRef.FileName));
            Assert.AreEqual(((ResXDataNode)rs.GetObject("filerefRelative")).FileRef.FileName, ((ResXDataNode)rsReloaded.GetObject("filerefRelative")).FileRef.FileName);

            File.Delete(newFile);
        }

        #endregion
    }
}
