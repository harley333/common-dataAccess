using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace EF.Utility.CS {

	public static class ResxReader {

		public struct ResourceEntry {
			public string Name { get; set; }
			public string Value { get; set; }
			public string Comment { get; set; }
		}

		public static List<ResourceEntry> GetEntriesFromString(string xmlContent) {
			var xml = new XmlDocument();
			xml.LoadXml(xmlContent);
			return GetEntriesFromXml(xml);
		}

		public static List<ResourceEntry> GetEntriesFromFile(string filePath) {
			List<ResourceEntry> list = new List<ResourceEntry>();

			if (File.Exists(filePath)) {
				var xml = new XmlDocument();
				xml.Load(filePath);
				list = GetEntriesFromXml(xml);
			}

			return list;
		}
		
		public static List<ResourceEntry> GetEntriesFromXml(XmlDocument xml) {
			var entries = xml.DocumentElement.SelectNodes("//data");
			var resourceEntries = new List<ResourceEntry>();

			foreach (XmlElement entryElement in entries) {
				var entry = new ResourceEntry {
					Name = MakeIntoValidIdentifier(entryElement.Attributes["name"].Value)
				};
				var valueElement = entryElement.SelectSingleNode("value");
				if (valueElement != null)
					entry.Value = valueElement.InnerText;
				var commentElement = entryElement.SelectSingleNode("comment");
				if (commentElement != null)
					entry.Comment = commentElement.InnerText;

				resourceEntries.Add(entry);
			}

			return resourceEntries;
		}

		public static string MakeIntoValidIdentifier(string arbitraryString) {
			var validIdentifier = Regex.Replace(arbitraryString, @"[^A-Za-z0-9-._]", " ");
			validIdentifier = ConvertToPascalCase(validIdentifier);
			if (Regex.IsMatch(validIdentifier, @"^\d")) validIdentifier = "_" + validIdentifier;
			return validIdentifier;
		}

		public static string ConvertToPascalCase(string phrase) {
			string[] splittedPhrase = phrase.Split(' ', '-', '.');
			var sb = new StringBuilder();

			foreach (String s in splittedPhrase) {
				char[] splittedPhraseChars = s.ToCharArray();
				if (splittedPhraseChars.Length > 0) {
					splittedPhraseChars[0] = ((new String(splittedPhraseChars[0], 1)).ToUpper().ToCharArray())[0];
				}
				sb.Append(new String(splittedPhraseChars));
			}
			return sb.ToString();
		}

		public static string ReconcileDifferences(string fileName, string newContent) {
			var oldEntries = GetEntriesFromFile(fileName);
			var newEntries = GetEntriesFromString(newContent);

			// find all the entries which are in the new file, but not in the old file
			var added = newEntries.Where(newItem => !oldEntries.Exists(oldItem => oldItem.Name == newItem.Name));
			// find all the entries which are in the old file, but not in the new file
			var deleted = oldEntries.Where(oldItem => !newEntries.Exists(newItem => oldItem.Name == newItem.Name));

			if (added.Any() || deleted.Any()) {
				oldEntries.RemoveAll(item => deleted.ToList().Exists(deletedItem => deletedItem.Name == item.Name));
				oldEntries.AddRange(added);

				var serializer = new XmlSerializer(typeof(List<ResourceEntry>));
				using (var sww = new StringWriter()) {
					using (var writer = XmlWriter.Create(sww)) {
						serializer.Serialize(writer, oldEntries);
						return sww.ToString();
					}
				}
			} else {
				return newContent;
			}
		}

	}

}
