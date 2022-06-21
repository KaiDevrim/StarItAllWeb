using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StarItAll.Views.Home;

using Microsoft.AspNetCore.Authentication;
using Octokit;
using Octokit.Internal;

public class IndexModel : PageModel
{
    public IReadOnlyList<Repository> Repositories { get; set; }
    public IReadOnlyList<Repository> StarredRepos { get; set; }
    public IReadOnlyList<User> Followers { get; set; }
    public IReadOnlyList<User> Following { get; set; }
    public async Task OnGetAsync()
    {
        if (User.Identity.IsAuthenticated)
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            var github = new GitHubClient(new ProductHeaderValue("AspNetCoreGitHubAuth"), new InMemoryCredentialStore(new Credentials(accessToken)));
            Repositories = await github.Repository.GetAllForCurrent();
            StarredRepos = await github.Activity.Starring.GetAllForCurrent();
            Followers = await github.User.Followers.GetAllForCurrent();
            Following = await github.User.Followers.GetAllFollowingForCurrent();
        }
    }
}