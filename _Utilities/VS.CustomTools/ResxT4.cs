using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj80;

namespace VS.CustomTools {

	[ComVisible(true)]
	[Guid("67935D38-DC3B-4F78-AEB9-82E991C4B60A")]
	[CodeGeneratorRegistration(typeof(ResxT4), "ResxT4", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true, GeneratorRegKeyName = "ResxT4")]
	[ProvideObject(typeof(ResxT4), RegisterUsing = RegistrationMethod.CodeBase)]
	public sealed class ResxT4 : IVsSingleFileGenerator {

		int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension) {
			pbstrDefaultExtension = ".cs";
			return VSConstants.S_OK;
		}

		int IVsSingleFileGenerator.Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace,
			IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress) {

			EnvDTE.DTE dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
			Array ary = (Array)dte.ActiveSolutionProjects;
			var project = (EnvDTE.Project)ary.GetValue(0);
			var ns = project.Properties.Item("DefaultNamespace").Value.ToString();
			var projectFolder = Path.GetDirectoryName(project.FullName);
			var fileFolder = Path.GetDirectoryName(wszInputFilePath);
			if (projectFolder != fileFolder) {
				fileFolder = fileFolder.Substring(projectFolder.Length + 1);
				if (fileFolder.Length > 0) {
					ns += "." + fileFolder.Replace('\\', '.');
				}
			}

			// generate code
			var template = new Templates.ResxT4();
			string code = template.TransformText(wszInputFilePath, wszDefaultNamespace, ns);

			// send code to visual studio (visual studio will create the new file and clean-up the allocated memory)
			byte[] contents = Encoding.UTF8.GetBytes(code);
			pcbOutput = (uint)contents.Length;
			rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(contents.Length);
			Marshal.Copy(contents, 0, rgbOutputFileContents[0], contents.Length);

			return VSConstants.S_OK;
		}


	}

}
