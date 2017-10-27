using System;
using System.IO;
using Patching.Repo;

namespace ServerService {
    internal class RepoHandler {
        private const string REPOSITORY_LOCATION = @"C:\wamp\www\uksfnew\public\launcher\repos";

        public static bool Create(string name, Action<string> progress) {
            try {
                Repo repo = new Repo(Path.Combine(REPOSITORY_LOCATION, name), name, progress);
                progress.Invoke($"Creating '{repo.RepoName}' repository");
                repo.CreateRepo();
                progress.Invoke("Creation complete");
            } catch (Exception exception) {
                progress.Invoke($"Error: {exception}");
                return false;
            }
            return true;
        }

        public static bool Update(string name, Action<string> progress) {
            try {
                Repo repo = new Repo(Path.Combine(REPOSITORY_LOCATION, name), name, progress);
                progress.Invoke($"Updating '{repo.RepoName}' repository");
                repo.UpdateRepo();
                progress.Invoke("Update complete");
            } catch (Exception exception) {
                progress.Invoke($"Error: {exception}");
                return false;
            }
            return true;
        }
    }
}