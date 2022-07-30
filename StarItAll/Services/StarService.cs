namespace StarItAll.Services;

using Models;
using Octokit;

public class StarService
{
    private readonly GitHubClient _client = new(new ProductHeaderValue("StarItAllWeb"));
    private readonly IndexViewModel _model = new();

    public async Task<IndexViewModel> Star(string user, string token)
    {
        _client.Credentials = new Credentials(token);
        try
        {
            IReadOnlyList<Repository> repos = await _client.Repository.GetAllForUser(user);
            foreach (Repository repo in repos)
            {
                await _client.Activity.Starring.StarRepo(user, repo.Name);
                _model.Starred.Add(repo);
            }

            _model.Username = user;
            _model.ErrorMessage = string.Empty;
            _model.Loading = false;
            return _model;
        }
        catch (NotFoundException)
        {
            _model.ErrorMessage = "User not found";
            return _model;
        }
        catch (RateLimitExceededException)
        {
            _model.ErrorMessage = "Hit rate limit (5,000 requests an hour). Please try again in a bit.";
            return _model;
        }
        catch (AuthorizationException)
        {
            _model.ErrorMessage = "Not authorized";
            return _model;
        }
        catch (ArgumentNullException)
        {
            _model.ErrorMessage = "Please enter a valid username";
            return _model;
        }
        catch (Exception e)
        {
            _model.ErrorMessage += "Please report this error to kai@devrim.tech";
            _model.ErrorMessage += e.ToString();
            return _model;
        }
    }
}