namespace StarItAll.Models;

using Octokit;

public class IndexViewModel
{
    public IndexViewModel(IEnumerable<Repository> repositories)
    {
        Repositories = repositories;
    }

    public IEnumerable<Repository> Repositories { get; }
}