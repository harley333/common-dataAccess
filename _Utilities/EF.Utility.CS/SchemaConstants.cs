using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF.Utility.CS {

	public struct SchemaConstants {
		public SchemaConstants(string edmxNamespace, string csdlNamespace, string ssdlNamespace, string mslNamespace, Version minimumTemplateVersion)
			: this() {
			EdmxNamespace = edmxNamespace;
			CsdlNamespace = csdlNamespace;
			SsdlNamespace = ssdlNamespace;
			MslNamespace = mslNamespace;
			MinimumTemplateVersion = minimumTemplateVersion;
		}

		public string EdmxNamespace { get; private set; }
		public string CsdlNamespace { get; private set; }
		public string SsdlNamespace { get; private set; }
		public string MslNamespace { get; private set; }
		public Version MinimumTemplateVersion { get; private set; }
	}

}
