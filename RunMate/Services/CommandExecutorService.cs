using System.Diagnostics;

namespace RunMate.Services
{
    public static class CommandExecutorService
    {
        public static string Execute(string command, string shellType)
        {
            try
            {
                switch (shellType.ToLower())
                {
                    case "pwsh":
                        return ExecuteShellCommand("pwsh", $"-NoProfile -Command \"{command}\"");
                    case "powershell":
                        return ExecuteShellCommand("powershell", $"-NoProfile -Command \"{command}\"");
                    case "cmd":
                        return ExecuteShellCommand("cmd.exe", $"/C {command}");
                    case "bash":
                        return ExecuteShellCommand("bash", $"-c \"{command}\"");
                    default:
                        return "Unsupported shell type.";
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        private static string ExecuteShellCommand(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return string.IsNullOrWhiteSpace(error) ? output : $"Error:\n{error}";
        }
    }
}
