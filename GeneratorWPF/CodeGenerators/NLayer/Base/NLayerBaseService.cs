using System.Diagnostics;
using System.IO;

namespace GeneratorWPF.CodeGenerators.NLayer.Base;

public class NLayerBaseService
{
    public string CreateSolution(string path, string projectName)
    {
        try
        {
            string solutionPath = Path.Combine(path, projectName);
            string slnPath = Path.Combine(solutionPath, $"{projectName}.sln");

            if (Directory.Exists(solutionPath) && File.Exists(slnPath))
                return "INFO: Solution already exists.";

            Directory.CreateDirectory(solutionPath);
            RunCommand(solutionPath, "dotnet", $"new sln  -n {projectName}");

            return "OK: Solution Created Successfully";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while creating the Solution. \n\t Details:{ex.Message}");
        }
    }

    private string AddFile(string folderPath, string fileName, string code)
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, code);
                return $"OK: File {fileName} added to Solution.";
            }
            else
            {
                return $"INFO: File {fileName} already exists in Solution.";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding file({fileName}) to Solution. \n\t Details:{ex.Message}");
        }
    }

    private string RunCommand(string workingDirectory, string fileName, string arguments)
    {
        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            string output = process!.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process!.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Command failed: {error}");
            }
            else
            {
                return output;
            }
        }
    }
}
